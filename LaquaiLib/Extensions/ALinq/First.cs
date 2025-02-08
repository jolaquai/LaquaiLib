namespace LaquaiLib.Extensions.ALinq;

// Provides parallel extensions for the System.Linq.Enumerable.First family of methods.
public static partial class IEnumerableExtensions
{
    public static Task<TSource> FirstAsync<TSource>(this IEnumerable<TSource> source, CancellationToken cancellationToken = default)
        => Task.Run(() => source.First(), cancellationToken);

    public static Task<TSource> FirstAsync<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate, CancellationToken cancellationToken = default)
        => Task.Run(() => source.First(predicate), cancellationToken);

}