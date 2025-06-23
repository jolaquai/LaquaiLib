namespace LaquaiLib.Extensions.Memory.Linq;

public static partial class LinqMemoryExtensions
{
    extension<TSource>(ReadOnlySpan<TSource> source)
    {
        /// <inheritdoc cref="Enumerable.DistinctBy{TSource, TKey}(IEnumerable{TSource}, Func{TSource, TKey})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int DistinctBy<TKey>(Func<TSource, TKey> keySelector, Span<TSource> destination) => DistinctBy(source, keySelector, destination, EqualityComparer<TKey>.Default);

        /// <inheritdoc cref="Enumerable.DistinctBy{TSource, TKey}(IEnumerable{TSource}, Func{TSource, TKey}, IEqualityComparer{TKey})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int DistinctBy<TKey>(Func<TSource, TKey> keySelector, Span<TSource> destination, IEqualityComparer<TKey> comparer)
        {
            if (destination.Length < source.Length)
            {
                throw new ArgumentException("Destination span is too short.", nameof(destination));
            }

            var destIndex = 0;
            comparer ??= EqualityComparer<TKey>.Default;
            var hashSet = new HashSet<TKey>(source.Length, comparer);
            for (var i = 0; i < source.Length; i++)
            {
                if (hashSet.Add(keySelector(source[i])))
                {
                    destination[destIndex++] = source[i];
                }
            }
            return destIndex;
        }
    }
}