using System.Collections.Concurrent;

namespace LaquaiLib.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="Partitioner"/> type and derived types.
/// </summary>
public static class PartitionerExtensions
{
    /// <summary>
    /// Enumerates each partition of the <paramref name="partitioner"/> and returns a <see cref="List{T}"/> of <see cref="List{T}"/>s containing the elements of each partition.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the <paramref name="partitioner"/>.</typeparam>
    /// <param name="partitioner">The <see cref="Partitioner{T}"/> to enumerate.</param>
    /// <param name="partitions">The number of partitions to request.</param>
    /// <returns>The <see cref="List{T}"/> of <see cref="List{T}"/>s containing the elements of each partition.</returns>
    public static List<T[]> ToList<T>(this Partitioner<T> partitioner, int partitions)
    {
        ArgumentNullException.ThrowIfNull(partitioner);

        List<T[]> list = [];
        foreach (var enumerator in partitioner.GetPartitions(partitions))
        {
            using (enumerator)
            {
                list.Add([.. enumerator.AsEnumerable()]);
            }
        } 
        return list;
    }
    /// <summary>
    /// Transforms each partition of the <paramref name="partitioner"/> into an <see cref="IEnumerable{T}"/> of <see cref="IEnumerable{T}"/>s containing the elements of each partition.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the <paramref name="partitioner"/>.</typeparam>
    /// <param name="partitioner">The <see cref="Partitioner{T}"/> to transform.</param>
    /// <param name="partitions">The number of partitions to request.</param>
    /// <returns>The <see cref="IEnumerable{T}"/> of <see cref="IEnumerable{T}"/>s enumerating the elements of each partition.</returns>
    public static IEnumerable<IEnumerable<T>> AsEnumerable<T>(this Partitioner<T> partitioner, int partitions)
    {
        ArgumentNullException.ThrowIfNull(partitioner);

        static IEnumerable<T> TransformPartition(IEnumerator<T> enumerator)
        {
            while (enumerator.MoveNext())
            {
                yield return enumerator.Current;
            }
        }

        foreach (var item in partitioner.GetPartitions(partitions))
        {
            yield return TransformPartition(item);
        }
    }
}
