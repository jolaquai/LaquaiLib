namespace LaquaiLib.Extensions.Memory.Linq;

public static partial class LinqMemoryExtensions
{
    extension<TSource>(ReadOnlySpan<TSource> source)
    {
        /// <inheritdoc cref="Enumerable.MinBy{TSource, TKey}(IEnumerable{TSource}, Func{TSource, TKey})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TSource MinBy<TKey>(Func<TSource, TKey> keySelector) => MinBy(source, keySelector, Comparer<TKey>.Default);

        /// <inheritdoc cref="Enumerable.MinBy{TSource, TKey}(IEnumerable{TSource}, Func{TSource, TKey}, IComparer{TKey})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TSource MinBy<TKey>(Func<TSource, TKey> keySelector, IComparer<TKey> comparer)
        {
            if (source.Length == 0)
            {
                throw new InvalidOperationException("Span is empty.");
            }
            var minItem = source[0];
            var minKey = keySelector(minItem);
            for (var i = 1; i < source.Length; i++)
            {
                var item = source[i];
                var key = keySelector(item);
                if (comparer.Compare(key, minKey) < 0)
                {
                    minItem = item;
                    minKey = key;
                }
            }
            return minItem;
        }
    }
}