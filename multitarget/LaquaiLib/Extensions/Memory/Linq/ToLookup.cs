namespace LaquaiLib.Extensions;

public static partial class LinqMemoryExtensions
{
    extension<TSource>(in ReadOnlySpan<TSource> source)
    {
        /// <inheritdoc cref="Enumerable.ToLookup{TSource, TKey}(IEnumerable{TSource}, Func{TSource, TKey})"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ILookup<TKey, TSource> ToLookup<TKey>(Func<TSource, TKey> keySelector)
        {
            var spanLookup = new SpanLookup<TKey, TSource>(source.Length, null);
            for (var i = 0; i < source.Length; i++)
            {
                var value = source[i];
                var key = keySelector(value);

                ref var list = ref CollectionsMarshal.GetValueRefOrAddDefault(spanLookup._lookup, key, out _);
                list ??= [];
                list.Add(value);
            }
            return spanLookup;
        }

        /// <inheritdoc cref="Enumerable.ToLookup{TSource, TKey}(IEnumerable{TSource}, Func{TSource, TKey}, IEqualityComparer{TKey})"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ILookup<TKey, TSource> ToLookup<TKey>(Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer)
        {
            var spanLookup = new SpanLookup<TKey, TSource>(source.Length, comparer);
            for (var i = 0; i < source.Length; i++)
            {
                var value = source[i];
                var key = keySelector(value);
                ref var list = ref CollectionsMarshal.GetValueRefOrAddDefault(spanLookup._lookup, key, out _);
                list ??= [];
                list.Add(value);
            }
            return spanLookup;
        }

        /// <inheritdoc cref="Enumerable.ToLookup{TSource, TKey, TElement}(IEnumerable{TSource}, Func{TSource, TKey}, Func{TSource, TElement})"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ILookup<TKey, TElement> ToLookup<TKey, TElement>(Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector)
        {
            var spanLookup = new SpanLookup<TKey, TElement>(source.Length, null);
            for (var i = 0; i < source.Length; i++)
            {
                var value = source[i];
                var key = keySelector(value);
                var element = elementSelector(value);
                ref var list = ref CollectionsMarshal.GetValueRefOrAddDefault(spanLookup._lookup, key, out _);
                list ??= [];
                list.Add(element);
            }
            return spanLookup;
        }

        /// <inheritdoc cref="Enumerable.ToLookup{TSource, TKey, TElement}(IEnumerable{TSource}, Func{TSource, TKey}, Func{TSource, TElement}, IEqualityComparer{TKey})"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ILookup<TKey, TElement> ToLookup<TKey, TElement>(Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey> comparer)
        {
            var spanLookup = new SpanLookup<TKey, TElement>(source.Length, comparer);
            for (var i = 0; i < source.Length; i++)
            {
                var value = source[i];
                var key = keySelector(value);
                var element = elementSelector(value);
                ref var list = ref CollectionsMarshal.GetValueRefOrAddDefault(spanLookup._lookup, key, out _);
                list ??= [];
                list.Add(element);
            }
            return spanLookup;
        }
    }
}