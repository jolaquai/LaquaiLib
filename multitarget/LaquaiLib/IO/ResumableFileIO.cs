using System.Text.Json.Serialization;

using LaquaiLib.Core;
using LaquaiLib.Extensions;

namespace LaquaiLib.IO;

/// <summary>
/// Implements resumable operations for copying and moving files.
/// Multiple <see cref="ResumableFileIO"/> instances can coexist and be used simultaneously (assuming the state file path is not explicitly set to the same file path for multiple instances), but a single instance is not thread-safe. This is enforced and attempts to run multiple operations on the same instance simultaneously will result in an <see cref="InvalidOperationException"/>.
/// </summary>
/// <param name="stateFilePath">Optional manual override for the state file path. At the same time, this acts as a unique identifier for a specific <see cref="ResumableFileIO"/>. If not provided, a temporary file will be used.</param>
public partial class ResumableFileIO(string stateFilePath = null)
{
    private class CopyState
    {
        public string SourcePath { get; set; }
        public string DestinationPath { get; set; }
        public long BytesCopied { get; set; }
        public long TotalBytes { get; set; }
        public DateTime LastUpdated { get; set; }
    }
    [JsonSerializable(typeof(CopyState))]
    private partial class ResumableFileCopySerializerContext : JsonSerializerContext;

    private const int BufferSize = 1 << 17;
    private readonly string _stateFilePath = stateFilePath ?? AppState.AppData.File(nameof(ResumableDirectoryIO), Guid.NewGuid().ToString()).FullName;
    private volatile int running;
    private CancellationTokenSource cts;

    public Task<bool> CopyFileAsync(
        string sourcePath,
        string destinationPath,
        bool overwrite = false,
        IProgress<(long BytesCopied, long TotalBytes)> progress = null,
        CancellationToken cancellationToken = default
    )
        => MigrateFileAsync(sourcePath, destinationPath, true, overwrite, progress, cancellationToken);
    public Task<bool> MoveFileAsync(
        string sourcePath,
        string destinationPath,
        bool overwrite = false,
        IProgress<(long BytesCopied, long TotalBytes)> progress = null,
        CancellationToken cancellationToken = default
    )
        => MigrateFileAsync(sourcePath, destinationPath, false, overwrite, progress, cancellationToken);

    private async Task<bool> MigrateFileAsync(
        string sourcePath,
        string destinationPath,
        bool copy,
        bool overwrite = false,
        IProgress<(long BytesCopied, long TotalBytes)> progress = null,
        CancellationToken cancellationToken = default)
    {
        if (Interlocked.Exchange(ref running, 1) == 1)
        {
            throw new InvalidOperationException($"Another operation is already running on this instance. {nameof(ResumableFileIO)} instances are not thread-safe.");
        }

        try
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(sourcePath);
            ArgumentException.ThrowIfNullOrWhiteSpace(destinationPath);

            // Check if the source file exists
            if (!File.Exists(sourcePath))
            {
                throw new FileNotFoundException("Source file not found.", sourcePath);
            }
            if (!overwrite && File.Exists(destinationPath))
            {
                throw new IOException("Destination file already exists.");
            }

            cts = new CancellationTokenSource();

            var sourceInfo = new FileInfo(sourcePath);
            var totalBytes = sourceInfo.Length;
            var copyState = new CopyState()
            {
                SourcePath = sourcePath,
                DestinationPath = destinationPath,
                TotalBytes = totalBytes,
                BytesCopied = 0
            };

            // Check for existing state
            if (File.Exists(_stateFilePath))
            {
                try
                {
                    var savedState = await LoadStateAsync().ConfigureAwait(false);

                    // Verify the saved state matches the current operation
                    if (savedState.SourcePath == sourcePath && savedState.DestinationPath == destinationPath)
                    {
                        copyState = savedState;
                    }
                }
                catch
                {
                    // Continue with fresh copy
                }
            }

            try
            {
                var sourceStream = new FileStream(sourcePath, FileMode.Open, FileAccess.Read, FileShare.Read, BufferSize, FileOptions.SequentialScan);
                var destinationStream = new FileStream(destinationPath, copyState.BytesCopied > 0 ? FileMode.OpenOrCreate : FileMode.Create, FileAccess.Write, FileShare.None, BufferSize, FileOptions.SequentialScan);
                await using (sourceStream.ConfigureAwait(false))
                await using (destinationStream.ConfigureAwait(false))
                {
                    // Set position for resume
                    if (copyState.BytesCopied > 0)
                    {
                        sourceStream.Position = copyState.BytesCopied;
                        destinationStream.Position = copyState.BytesCopied;
                    }

                    var buffer = new byte[BufferSize];
                    int bytesRead;

                    // Copy the file in chunks
                    while ((bytesRead = await sourceStream.ReadAsync(buffer, cancellationToken).ConfigureAwait(false)) > 0)
                    {
                        await destinationStream.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken).ConfigureAwait(false);

                        copyState.BytesCopied += bytesRead;
                        copyState.LastUpdated = DateTime.UtcNow;

                        // Update state file periodically (every ~5MB)
                        if (copyState.BytesCopied % (BufferSize * 64) < BufferSize)
                        {
                            await SaveStateAsync(copyState).ConfigureAwait(false);
                        }

                        // Report progress
                        progress?.Report((copyState.BytesCopied, totalBytes));

                        // Check for cancellation
                        cancellationToken.ThrowIfCancellationRequested();
                        cts.Token.ThrowIfCancellationRequested();
                    }

                    if (!copy)
                    {
                        // Delete source file on successful move
                        File.Delete(sourcePath);
                    }

                    // Ensure final size matches
                    if (copyState.BytesCopied != totalBytes)
                    {
                        throw new IOException($"Copy failed: Expected {totalBytes} bytes but copied {copyState.BytesCopied} bytes.");
                    }

                    // Clean up state file on successful completion
                    File.Delete(_stateFilePath);
                    return true;
                }
            }
            catch (OperationCanceledException)
            {
                // Save state on cancellation
                await SaveStateAsync(copyState).ConfigureAwait(false);
                return false;
            }
            catch
            {
                // Save state on other errors
                await SaveStateAsync(copyState).ConfigureAwait(false);
                throw;
            }
        }
        finally
        {
            cts = null;
            _ = Interlocked.Exchange(ref running, 0);
        }
    }
    private async Task SaveStateAsync(CopyState state)
    {
        var json = JsonSerializer.Serialize(state, ResumableFileCopySerializerContext.Default.CopyState);
        await File.WriteAllTextAsync(_stateFilePath, json).ConfigureAwait(false);
    }
    private async Task<CopyState> LoadStateAsync()
    {
        var json = await File.ReadAllTextAsync(_stateFilePath).ConfigureAwait(false);
        return JsonSerializer.Deserialize(json, ResumableFileCopySerializerContext.Default.CopyState);
    }

    /// <summary>
    /// Cancels any ongoing operation. If no operation is running, this method is a no-op and will complete immediately.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the cancellation request. When it completes, the operation has been canceled.</returns>
    public async Task Cancel()
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