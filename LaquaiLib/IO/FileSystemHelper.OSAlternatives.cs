using System.Diagnostics;

namespace LaquaiLib.Util;

// This partial part implements significantly faster versions of the static File and Directory methods.
// It seems like File just calls directly into the OS, and copying cross-device takes AGES. Buffering through memory is in the reigns of 100x faster.
public partial class FileSystemHelper
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

        using (var srcFs = File.OpenRead(source))
        using (var destFs = File.Create(destination))
        {
            srcFs.CopyTo(destFs);
        }
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
}
