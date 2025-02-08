namespace LaquaiLib.Extensions.ALinq;

// Provides parallel extensions for the System.Linq.Enumerable.MinBy family of methods.
public static partial class IEnumerableExtensions
{
    public static Task<TSource> MinByAsync<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, CancellationToken cancellationToken = default)
        => Task.Run(() => source.MinBy(keySelector), cancellationToken);

    public static Task<TSource> MinByAsync<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IComparer<TKey> comparer, CancellationToken cancellationToken = default)
        => Task.Run(() => source.MinBy(keySelector, comparer), cancellationToken);

}