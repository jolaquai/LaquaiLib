namespace LaquaiLib.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="Partitioner"/> type and derived types.
/// </summary>
public static class PartitionerExtensions
{
    extension<T>(Partitioner<T> partitioner)
    {
        /// <summary>
        /// Enumerates each partition of the <paramref name="partitioner"/> and returns a <see cref="List{T}"/> of <see cref="List{T}"/>s containing the elements of each partition.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the <paramref name="partitioner"/>.</typeparam>
        /// <param name="partitioner">The <see cref="Partitioner{T}"/> to enumerate.</param>
        /// <param name="partitions">The number of partitions to request.</param>
        /// <returns>The <see cref="List{T}"/> of <see cref="List{T}"/>s containing the elements of each partition.</returns>
        public List<T[]> ToList(int partitions)
        {
            ArgumentNullException.ThrowIfNull(partitioner);

            List<T[]> list = [];
            var partitionEnumerators = partitioner.GetPartitions(partitions);
            for (var i = 0; i < partitionEnumerators.Count; i++)
            {
                var enumerator = partitionEnumerators[i];
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
        public IEnumerable<IEnumerable<T>> AsEnumerable(int partitions)
        {
            ArgumentNullException.ThrowIfNull(partitioner);

            static IEnumerable<T> TransformPartition(IEnumerator<T> enumerator)
            {
                while (enumerator.MoveNext())
                {
                    yield return enumerator.Current;
                }
            }

            var list = partitioner.GetPartitions(partitions);
            for (var i = 0; i < list.Count; i++)
            {
                using var item = list[i];
                yield return TransformPartition(item);
            }
        }
        /// <summary>
        /// Flattens a <see cref="Partitioner{TSource}"/> back into a single <see cref="IEnumerable{T}"/>.
        /// </summary>
        /// <returns>The flattened <see cref="IEnumerable{T}"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<T> Flatten() => partitioner.AsEnumerable(1).SelectMany();
    }
}
