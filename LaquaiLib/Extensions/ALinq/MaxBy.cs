namespace LaquaiLib.Extensions.ALinq;

// Provides parallel extensions for the System.Linq.Enumerable.MaxBy family of methods.
public static partial class IEnumerableExtensions
{
    public static Task<TSource> MaxByAsync<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, CancellationToken cancellationToken = default)
        => Task.Run(() => source.MaxBy(keySelector), cancellationToken);

    public static Task<TSource> MaxByAsync<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IComparer<TKey> comparer, CancellationToken cancellationToken = default)
        => Task.Run(() => source.MaxBy(keySelector, comparer), cancellationToken);

}