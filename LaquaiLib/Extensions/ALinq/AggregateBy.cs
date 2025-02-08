namespace LaquaiLib.Extensions.ALinq;

// Provides parallel extensions for the System.Linq.Enumerable.AggregateBy family of methods.
public static partial class IEnumerableExtensions
{
    public static Task<IEnumerable<KeyValuePair<TKey, TAccumulate>>> AggregateByAsync<TSource, TKey, TAccumulate>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> func, IEqualityComparer<TKey> keyComparer, CancellationToken cancellationToken = default)
        => Task.Run(() => source.AggregateBy(keySelector, seed, func, keyComparer), cancellationToken);

    public static Task<IEnumerable<KeyValuePair<TKey, TAccumulate>>> AggregateByAsync<TSource, TKey, TAccumulate>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TKey, TAccumulate> seedSelector, Func<TAccumulate, TSource, TAccumulate> func, IEqualityComparer<TKey> keyComparer, CancellationToken cancellationToken = default)
        => Task.Run(() => source.AggregateBy(keySelector, seedSelector, func, keyComparer), cancellationToken);

}