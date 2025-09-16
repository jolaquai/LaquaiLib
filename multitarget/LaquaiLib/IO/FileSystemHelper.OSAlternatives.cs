using System.Buffers;
using System.ComponentModel;
using System.Diagnostics;

using LaquaiLib.Extensions.Memory;
using LaquaiLib.UnsafeUtils;

namespace LaquaiLib.IO;

// This partial part implements significantly faster versions of the static File and Directory methods.
// It seems like File just calls directly into the OS, and copying cross-device takes AGES. Buffering through memory is in the reigns of 100x faster.
public static partial class FileSystemHelper
{
    [StackTraceHidden]
    private static void EnsureArgumentsValid(ref string source, ref string destination, bool overwrite)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(source);
        ArgumentException.ThrowIfNullOrWhiteSpace(destination);
        source = Path.GetFullPath(source);
        destination = Path.GetFullPath(destination);

        if (source == destination)
        {
            throw new ArgumentException("Source and destination are the same.");
        }

        if (!overwrite && File.Exists(destination))
        {
            throw new IOException("Destination file already exists.");
        }
    }
    /// <summary>
    /// Copies a file to a new location.
    /// </summary>
    /// <param name="source">The path of the file to copy.</param>
    /// <param name="destination">The path to the new location for the file.</param>
    /// <param name="overwrite">Whether to overwrite the destination file if it already exists.</param>
    /// <exception cref="ArgumentException">Thrown when either path is null or whitespace, or the paths resolve to the same location.</exception>
    /// <exception cref="IOException">Thrown when <paramref name="overwrite"/> is <see langword="false"/> and the destination file already exists.</exception>
    public static void CopyFile(string source, string destination, bool overwrite = false)
    {
        EnsureArgumentsValid(ref source, ref destination, overwrite);

        using var srcFs = File.OpenRead(source);
        using var destFs = File.Create(destination);

        srcFs.CopyTo(destFs);
    }
    /// <summary>
    /// Moves a file to a new location.
    /// </summary>
    /// <param name="source">The path of the file to move.</param>
    /// <param name="destination">The path to the new location for the file.</param>
    /// <param name="overwrite">Whether to overwrite the destination file if it already exists.</param>
    /// <exception cref="ArgumentException">Thrown when either path is null or whitespace, or the paths resolve to the same location.</exception>
    /// <exception cref="IOException">Thrown when <paramref name="overwrite"/> is <see langword="false"/> and the destination file already exists.</exception>
    public static void MoveFile(string source, string destination, bool overwrite = false)
    {
        CopyFile(source, destination, overwrite);

        try
        {
            File.Delete(source);
        }
        catch
        {
        }
    }
    /// <summary>
    /// Asynchronously copies a file to a new location.
    /// </summary>
    /// <param name="source">The path of the file to copy.</param>
    /// <param name="destination">The path to the new location for the file.</param>
    /// <param name="overwrite">Whether to overwrite the destination file if it already exists.</param>
    /// <param name="cancellationToken">A cancellation token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <exception cref="ArgumentException">Thrown when either path is null or whitespace, or the paths resolve to the same location.</exception>
    /// <exception cref="IOException">Thrown when <paramref name="overwrite"/> is <see langword="false"/> and the destination file already exists.</exception>
    public static async Task CopyFileAsync(string source, string destination, bool overwrite = false, CancellationToken cancellationToken = default)
    {
        EnsureArgumentsValid(ref source, ref destination, overwrite);

        var srcFs = File.OpenRead(source);
        var destFs = File.Create(destination);
        await using (srcFs.ConfigureAwait(false))
        await using (destFs.ConfigureAwait(false))
        {
            await srcFs.CopyToAsync(destFs, cancellationToken).ConfigureAwait(false);
        }
    }
    /// <summary>
    /// Asynchronously moves a file to a new location.
    /// </summary>
    /// <param name="source">The path of the file to move.</param>
    /// <param name="destination">The path to the new location for the file.</param>
    /// <param name="overwrite">Whether to overwrite the destination file if it already exists.</param>
    /// <param name="cancellationToken">A cancellation token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <exception cref="ArgumentException">Thrown when either path is null or whitespace, or the paths resolve to the same location.</exception>
    /// <exception cref="IOException">Thrown when <paramref name="overwrite"/> is <see langword="false"/> and the destination file already exists.</exception>
    public static async Task MoveFileAsync(string source, string destination, bool overwrite = false, CancellationToken cancellationToken = default)
    {
        await CopyFileAsync(source, destination, overwrite, cancellationToken).ConfigureAwait(false);

        try
        {
            File.Delete(source);
        }
        finally
        {
            File.Delete(destination);
        }
    }

    /// <summary>
    /// Using the most efficient strategy possible, copies the specified file at <paramref name="source"/> to <paramref name="destination"/>.
    /// </summary>
    /// <param name="source">The path of the file to copy.</param>
    /// <param name="destination">The path to the new location for the file.</param>
    /// <param name="overwrite">Whether to overwrite the destination file if it already exists.</param>
    /// <param name="memoryConstraint">The maximum amount of memory to use for the copy operation. If set to -1, uses default behavior (80% of total available memory).</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    public static async Task RageCopyAsync(string source, string destination, bool overwrite = false, long memoryConstraint = -1L)
    {
        ArgumentException.ThrowIfNullOrEmpty(source);
        ArgumentException.ThrowIfNullOrEmpty(destination);

        // Fast file size retrieval via P/Invoke
        if (!Kernel32.GetFileAttributesEx(source, 0, out var fileData))
        {
            throw new FileNotFoundException("Source file not found", source);
        }
        if (!overwrite && Kernel32.GetFileAttributesEx(source, 0, out _))
        {
            throw new IOException("Destination file already exists.");
        }

        var fileSize = ((long)fileData.nFileSizeHigh << 32) | fileData.nFileSizeLow;

        // Get actual sector size for this drive
        var driveLetter = Path.GetPathRoot(source);
        _ = Kernel32.GetDiskFreeSpace(driveLetter, out _, out var sectorSize, out _, out _);

        // For files that fit in your 25GB memory, use single-shot copy
        if (memoryConstraint < 0)
        {
            memoryConstraint = (long)(GC.GetGCMemoryInfo().TotalAvailableMemoryBytes * 0.8);
        }
        if (fileSize <= memoryConstraint
            && fileSize >= 1024 * 1024
            && fileSize % sectorSize == 0 // Unbuffered copy requires sector alignment
            && await Task.Run(() => TryUnbufferedCopy(source, destination)).ConfigureAwait(false)
        )
        {
            return;
        }
        // Attempt single-shot memory copy for files
        // - larger than the memory constraint (which almost definitely means it's larger than CopySingleShot can do), OR
        // - smaller than the minimum for unbuffered copy (since that would hurt performance), OR
        // - not sector aligned, OR
        // - as a fallback for unbuffered copy failure
        if (await Task.Run(() => CopySingleShot(source, destination, fileSize)).ConfigureAwait(false))
        {
            return;
        }
        // Attempt chunked memory copy for files as a fallback for SingleShotCopy
        if (await Task.Run(() => CopyChunked(source, destination, fileSize)).ConfigureAwait(false))
        {
            return;
        }
        // Final fallback is a typical streamed copy
        if (await CopyStreamed(source, destination).ConfigureAwait(false))
        {
            return;
        }

        // Last resort: let the OS handle it
        try
        {
            File.Copy(source, destination, overwrite);
        }
        catch (Exception ex)
        {
            // If we reach here, all our attempts have failed
            throw new IOException($"Failed to copy file from '{source}' to '{destination}'.", ex);
        }
    }
    private static bool TryUnbufferedCopy(string source, string dest)
    {
        var cancel = false;

        if (!Kernel32.CopyFileEx(source, dest, IntPtr.Zero, IntPtr.Zero, ref cancel, 0x1008))
        {
            var error = Marshal.GetLastWin32Error();

            // Common failure cases where we should fallback, in that order:
            // 1: Invalid function, 87: Invalid parameter, 112: Insufficient disk space, 32: Sharing violation, 5: Access denied
            switch (error)
            {
                case 1 or 87 or 112 or 32 or 5:
                    return false; // Trigger fallback
                default:
                    throw new Win32Exception(error);
            }
        }

        return true;
    }
    private static bool CopySingleShot(string source, string dest, long fileSize)
    {
        // Bail now if the file is larger than the 2GB we can contain in a Span<byte>
        if (fileSize > int.MaxValue)
        {
            return false;
        }

        // Always allocate unmanaged memory for this since it's dramatically faster than any kind of managed alloc
        var span = MemoryManager.Allocate<byte>(unchecked((int)fileSize));
        try
        {
            using var sourceFile = new FileStream(source, FileMode.Open, FileAccess.Read, FileShare.Read, 0, FileOptions.Asynchronous | FileOptions.SequentialScan);
            using var destFile = new FileStream(dest, FileMode.Create, FileAccess.Write, FileShare.None, 0, FileOptions.Asynchronous | FileOptions.WriteThrough);

            sourceFile.ReadExactly(span);
            destFile.Write(span);

            return true;
        }
        finally
        {
            MemoryManager.Free(span);
        }
    }
    private static bool CopyChunked(string source, string dest, long fileSize)
    {
        if (fileSize <= int.MaxValue)
        {
            return CopySingleShot(source, dest, fileSize);
        }

        // Always allocate unmanaged memory for this since it's dramatically faster than any kind of managed alloc
        var span = MemoryManager.Allocate<byte>(int.MaxValue);

        try
        {
            using var sourceFile = new FileStream(source, FileMode.Open, FileAccess.Read, FileShare.Read, 0, FileOptions.Asynchronous | FileOptions.SequentialScan);
            using var destFile = new FileStream(dest, FileMode.Create, FileAccess.Write, FileShare.None, 0, FileOptions.Asynchronous | FileOptions.WriteThrough);

            var remaining = fileSize;
            while (remaining > 0)
            {
                var chunkSize = (int)Math.Min(remaining, int.MaxValue);
                var chunk = span[..chunkSize];

                sourceFile.ReadExactly(chunk);
                destFile.Write(chunk);

                remaining -= chunkSize;
            }

            return true;
        }
        finally
        {
            MemoryManager.Free(span);
        }
    }
    private static async Task<bool> CopyStreamed(string source, string dest)
    {
        try
        {
            using var sourceFile = new FileStream(source, FileMode.Open, FileAccess.Read, FileShare.Read, 0, FileOptions.Asynchronous | FileOptions.SequentialScan);
            using var destFile = new FileStream(dest, FileMode.Create, FileAccess.Write, FileShare.None, 0, FileOptions.Asynchronous | FileOptions.WriteThrough);

            await sourceFile.CopyToAsync(destFile).ConfigureAwait(false);
            return true;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return false; // Trigger fallback
        }
    }
}
