namespace LaquaiLib.Extensions.ALinq;

// Provides parallel extensions for the System.Linq.Enumerable.Contains family of methods.
public static partial class IEnumerableExtensions
{
    public static Task<bool> ContainsAsync<TSource>(this IEnumerable<TSource> source, TSource value, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Contains(value), cancellationToken);

    public static Task<bool> ContainsAsync<TSource>(this IEnumerable<TSource> source, TSource value, IEqualityComparer<TSource> comparer, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Contains(value, comparer), cancellationToken);

}