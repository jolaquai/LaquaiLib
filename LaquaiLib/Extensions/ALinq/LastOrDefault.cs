namespace LaquaiLib.Extensions.ALinq;

// Provides parallel extensions for the System.Linq.Enumerable.LastOrDefault family of methods.
public static partial class IEnumerableExtensions
{
    public static Task<TSource> LastOrDefaultAsync<TSource>(this IEnumerable<TSource> source, CancellationToken cancellationToken = default)
        => Task.Run(() => source.LastOrDefault(), cancellationToken);

    public static Task<TSource> LastOrDefaultAsync<TSource>(this IEnumerable<TSource> source, TSource defaultValue, CancellationToken cancellationToken = default)
        => Task.Run(() => source.LastOrDefault(defaultValue), cancellationToken);

    public static Task<TSource> LastOrDefaultAsync<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate, CancellationToken cancellationToken = default)
        => Task.Run(() => source.LastOrDefault(predicate), cancellationToken);

    public static Task<TSource> LastOrDefaultAsync<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate, TSource defaultValue, CancellationToken cancellationToken = default)
        => Task.Run(() => source.LastOrDefault(predicate, defaultValue), cancellationToken);

}