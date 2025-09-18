namespace LaquaiLib.Extensions;

public static partial class LinqMemoryExtensions
{
    extension<TSource>(ReadOnlySpan<TSource> source)
    {
        /// <inheritdoc cref="Enumerable.CountBy{TSource, TKey}(IEnumerable{TSource}, Func{TSource, TKey}, IEqualityComparer{TKey})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<KeyValuePair<TKey, int>> CountBy<TKey>(Func<TSource, TKey> keySelector, IEqualityComparer<TKey> keyComparer)
        {
            var dict = new Dictionary<TKey, int>(keyComparer);
            for (var i = 0; i < source.Length; i++)
            {
                var key = keySelector(source[i]);
                dict.AddOrUpdate(key, 1, (k, v) => v + 1);
            }
            return dict;
        }
    }
}