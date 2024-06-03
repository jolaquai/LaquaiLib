namespace LaquaiLib.Util;

/// <summary>
/// Provides methods and events for working with files and directories.
/// </summary>
public static class FileSystemHelper
{
    /// <summary>
    /// In parallel, migrates the contents of a directory from one location to another.
    /// </summary>
    /// <param name="source">The directory to move.</param>
    /// <param name="to">The directory that becomes the new parent directory of <paramref name="source"/>.</param>
    /// <param name="copy">Replicate the directory and its contents at the source location instead of moving it.</param>
    /// <param name="allowExisting">Whether to allow the destination directory to already exist and contain files, and whether to overwrite existing files when <paramref name="copy"/> is <see langword="true"/>.</param>
    /// <param name="maxDegreeOfParallelism">The maximum number of concurrent operations to allow.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests.</param>
    /// <remarks>
    /// This method uses <see cref="FileSizePartitioner"/> to create partitions for parallel processing that accounts for the size of the files in the directories.
    /// </remarks>
    public static void MigrateDirectory(
        string source,
        string to,
        bool copy = false,
        bool allowExisting = false,
        int maxDegreeOfParallelism = -1,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(to);
        if (maxDegreeOfParallelism < 0)
        {
            maxDegreeOfParallelism = Environment.ProcessorCount;
        }

        var newTopPath = Path.Combine(to, Path.GetFileName(source));

        if (!Directory.Exists(source))
        {
            throw new ArgumentException($"Directory {source} does not exist.", nameof(source));
        }
        if (!Directory.Exists(to))
        {
            throw new ArgumentException($"Directory {to} does not exist.", nameof(to));
        }
        if (!allowExisting && Directory.Exists(newTopPath))
        {
            throw new ArgumentException($"Directory {newTopPath} already exists.", nameof(to));
        }
        if (source.Equals(to, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Source and destination are the same.");
        }
        if (source.Contains(to, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Destination is a subdirectory of the source.", nameof(to));
        }
        if (to.Contains(source, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Source is a subdirectory of the destination.", nameof(source));
        }

        _ = Directory.CreateDirectory(newTopPath);

        foreach (var dirPath in Directory.GetDirectories(source, "*", SearchOption.AllDirectories))
        {
            var newDirPath = dirPath.Replace(source, newTopPath);
            _ = Directory.CreateDirectory(newDirPath);
        }

        var partitioner = new FileSizePartitioner(Directory.GetFiles(source, "*", SearchOption.AllDirectories));
        var parallelOptions = new ParallelOptions()
        {
            CancellationToken = cancellationToken,
            MaxDegreeOfParallelism = maxDegreeOfParallelism
        };

        _ = copy
            ? Parallel.ForEach(
                partitioner,
                parallelOptions,
                newPath =>
                {
                    var newFilePath = newPath.Replace(source, newTopPath);
                    File.Copy(newPath, newFilePath, allowExisting);
                }
            )
            : Parallel.ForEach(
                partitioner,
                parallelOptions,
                newPath =>
                {
                    var newFilePath = newPath.Replace(source, newTopPath);
                    File.Move(newPath, newFilePath);
                }
            );
    }
}
