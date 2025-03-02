using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Runtime.CompilerServices;

namespace LaquaiLib.IO;

/// <summary>
/// Implements a <see cref="Partitioner{TSource}"/> that partitions files by their size.
/// </summary>
public class FileSizePartitioner : Partitioner<string>
{
    private readonly FrozenDictionary<string, FileInfo> _files;

    /// <summary>
    /// The total size of all files in the partitioner in bytes.
    /// </summary>
    public ulong TotalSize { get; }
    /// <summary>
    /// The total count of all files in the partitioner.
    /// </summary>
    public int TotalCount { get; }

    /// <summary>
    /// Initializes a new <see cref="FileSizePartitioner"/> using the specified file paths.
    /// </summary>
    /// <param name="paths">The paths to the files to partition.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public FileSizePartitioner(IEnumerable<string> paths)
    {
        _files = paths.ToFrozenDictionary(static path => path, static path => new FileInfo(path));
        TotalSize = _files.Aggregate(0ul, static (acc, pair) => acc + (ulong)pair.Value.Length);
        TotalCount = _files.Count;
    }
    /// <summary>
    /// Initializes a new <see cref="FileSizePartitioner"/> using the specified <see cref="FileInfo"/> instances.
    /// </summary>
    /// <param name="fileInfos">The <see cref="FileInfo"/> instances to partition.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public FileSizePartitioner(IEnumerable<FileInfo> fileInfos)
    {
        _files = fileInfos.ToFrozenDictionary(static fi => fi.FullName);
        TotalCount = _files.Count;
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

        var ordered = _files.OrderByDescending(static pair => pair.Value.Length);
        var partitions = new List<IEnumerator<string>>(partitionCount);

        var partitionSize = (int)Math.Ceiling((double)TotalSize / partitionCount);
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
