namespace LaquaiLib.Extensions.ALinq;

// Provides parallel extensions for the System.Linq.Enumerable.Aggregate family of methods.
public static partial class IEnumerableExtensions
{
    public static Task<TSource> AggregateAsync<TSource>(this IEnumerable<TSource> source, Func<TSource, TSource, TSource> func, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Aggregate(func), cancellationToken);

    public static Task<TAccumulate> AggregateAsync<TSource, TAccumulate>(this IEnumerable<TSource> source, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> func, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Aggregate(seed, func), cancellationToken);

    public static Task<TResult> AggregateAsync<TSource, TAccumulate, TResult>(this IEnumerable<TSource> source, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> func, Func<TAccumulate, TResult> resultSelector, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Aggregate(seed, func, resultSelector), cancellationToken);

}