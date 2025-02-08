namespace LaquaiLib.Extensions.ALinq;

// Provides parallel extensions for the System.Linq.Enumerable.Last family of methods.
public static partial class IEnumerableExtensions
{
    public static Task<TSource> LastAsync<TSource>(this IEnumerable<TSource> source, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Last(), cancellationToken);

    public static Task<TSource> LastAsync<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Last(predicate), cancellationToken);

}