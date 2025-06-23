namespace LaquaiLib.Extensions.Memory.Linq;

public static partial class LinqMemoryExtensions
{
    extension<TKey, TValue>(ReadOnlySpan<KeyValuePair<TKey, TValue>> source)
    {
        /// <inheritdoc cref="Enumerable.ToDictionary{TKey, TValue}(IEnumerable{KeyValuePair{TKey, TValue}})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Dictionary<TKey, TValue> ToDictionary()
        {
            var dictionary = new Dictionary<TKey, TValue>(source.Length);
            for (var i = 0; i < source.Length; i++)
            {
                var pair = source[i];
                dictionary.Add(pair.Key, pair.Value);
            }
            return dictionary;
        }

        /// <inheritdoc cref="Enumerable.ToDictionary{TKey, TValue}(IEnumerable{KeyValuePair{TKey, TValue}}, IEqualityComparer{TKey})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Dictionary<TKey, TValue> ToDictionary(IEqualityComparer<TKey> comparer)
        {
            var dictionary = new Dictionary<TKey, TValue>(source.Length, comparer);
            for (var i = 0; i < source.Length; i++)
            {
                var pair = source[i];
                dictionary.Add(pair.Key, pair.Value);
            }
            return dictionary;
        }
    }
}