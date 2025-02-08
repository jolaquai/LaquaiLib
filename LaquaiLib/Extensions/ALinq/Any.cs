namespace LaquaiLib.Extensions.ALinq;

// Provides parallel extensions for the System.Linq.Enumerable.Any family of methods.
public static partial class IEnumerableExtensions
{
    public static Task<bool> AnyAsync<TSource>(this IEnumerable<TSource> source, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Any(), cancellationToken);

    public static Task<bool> AnyAsync<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Any(predicate), cancellationToken);

}