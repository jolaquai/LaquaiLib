namespace LaquaiLib.Extensions;

public static partial class LinqMemoryExtensions
{
    extension<TSource>(in ReadOnlySpan<TSource> source)
    {
        /// <inheritdoc cref="Enumerable.AggregateBy{TSource, TKey, TAccumulate}(IEnumerable{TSource}, Func{TSource, TKey}, TAccumulate, Func{TAccumulate, TSource, TAccumulate}, IEqualityComparer{TKey})"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<KeyValuePair<TKey, TAccumulate>> AggregateBy<TKey, TAccumulate>(Func<TSource, TKey> keySelector, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> func, IEqualityComparer<TKey> keyComparer)
            => GroupBy(source, keySelector, keyComparer).Select(g => KeyValuePair.Create(g.Key, g.Aggregate(seed, func)));

        /// <inheritdoc cref="Enumerable.AggregateBy{TSource, TKey, TAccumulate}(IEnumerable{TSource}, Func{TSource, TKey}, Func{TKey, TAccumulate}, Func{TAccumulate, TSource, TAccumulate}, IEqualityComparer{TKey})"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<KeyValuePair<TKey, TAccumulate>> AggregateBy<TKey, TAccumulate>(Func<TSource, TKey> keySelector, Func<TKey, TAccumulate> seedSelector, Func<TAccumulate, TSource, TAccumulate> func, IEqualityComparer<TKey> keyComparer)
            => GroupBy(source, keySelector, keyComparer).Select(g => KeyValuePair.Create(g.Key, g.Aggregate(seedSelector(g.Key), func)));
    }
}