using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.IO;

namespace LaquaiLib.Util;

/// <summary>
/// Provides methods and events for working with files and directories.
/// </summary>
public static class FileHelper
{
    /// <summary>
    /// In parallel, migrates the contents of a directory from one location to another.
    /// </summary>
    /// <param name="source">The directory to move.</param>
    /// <param name="to">The directory that becomes the new parent directory of <paramref name="source"/>.</param>
    /// <param name="allowExisting">Whether to allow the destination directory to already exist and contain files.</param>
    /// <param name="maxDegreeOfParallelism">The maximum number of concurrent operations to allow.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests.</param>
    /// <remarks>
    /// This method uses <see cref="FileSizePartitioner"/> to create partitions for parallel processing that accounts for the size of the files in the directories.
    /// </remarks>
    public static void MigrateDirectory(
        string source,
        string to,
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
        if (source == to)
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

        Directory.CreateDirectory(newTopPath);

        foreach (var dirPath in Directory.GetDirectories(source, "*", SearchOption.AllDirectories))
        {
            var newDirPath = dirPath.Replace(source, newTopPath);
            Directory.CreateDirectory(newDirPath);
        }

        Parallel.ForEach(
            new FileSizePartitioner(Directory.GetFiles(source, "*", SearchOption.AllDirectories)),
            new ParallelOptions() { CancellationToken = cancellationToken, MaxDegreeOfParallelism = maxDegreeOfParallelism },
            newPath =>
            {
                var newFilePath = newPath.Replace(source, newTopPath);
                File.Move(newPath, newFilePath);
            }
        );
    }

    #region FileSizePartitioner
    /// <summary>
    /// Represents a <see cref="Partitioner{TSource}"/> implementation that partitions files by their size.
    /// </summary>
    public class FileSizePartitioner : Partitioner<string>
    {
        private readonly Dictionary<string, FileInfo> _files;

        /// <summary>
        /// Instantiates a new <see cref="FileSizePartitioner"/> using the specified file paths.
        /// </summary>
        /// <param name="paths">The paths to the files to partition.</param>
        public FileSizePartitioner(IEnumerable<string> paths)
        {
            _files = paths.Select(path => new KeyValuePair<string, FileInfo>(path, new FileInfo(path)))
                          .ToDictionary();
        }
        /// <summary>
        /// Instantiates a new <see cref="FileSizePartitioner"/> using the specified <see cref="FileInfo"/> instances.
        /// </summary>
        /// <param name="fileInfos">The <see cref="FileInfo"/> instances to partition.</param>
        public FileSizePartitioner(IEnumerable<FileInfo> fileInfos)
        {
            _files = fileInfos.Select(fileInfo => new KeyValuePair<string, FileInfo>(fileInfo.FullName, fileInfo))
                              .ToDictionary();
        }

        /// <summary>
        /// Partitions the file set into at most <paramref name="partitionCount"/> partitions, accounting for the size of the files.
        /// The total size of the files in each partition is approximately equal, but the number of files in each partition may vary.
        /// </summary>
        /// <param name="partitionCount">The number of partitions to create.</param>
        /// <returns>An <see cref="IList{T}"/> of <see cref="IEnumerator{T}"/> instances that represent the partitions.</returns>
        public override IList<IEnumerator<string>> GetPartitions(int partitionCount)
        {
            partitionCount--;

            var ordered = _files.OrderByDescending(pair => pair.Value.Length);
            var totalSize = ordered.Sum(pair => pair.Value.Length);
            var partitions = new List<IEnumerator<string>>(partitionCount);

            var partitionSize = (int)Math.Ceiling((double)totalSize / partitionCount);
            var currentPartition = new List<string>();
            var currentSize = 0L;

            foreach (var file in ordered)
            {
                var fileSize = file.Value.Length;
                if (currentSize + fileSize <= partitionSize)
                {
                    currentPartition.Add(file.Key);
                    currentSize += fileSize;
                }
                else
                {
                    partitions.Add(currentPartition.GetEnumerator());
                    currentPartition = [file.Key];
                    currentSize = fileSize;
                }
            }

            partitions.Add(currentPartition.GetEnumerator());

            return partitions;
        }

        /// <inheritdoc/>
        public override bool SupportsDynamicPartitions => false;
    }
    #endregion
}
