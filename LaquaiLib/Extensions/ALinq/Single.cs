namespace LaquaiLib.Extensions.ALinq;

// Provides parallel extensions for the System.Linq.Enumerable.Single family of methods.
public static partial class IEnumerableExtensions
{
    public static Task<TSource> SingleAsync<TSource>(this IEnumerable<TSource> source, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Single(), cancellationToken);

    public static Task<TSource> SingleAsync<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Single(predicate), cancellationToken);

}