using System.Collections.Concurrent;
using System.Collections.Frozen;

namespace LaquaiLib.Util;

/// <summary>
/// Implements a <see cref="Partitioner{TSource}"/> that partitions files by their size.
/// </summary>
public class FileSizePartitioner : Partitioner<string>
{
    private readonly FrozenDictionary<string, FileInfo> _files;

    /// <summary>
    /// Initializes a new <see cref="FileSizePartitioner"/> using the specified file paths.
    /// </summary>
    /// <param name="paths">The paths to the files to partition.</param>
    public FileSizePartitioner(IEnumerable<string> paths)
    {
        _files = paths.ToFrozenDictionary(path => path, path => new FileInfo(path));
    }
    /// <summary>
    /// Initializes a new <see cref="FileSizePartitioner"/> using the specified <see cref="FileInfo"/> instances.
    /// </summary>
    /// <param name="fileInfos">The <see cref="FileInfo"/> instances to partition.</param>
    public FileSizePartitioner(IEnumerable<FileInfo> fileInfos)
    {
        _files = fileInfos.ToFrozenDictionary(fi => fi.FullName);
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
