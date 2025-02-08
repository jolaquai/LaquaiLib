namespace LaquaiLib.Extensions.ALinq;

// Provides parallel extensions for the System.Linq.Enumerable.ToArray family of methods.
public static partial class IEnumerableExtensions
{
    public static Task<TSource[]> ToArrayAsync<TSource>(this IEnumerable<TSource> source, CancellationToken cancellationToken = default)
        => Task.Run(() => source.ToArray(), cancellationToken);

}