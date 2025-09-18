namespace LaquaiLib.Extensions;

public static partial class LinqMemoryExtensions
{
    extension<TSource>(ReadOnlySpan<TSource> source)
    {
        /// <inheritdoc cref="Enumerable.ToDictionary{TSource, TKey}(IEnumerable{TSource}, Func{TSource, TKey})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Dictionary<TKey, TSource> ToDictionary<TKey, TValue>(Func<TSource, TKey> keySelector)
        {
            var dict = new Dictionary<TKey, TSource>(source.Length);
            for (var i = 0; i < source.Length; i++)
            {
                var value = source[i];
                var key = keySelector(value);
                if (!dict.TryAdd(key, value))
                {
                    throw new ArgumentException($"Duplicate key found: {key}");
                }
            }
            return dict;
        }

        /// <inheritdoc cref="Enumerable.ToDictionary{TSource, TKey}(IEnumerable{TSource}, Func{TSource, TKey}, IEqualityComparer{TKey})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Dictionary<TKey, TSource> ToDictionary<TKey, TValue>(Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer)
        {
            var dict = new Dictionary<TKey, TSource>(source.Length, comparer);
            for (var i = 0; i < source.Length; i++)
            {
                var value = source[i];
                var key = keySelector(value);
                if (!dict.TryAdd(key, value))
                {
                    throw new ArgumentException($"Duplicate key found: {key}");
                }
            }
            return dict;
        }

        /// <inheritdoc cref="Enumerable.ToDictionary{TSource, TKey, TElement}(IEnumerable{TSource}, Func{TSource, TKey}, Func{TSource, TElement})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Dictionary<TKey, TElement> ToDictionary<TKey, TElement>(Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector)
        {
            var dict = new Dictionary<TKey, TElement>(source.Length);
            for (var i = 0; i < source.Length; i++)
            {
                var value = source[i];
                var key = keySelector(value);
                var element = elementSelector(value);
                if (!dict.TryAdd(key, element))
                {
                    throw new ArgumentException($"Duplicate key found: {key}");
                }
            }
            return dict;
        }

        /// <inheritdoc cref="Enumerable.ToDictionary{TSource, TKey, TElement}(IEnumerable{TSource}, Func{TSource, TKey}, Func{TSource, TElement}, IEqualityComparer{TKey})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Dictionary<TKey, TElement> ToDictionary<TKey, TElement>(Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey> comparer)
        {
            var dict = new Dictionary<TKey, TElement>(source.Length, comparer);
            for (var i = 0; i < source.Length; i++)
            {
                var value = source[i];
                var key = keySelector(value);
                var element = elementSelector(value);
                if (!dict.TryAdd(key, element))
                {
                    throw new ArgumentException($"Duplicate key found: {key}");
                }
            }
            return dict;
        }
    }
}