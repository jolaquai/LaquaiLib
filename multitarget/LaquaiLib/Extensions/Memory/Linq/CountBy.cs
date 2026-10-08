namespace LaquaiLib.Extensions;

public static partial class LinqMemoryExtensions
{
    extension<TSource>(in ReadOnlySpan<TSource> source)
    {
        /// <inheritdoc cref="Enumerable.CountBy{TSource, TKey}(IEnumerable{TSource}, Func{TSource, TKey}, IEqualityComparer{TKey})"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<KeyValuePair<TKey, int>> CountBy<TKey>(Func<TSource, TKey> keySelector, IEqualityComparer<TKey> keyComparer)
        {
            var dict = new Dictionary<TKey, int>(keyComparer);
            for (var i = 0; i < source.Length; i++)
            {
                var key = keySelector(source[i]);
                // AddOrUpdate's two-arg factory is (existingValue, addValue); increment the existing count.
                dict.AddOrUpdate(key, 1, (existing, _) => existing + 1);
            }
            return dict;
        }
    }
}