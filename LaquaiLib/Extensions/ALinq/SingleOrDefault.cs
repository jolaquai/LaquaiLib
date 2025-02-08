namespace LaquaiLib.Extensions.ALinq;

// Provides parallel extensions for the System.Linq.Enumerable.SingleOrDefault family of methods.
public static partial class IEnumerableExtensions
{
    public static Task<TSource> SingleOrDefaultAsync<TSource>(this IEnumerable<TSource> source, CancellationToken cancellationToken = default)
        => Task.Run(() => source.SingleOrDefault(), cancellationToken);

    public static Task<TSource> SingleOrDefaultAsync<TSource>(this IEnumerable<TSource> source, TSource defaultValue, CancellationToken cancellationToken = default)
        => Task.Run(() => source.SingleOrDefault(defaultValue), cancellationToken);

    public static Task<TSource> SingleOrDefaultAsync<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate, CancellationToken cancellationToken = default)
        => Task.Run(() => source.SingleOrDefault(predicate), cancellationToken);

    public static Task<TSource> SingleOrDefaultAsync<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate, TSource defaultValue, CancellationToken cancellationToken = default)
        => Task.Run(() => source.SingleOrDefault(predicate, defaultValue), cancellationToken);

}