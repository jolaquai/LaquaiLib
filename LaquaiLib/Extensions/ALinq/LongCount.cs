namespace LaquaiLib.Extensions.ALinq;

// Provides parallel extensions for the System.Linq.Enumerable.LongCount family of methods.
public static partial class IEnumerableExtensions
{
    public static Task<long> LongCountAsync<TSource>(this IEnumerable<TSource> source, CancellationToken cancellationToken = default)
        => Task.Run(() => source.LongCount(), cancellationToken);

    public static Task<long> LongCountAsync<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate, CancellationToken cancellationToken = default)
        => Task.Run(() => source.LongCount(predicate), cancellationToken);

}