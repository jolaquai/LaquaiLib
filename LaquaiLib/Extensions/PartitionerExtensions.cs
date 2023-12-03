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
    public static List<List<T>> ToList<T>(this Partitioner<T> partitioner, int partitions)
    {
        ArgumentNullException.ThrowIfNull(partitioner);

        var list = new List<List<T>>();
        foreach (var item in partitioner.GetPartitions(partitions))
        {
            var sublist = new List<T>();
            while (item.MoveNext())
            {
                sublist.Add(item.Current);
            }
        }
        return list;
    }
}
