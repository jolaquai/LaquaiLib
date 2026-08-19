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
        /// <param name="partitions">The number of partitions to request.</param>
        /// <returns>A jagged array containing the elements of each partition.</returns>
        public T[][] ToArray(int partitions)
        {
            ArgumentNullException.ThrowIfNull(partitioner);

            var ret = new T[partitions][];
            var partitionEnumerators = partitioner.GetPartitions(partitions);
            var list = new List<T>();
            for (var i = 0; i < partitionEnumerators.Count; i++)
            {
                list.Clear();
                foreach (var item in partitionEnumerators[i])
                    list.Add(item);
                ret[i] = list.DrainToArray();
            }
            return ret;
        }
    }
}
