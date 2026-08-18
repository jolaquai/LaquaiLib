using System.Collections.Frozen;
using System.Diagnostics;
using System.Buffers;

namespace LaquaiLib.IO;

/// <summary>
/// Implements a <see cref="Partitioner{TSource}"/> that partitions files by their size.
/// </summary>
/// <remarks>
/// This type cannot be used as a partitioner for <see cref="Parallel.ForEach{TSource}(Partitioner{TSource}, Action{TSource})"/> or similar calls because it does not support dynamic partitions. Instead, use <see cref="GetPartitions"/> to get the partitions and iterate over them manually.
/// </remarks>
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
    public FileSizePartitioner(IEnumerable<FileInfo> fileInfos)
    {
        _files = fileInfos.ToFrozenDictionary(static fi => fi.FullName);
        TotalSize = _files.Aggregate(0ul, static (acc, pair) => acc + (ulong)pair.Value.Length);
        TotalCount = _files.Count;
    }
    /// <summary>
    /// Initializes a new <see cref="FileSizePartitioner"/> using the files in the specified directory.
    /// </summary>
    /// <param name="directoryInfo">The directory to partition.</param>
    /// <param name="enumerationOptions">An optional <see cref="EnumerationOptions"/> instance that specifies how to enumerate the files in the directory.</param>
    public FileSizePartitioner(DirectoryInfo directoryInfo, EnumerationOptions enumerationOptions = null)
    {
        enumerationOptions ??= new EnumerationOptions()
        {
            RecurseSubdirectories = false,
            ReturnSpecialDirectories = false,
            AttributesToSkip = FileAttributes.Hidden | FileAttributes.System,
            IgnoreInaccessible = true,
        };

        _files = directoryInfo.EnumerateFiles("*", enumerationOptions).ToFrozenDictionary(static fi => fi.FullName);
        TotalSize = _files.Aggregate(0ul, static (acc, pair) => acc + (ulong)pair.Value.Length);
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
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(partitionCount);

        var ordered = _files.OrderByDescending(static pair => pair.Value.Length);

        // Largest-processing-time greedy bin packing: walk the files largest-first and place each into the
        // bucket that currently holds the least data. This yields exactly partitionCount buckets (some may be
        // empty when there are fewer files than partitions) with approximately equal total sizes.
        var buckets = new List<string>[partitionCount];
        var sizesBuf = ArrayPool<long>.Shared.Rent(partitionCount);
        try
        {
            var sizes = sizesBuf.AsSpan(0, partitionCount);

            for (var i = 0; i < partitionCount; i++)
                buckets[i] = [];

            foreach (var (k, v) in ordered)
            {
                var minIndex = 0;
                for (var i = 1; i < partitionCount; i++)
                    if (sizes[i] < sizes[minIndex])
                        minIndex = i;
                buckets[minIndex].Add(k);
                sizes[minIndex] += v.Length;
            }
        }
        finally
        {
            ArrayPool<long>.Shared.Return(sizesBuf);
        }

        var partitions = new List<IEnumerator<string>>(partitionCount);
        foreach (var bucket in buckets)
            partitions.Add(bucket.GetEnumerator());

        Debug.Assert(partitions.Count == partitionCount);

        return partitions;
    }

    /// <inheritdoc/>
    public override bool SupportsDynamicPartitions => false;
}
