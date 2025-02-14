namespace LaquaiLib.Util;

// This partial part implements significantly faster versions of the static File and Directory methods.
public partial class FileSystemHelper
{
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

        {
            var srcFs = File.OpenRead(source);
            var destFs = File.Create(destination);
            await using (srcFs.ConfigureAwait(false))
            await using (destFs.ConfigureAwait(false))
            {
                await srcFs.CopyToAsync(destFs, cancellationToken);
            }
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

        {
            var srcFs = File.OpenRead(source);
            var destFs = File.Create(destination);
            await using (srcFs.ConfigureAwait(false))
            await using (destFs.ConfigureAwait(false))
            {
                await srcFs.CopyToAsync(destFs, cancellationToken);
            }
        }

        try
        {
            File.Delete(source);
        }
        catch
        {
            File.Delete(destination);
            throw;
        }
    }
}
