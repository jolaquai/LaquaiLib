namespace LaquaiLib.Extensions;

public static partial class LinqMemoryExtensions
{
    extension<TSource>(ReadOnlySpan<TSource> source)
    {
        /// <inheritdoc cref="Enumerable.GroupBy{TSource, TKey}(IEnumerable{TSource}, Func{TSource, TKey})"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<IGrouping<TKey, TSource>> GroupBy<TKey>(Func<TSource, TKey> keySelector) => ToLookup(source, keySelector);

        /// <inheritdoc cref="Enumerable.GroupBy{TSource, TKey}(IEnumerable{TSource}, Func{TSource, TKey}, IEqualityComparer{TKey})"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<IGrouping<TKey, TSource>> GroupBy<TKey>(Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer) => ToLookup(source, keySelector, comparer);

        /// <inheritdoc cref="Enumerable.GroupBy{TSource, TKey, TElement}(IEnumerable{TSource}, Func{TSource, TKey}, Func{TSource, TElement})"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<IGrouping<TKey, TElement>> GroupBy<TKey, TElement>(Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector) => ToLookup(source, keySelector, elementSelector);

        /// <inheritdoc cref="Enumerable.GroupBy{TSource, TKey, TElement}(IEnumerable{TSource}, Func{TSource, TKey}, Func{TSource, TElement}, IEqualityComparer{TKey})"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<IGrouping<TKey, TElement>> GroupBy<TKey, TElement>(Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey> comparer) => ToLookup(source, keySelector, elementSelector, comparer);

        /// <inheritdoc cref="Enumerable.GroupBy{TSource, TKey, TResult}(IEnumerable{TSource}, Func{TSource, TKey}, Func{TKey, IEnumerable{TSource}, TResult})"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<TResult> GroupBy<TKey, TResult>(Func<TSource, TKey> keySelector, Func<TKey, IEnumerable<TSource>, TResult> resultSelector)
            => ToLookup(source, keySelector).Select(group => resultSelector(group.Key, group));

        /// <inheritdoc cref="Enumerable.GroupBy{TSource, TKey, TResult}(IEnumerable{TSource}, Func{TSource, TKey}, Func{TKey, IEnumerable{TSource}, TResult}, IEqualityComparer{TKey})"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<TResult> GroupBy<TKey, TResult>(Func<TSource, TKey> keySelector, Func<TKey, IEnumerable<TSource>, TResult> resultSelector, IEqualityComparer<TKey> comparer)
            => ToLookup(source, keySelector, comparer).Select(group => resultSelector(group.Key, group));

        /// <inheritdoc cref="Enumerable.GroupBy{TSource, TKey, TElement, TResult}(IEnumerable{TSource}, Func{TSource, TKey}, Func{TSource, TElement}, Func{TKey, IEnumerable{TElement}, TResult})"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<TResult> GroupBy<TKey, TElement, TResult>(Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, Func<TKey, IEnumerable<TElement>, TResult> resultSelector)
            => ToLookup(source, keySelector, elementSelector).Select(group => resultSelector(group.Key, group));

        /// <inheritdoc cref="Enumerable.GroupBy{TSource, TKey, TElement, TResult}(IEnumerable{TSource}, Func{TSource, TKey}, Func{TSource, TElement}, Func{TKey, IEnumerable{TElement}, TResult}, IEqualityComparer{TKey})"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<TResult> GroupBy<TKey, TElement, TResult>(Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, Func<TKey, IEnumerable<TElement>, TResult> resultSelector, IEqualityComparer<TKey> comparer)
            => ToLookup(source, keySelector, elementSelector, comparer).Select(group => resultSelector(group.Key, group));
    }
}