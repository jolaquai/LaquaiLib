namespace LaquaiLib.Extensions;

public static partial class LinqMemoryExtensions
{
    extension<TSource>(ReadOnlySpan<TSource> source)
    {
        /// <inheritdoc cref="Enumerable.MaxBy{TSource, TKey}(IEnumerable{TSource}, Func{TSource, TKey})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TSource MaxBy<TKey>(Func<TSource, TKey> keySelector) => MaxBy(source, keySelector, Comparer<TKey>.Default);

        /// <inheritdoc cref="Enumerable.MaxBy{TSource, TKey}(IEnumerable{TSource}, Func{TSource, TKey}, IComparer{TKey})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TSource MaxBy<TKey>(Func<TSource, TKey> keySelector, IComparer<TKey> comparer)
        {
            if (source.Length == 0)
            {
                throw new InvalidOperationException("Span is empty.");
            }
            var maxItem = source[0];
            var maxKey = keySelector(maxItem);
            for (var i = 1; i < source.Length; i++)
            {
                var item = source[i];
                var key = keySelector(item);
                if (comparer.Compare(key, maxKey) > 0)
                {
                    maxItem = item;
                    maxKey = key;
                }
            }
            return maxItem;
        }
    }
}