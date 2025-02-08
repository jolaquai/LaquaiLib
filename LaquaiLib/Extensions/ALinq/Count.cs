namespace LaquaiLib.Extensions.ALinq;

// Provides parallel extensions for the System.Linq.Enumerable.Count family of methods.
public static partial class IEnumerableExtensions
{
    public static Task<int> CountAsync<TSource>(this IEnumerable<TSource> source, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Count(), cancellationToken);

    public static Task<int> CountAsync<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Count(predicate), cancellationToken);

}