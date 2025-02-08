namespace LaquaiLib.Extensions.ALinq;

// Provides parallel extensions for the System.Linq.Enumerable.ElementAt family of methods.
public static partial class IEnumerableExtensions
{
    public static Task<TSource> ElementAtAsync<TSource>(this IEnumerable<TSource> source, int index, CancellationToken cancellationToken = default)
        => Task.Run(() => source.ElementAt(index), cancellationToken);

    public static Task<TSource> ElementAtAsync<TSource>(this IEnumerable<TSource> source, Index index, CancellationToken cancellationToken = default)
        => Task.Run(() => source.ElementAt(index), cancellationToken);

}