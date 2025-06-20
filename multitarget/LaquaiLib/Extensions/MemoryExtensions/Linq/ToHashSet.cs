namespace LaquaiLib.Extensions.MemoryExtensions.Linq;

public static partial class MemoryExtensions
{
    extension<TSource>(ReadOnlySpan<TSource> source)
    {
        /// <inheritdoc cref="Enumerable.ToHashSet{TSource}(IEnumerable{TSource})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public HashSet<TSource> ToHashSet()
        {
            var set = new HashSet<TSource>(source.Length);
            for (var i = 0; i < source.Length; i++)
            {
                _ = set.Add(source[i]);
            }
            return set;
        }

        /// <inheritdoc cref="Enumerable.ToHashSet{TSource}(IEnumerable{TSource}, IEqualityComparer{TSource})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public HashSet<TSource> ToHashSet(IEqualityComparer<TSource> comparer)
        {
            var set = new HashSet<TSource>(source.Length, comparer);
            for (var i = 0; i < source.Length; i++)
            {
                _ = set.Add(source[i]);
            }
            return set;
        }
    }
}