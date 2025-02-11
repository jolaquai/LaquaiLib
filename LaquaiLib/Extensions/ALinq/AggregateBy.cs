namespace LaquaiLib.Extensions.ALinq;

/// <summary>
/// Provides parallel extensions for the Enumerable.AggregateBy family of methods.
/// Note that none of these methods parallelize the consumption of the query, it is merely started in a new <see cref="Task" />.
/// </summary>
public static partial class IEnumerableExtensions
{
    /// <inheritdoc cref="Enumerable.AggregateBy{TSource, TKey, TAccumulate}(IEnumerable{TSource}, Func{TSource, TKey}, TAccumulate, Func{TAccumulate, TSource, TAccumulate}, IEqualityComparer{TKey})" />
    public static Task<IEnumerable<KeyValuePair<TKey, TAccumulate>>> AggregateByAsync<TSource, TKey, TAccumulate>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> func, IEqualityComparer<TKey> keyComparer, CancellationToken cancellationToken = default)
        => Task.Run(() => source.AggregateBy(keySelector, seed, func, keyComparer), cancellationToken);

    /// <inheritdoc cref="Enumerable.AggregateBy{TSource, TKey, TAccumulate}(IEnumerable{TSource}, Func{TSource, TKey}, Func{TKey, TAccumulate}, Func{TAccumulate, TSource, TAccumulate}, IEqualityComparer{TKey})" />
    public static Task<IEnumerable<KeyValuePair<TKey, TAccumulate>>> AggregateByAsync<TSource, TKey, TAccumulate>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TKey, TAccumulate> seedSelector, Func<TAccumulate, TSource, TAccumulate> func, IEqualityComparer<TKey> keyComparer, CancellationToken cancellationToken = default)
        => Task.Run(() => source.AggregateBy(keySelector, seedSelector, func, keyComparer), cancellationToken);

}