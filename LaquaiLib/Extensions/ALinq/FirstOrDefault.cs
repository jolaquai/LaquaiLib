namespace LaquaiLib.Extensions.ALinq;

// Provides parallel extensions for the System.Linq.Enumerable.FirstOrDefault family of methods.
public static partial class IEnumerableExtensions
{
    public static Task<TSource> FirstOrDefaultAsync<TSource>(this IEnumerable<TSource> source, CancellationToken cancellationToken = default)
        => Task.Run(() => source.FirstOrDefault(), cancellationToken);

    public static Task<TSource> FirstOrDefaultAsync<TSource>(this IEnumerable<TSource> source, TSource defaultValue, CancellationToken cancellationToken = default)
        => Task.Run(() => source.FirstOrDefault(defaultValue), cancellationToken);

    public static Task<TSource> FirstOrDefaultAsync<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate, CancellationToken cancellationToken = default)
        => Task.Run(() => source.FirstOrDefault(predicate), cancellationToken);

    public static Task<TSource> FirstOrDefaultAsync<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate, TSource defaultValue, CancellationToken cancellationToken = default)
        => Task.Run(() => source.FirstOrDefault(predicate, defaultValue), cancellationToken);

}