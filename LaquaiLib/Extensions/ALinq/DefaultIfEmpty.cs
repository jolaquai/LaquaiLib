namespace LaquaiLib.Extensions.ALinq;

// Provides parallel extensions for the System.Linq.Enumerable.DefaultIfEmpty family of methods.
public static partial class IEnumerableExtensions
{
    public static Task<IEnumerable<TSource>> DefaultIfEmptyAsync<TSource>(this IEnumerable<TSource> source, CancellationToken cancellationToken = default)
        => Task.Run(() => source.DefaultIfEmpty(), cancellationToken);

    public static Task<IEnumerable<TSource>> DefaultIfEmptyAsync<TSource>(this IEnumerable<TSource> source, TSource defaultValue, CancellationToken cancellationToken = default)
        => Task.Run(() => source.DefaultIfEmpty(defaultValue), cancellationToken);

}