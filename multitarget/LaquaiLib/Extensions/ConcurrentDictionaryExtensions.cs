namespace LaquaiLib.Extensions;

/// <inheritdoc/>
public static class ConcurrentDictionaryExtensions
{
    extension<TKey, TValue>(ConcurrentDictionary<TKey, TValue>)
    {
        /// <summary>
        /// Creates a <see cref="ConcurrentDictionary{TKey, TValue}"/> from a <see cref="ReadOnlySpan{T}"/> of <see cref="KeyValuePair{TKey, TValue}"/>s.
        /// </summary>
        /// <param name="keyValuePairs">The key value pairs to add to the dictionary.</param>
        /// <returns>The created <see cref="ConcurrentDictionary{TKey, TValue}"/>.</returns>
        public static ConcurrentDictionary<TKey, TValue> Create(ReadOnlySpan<KeyValuePair<TKey, TValue>> keyValuePairs)
        {
            var dictionary = new ConcurrentDictionary<TKey, TValue>();
            foreach (var kvp in keyValuePairs)
                dictionary.TryAdd(kvp.Key, kvp.Value);
            return dictionary;
        }
    }
}
