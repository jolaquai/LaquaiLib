using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;

using LaquaiLib.Core;
using LaquaiLib.Extensions;

namespace LaquaiLib.IO;

/// <summary>
/// Implements resumable operations for copying and moving directories.
/// Multiple <see cref="ResumableDirectoryIO"/> instances can coexist and be used simultaneously (assuming the state file path is not explicitly set to the same file path for multiple instances), but a single instance is not thread-safe. This is enforced and attempts to run multiple operations on the same instance simultaneously will result in an <see cref="InvalidOperationException"/>.
/// </summary>
/// <param name="stateFilePath">Optional manual override for the state file path. At the same time, this acts as a unique identifier for a specific <see cref="ResumableDirectoryIO"/>. If not provided, a temporary file will be used.</param>
public partial class ResumableDirectoryIO(string stateFilePath = null)
{
    private class DirectoryCopyState
    {
        public string SourcePath { get; set; }
        public string DestinationPath { get; set; }
        public List<FileCopyState> PendingFiles { get; set; } = [];
        public List<string> CompletedFiles { get; set; } = [];
        public DateTime LastUpdated { get; set; }
    }
    private class FileCopyState
    {
        public string RelativePath { get; set; }
        public long BytesCopied { get; set; }
        public long TotalBytes { get; set; }
        public string FileHash { get; set; }
    }
    [JsonSerializable(typeof(FileCopyState))]
    [JsonSerializable(typeof(DirectoryCopyState))]
    private partial class ResumableDirectoryCopySerializerContext : JsonSerializerContext;

    private const int BufferSize = 1 << 17;
    private readonly string _stateFilePath = stateFilePath ?? AppState.AppData.File(nameof(ResumableDirectoryIO), Guid.NewGuid().ToString()).FullName;
    private volatile int running;
    private CancellationTokenSource cts;

    /// <summary>
    /// Copies a directory from the source path to the destination path.
    /// </summary>
    /// <param name="sourcePath">The source directory path.</param>
    /// <param name="destinationPath">The destination directory path.</param>
    /// <param name="overwrite">Whether to allow overwriting existing files in the destination directory.</param>
    /// <param name="preserveTimestamps">Whether to preserve the original file timestamps.</param>
    /// <param name="verifyFiles">Whether to verify the copied files by comparing hashes.</param>
    /// <param name="progressSink">An <see cref="IProgress{T}"/> implementation to report progress.</param>
    /// <param name="cancellationToken">An optional cancellation token.</param>
    /// <returns>A task that represents the asynchronous copy operation.</returns>
    public Task<bool> CopyDirectoryAsync(
        string sourcePath,
        string destinationPath,
        bool overwrite = false,
        bool preserveTimestamps = true,
        bool verifyFiles = true,
        IProgress<(int FilesCompleted, int TotalFiles, long BytesCopied, long TotalBytes, string CurrentFile)> progressSink = null,
        CancellationToken cancellationToken = default
    )
        => MigrateDirectoryAsync(sourcePath, destinationPath, true, overwrite, preserveTimestamps, verifyFiles, progressSink, cancellationToken);
    /// <summary>
    /// Moves a directory from the source path to the destination path.
    /// </summary>
    /// <param name="sourcePath">The source directory path.</param>
    /// <param name="destinationPath">The destination directory path.</param>
    /// <param name="overwrite">Whether to allow overwriting existing files in the destination directory.</param>
    /// <param name="preserveTimestamps">Whether to preserve the original file timestamps.</param>
    /// <param name="verifyFiles">Whether to verify the copied files by comparing hashes.</param>
    /// <param name="progressSink">An <see cref="IProgress{T}"/> implementation to report progress.</param>
    /// <param name="cancellationToken">An optional cancellation token.</param>
    /// <returns>A task that represents the asynchronous move operation.</returns>
    public Task<bool> MoveDirectoryAsync(
        string sourcePath,
        string destinationPath,
        bool overwrite = false,
        bool preserveTimestamps = true,
        bool verifyFiles = true,
        IProgress<(int FilesCompleted, int TotalFiles, long BytesCopied, long TotalBytes, string CurrentFile)> progressSink = null,
        CancellationToken cancellationToken = default
    )
        => MigrateDirectoryAsync(sourcePath, destinationPath, true, overwrite, preserveTimestamps, verifyFiles, progressSink, cancellationToken);

    private async Task<bool> MigrateDirectoryAsync(
        string sourcePath,
        string destinationPath,
        bool copy = false,
        bool overwrite = false,
        bool preserveTimestamps = true,
        bool verifyFiles = true,
        IProgress<(int FilesCompleted, int TotalFiles, long BytesCopied, long TotalBytes, string CurrentFile)> progress = null,
        CancellationToken cancellationToken = default)
    {
        if (Interlocked.Exchange(ref running, 1) == 1)
        {
            throw new InvalidOperationException($"Another operation is already running on this instance. {nameof(ResumableDirectoryIO)} instances are not thread-safe.");
        }

        try
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(sourcePath);
            ArgumentException.ThrowIfNullOrWhiteSpace(destinationPath);

            if (!Directory.Exists(sourcePath))
            {
                throw new DirectoryNotFoundException($"Source directory not found: '{sourcePath}'.");
            }
            if (!overwrite && Directory.Exists(destinationPath))
            {
                throw new IOException($"Destination directory already exists: '{destinationPath}'.");
            }

            cts = new CancellationTokenSource();

            // Create destination directory
            Directory.CreateDirectory(destinationPath);

            DirectoryCopyState copyState;
            var isResume = false;

            // Check for existing state
            if (File.Exists(_stateFilePath))
            {
                try
                {
                    copyState = await LoadStateAsync().ConfigureAwait(false);

                    // Verify the saved state matches the current operation
                    if (copyState.SourcePath == sourcePath && copyState.DestinationPath == destinationPath)
                    {
                        isResume = true;
                    }
                    else
                    {
                        copyState = CreateNewCopyState(sourcePath, destinationPath);
                    }
                }
                catch
                {
                    copyState = CreateNewCopyState(sourcePath, destinationPath);
                }
            }
            else
            {
                copyState = CreateNewCopyState(sourcePath, destinationPath);
            }

            // If not resuming, scan the directory and build the file list
            if (!isResume)
            {
                await ScanDirectoryAsync(copyState, sourcePath, preserveTimestamps, cancellationToken).ConfigureAwait(false);
            }

            // Copy progress tracking
            var totalFiles = copyState.PendingFiles.Count + copyState.CompletedFiles.Count;
            var totalBytes = copyState.PendingFiles.Sum(f => f.TotalBytes) +
                            (isResume ? copyState.CompletedFiles.Count * 1 : 0); // Completed files are counted as 1 byte each
            long bytesCopied = copyState.CompletedFiles.Count > 0 ? copyState.CompletedFiles.Count : 0;

            try
            {
                // Process each pending file
                for (var i = 0; i < copyState.PendingFiles.Count; i++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    cts.Token.ThrowIfCancellationRequested();

                    var fileState = copyState.PendingFiles[i];
                    var sourceFilePath = Path.Combine(sourcePath, fileState.RelativePath);
                    var destFilePath = Path.Combine(destinationPath, fileState.RelativePath);

                    // Ensure destination directory exists
                    Directory.CreateDirectory(Path.GetDirectoryName(destFilePath));

                    // Copy the individual file
                    var fileSuccess = await MigrateFileAsync(
                        sourceFilePath,
                        destFilePath,
                        copy,
                        fileState,
                        preserveTimestamps,
                        verifyFiles,
                        progress: new Progress<(long BytesCopied, long TotalBytes)>(p =>
                        {
                            var currentBytesCopied = bytesCopied + p.BytesCopied - fileState.BytesCopied;
                            progress?.Report((
                                copyState.CompletedFiles.Count,
                                totalFiles,
                                currentBytesCopied,
                                totalBytes,
                                fileState.RelativePath
                            ));
                        }),
                        cancellationToken).ConfigureAwait(false);

                    if (fileSuccess)
                    {
                        // Move file from pending to completed
                        copyState.CompletedFiles.Add(fileState.RelativePath);
                        bytesCopied += fileState.TotalBytes;

                        // Remove this file from the pending list
                        copyState.PendingFiles.RemoveAt(i);
                        i--; // Adjust index since we removed an item

                        // Update the state periodically
                        copyState.LastUpdated = DateTime.UtcNow;
                        await SaveStateAsync(copyState).ConfigureAwait(false);

                        // Report overall progress
                        progress?.Report((
                            copyState.CompletedFiles.Count,
                            totalFiles,
                            bytesCopied,
                            totalBytes,
                            "Completed: " + fileState.RelativePath
                        ));
                    }
                    else
                    {
                        // File copy failed or was canceled
                        // We've already updated the file state, so just save and exit
                        await SaveStateAsync(copyState).ConfigureAwait(false);
                        return false;
                    }
                }

                if (!copy)
                {
                    // Delete source file on successful move
                    Directory.Delete(sourcePath, true);
                }

                // Clean up state file on successful completion
                if (File.Exists(_stateFilePath))
                {
                    File.Delete(_stateFilePath);
                }

                return true;
            }
            catch (OperationCanceledException)
            {
                // Save state on cancellation
                copyState.LastUpdated = DateTime.UtcNow;
                await SaveStateAsync(copyState).ConfigureAwait(false);
                return false;
            }
            catch (Exception)
            {
                // Save state on other errors
                copyState.LastUpdated = DateTime.UtcNow;
                await SaveStateAsync(copyState).ConfigureAwait(false);
                throw;
            }
        }
        finally
        {
            cts = null;
            Interlocked.Exchange(ref running, 0);
        }
    }
    private async Task<bool> MigrateFileAsync(
        string sourceFilePath,
        string destFilePath,
        bool copy,
        FileCopyState fileState,
        bool preserveTimestamps,
        bool verifyFiles,
        IProgress<(long BytesCopied, long TotalBytes)> progress = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var sourceStream = new FileStream(sourceFilePath, FileMode.Open, FileAccess.Read, FileShare.Read, BufferSize, FileOptions.SequentialScan);
            using var destStream = new FileStream(destFilePath, fileState.BytesCopied > 0 ? FileMode.OpenOrCreate : FileMode.Create, FileAccess.Write, FileShare.None, BufferSize, FileOptions.SequentialScan);

            // Set position for resume
            if (fileState.BytesCopied > 0)
            {
                sourceStream.Position = fileState.BytesCopied;
                destStream.Position = fileState.BytesCopied;
            }

            var buffer = new byte[BufferSize];
            int bytesRead;

            // Copy the file in chunks
            while ((bytesRead = await sourceStream.ReadAsync(buffer, cancellationToken).ConfigureAwait(false)) > 0)
            {
                await destStream.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken).ConfigureAwait(false);
                fileState.BytesCopied += bytesRead;

                // Report progress
                progress?.Report((fileState.BytesCopied, fileState.TotalBytes));

                // Check for cancellation
                cancellationToken.ThrowIfCancellationRequested();
                cts.Token.ThrowIfCancellationRequested();
            }

            // Verify final size matches
            if (fileState.BytesCopied != fileState.TotalBytes)
            {
                throw new IOException($"Copy failed: Expected {fileState.TotalBytes} bytes but copied {fileState.BytesCopied} bytes.");
            }

            // Verify file hash if requested
            if (verifyFiles && !string.IsNullOrEmpty(fileState.FileHash))
            {
                var hash = await ComputeFileHashAsync(destFilePath, cancellationToken).ConfigureAwait(false);
                if (hash != fileState.FileHash)
                {
                    throw new IOException($"File verification failed: Hash mismatch for '{destFilePath}'.");
                }
            }

            // Set file timestamps if requested
            if (preserveTimestamps)
            {
                File.SetCreationTime(destFilePath, File.GetCreationTime(sourceFilePath));
                File.SetLastWriteTime(destFilePath, File.GetLastWriteTime(sourceFilePath));
                File.SetLastAccessTime(destFilePath, File.GetLastAccessTime(sourceFilePath));
            }

            if (!copy)
            {
                // Delete source file on successful move
                File.Delete(sourceFilePath);
            }

            return true;
        }
        catch (OperationCanceledException)
        {
            // Pass cancellation up
            throw;
        }
        catch
        {
            return false;
        }
    }

    private async Task ScanDirectoryAsync(DirectoryCopyState copyState, string sourcePath, bool computeHashes = true, CancellationToken cancellationToken = default)
    {
        var allFiles = Directory.GetFiles(sourcePath, "*", SearchOption.AllDirectories);

        // Create pending file entries
        for (var i = 0; i < allFiles.Length; i++)
        {
            var filePath = allFiles[i];
            cancellationToken.ThrowIfCancellationRequested();
            cts.Token.ThrowIfCancellationRequested();

            var fileInfo = new FileInfo(filePath);
            var relativePath = Path.GetRelativePath(sourcePath, filePath);

            var fileState = new FileCopyState
            {
                RelativePath = relativePath,
                BytesCopied = 0,
                TotalBytes = fileInfo.Length
            };

            // Compute hash for verification if requested
            if (computeHashes)
            {
                fileState.FileHash = await ComputeFileHashAsync(filePath, cancellationToken).ConfigureAwait(false);
            }

            copyState.PendingFiles.Add(fileState);
        }
    }
    private static async Task<string> ComputeFileHashAsync(string filePath, CancellationToken cancellationToken)
    {
        using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, BufferSize, true);
        var hashBytes = await SHA256.HashDataAsync(stream, cancellationToken).ConfigureAwait(false);
        return Convert.ToHexStringLower(hashBytes);
    }
    private static DirectoryCopyState CreateNewCopyState(string sourcePath, string destinationPath)
    {
        return new DirectoryCopyState
        {
            SourcePath = sourcePath,
            DestinationPath = destinationPath,
            PendingFiles = [],
            CompletedFiles = [],
            LastUpdated = DateTime.UtcNow
        };
    }
    private async Task SaveStateAsync(DirectoryCopyState state)
    {
        var json = JsonSerializer.Serialize(state, ResumableDirectoryCopySerializerContext.Default.DirectoryCopyState);
        await File.WriteAllTextAsync(_stateFilePath, json).ConfigureAwait(false);
    }
    private async Task<DirectoryCopyState> LoadStateAsync()
    {
        var json = await File.ReadAllTextAsync(_stateFilePath).ConfigureAwait(false);
        return JsonSerializer.Deserialize(json, ResumableDirectoryCopySerializerContext.Default.DirectoryCopyState);
    }

    /// <summary>
    /// Cancels any ongoing operation. If no operation is running, this method is a no-op and will complete immediately.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the cancellation request. When it completes, the operation has been canceled.</returns>
    public async Task CancelAsync()
    {
        if (cts is not null && !cts.IsCancellationRequested && running == 1)
        {
            cts.Cancel();
            // wait until running becomes 0, longer on each iteration
            for (var i = 10; running == 1; i *= 2)
            {
                await Task.Delay(i).ConfigureAwait(false);
            }
        }
    }
}
