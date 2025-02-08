namespace LaquaiLib.Extensions.ALinq;

// Provides parallel extensions for the System.Linq.Enumerable.All family of methods.
public static partial class IEnumerableExtensions
{
    public static Task<bool> AllAsync<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate, CancellationToken cancellationToken = default)
        => Task.Run(() => source.All(predicate), cancellationToken);

}