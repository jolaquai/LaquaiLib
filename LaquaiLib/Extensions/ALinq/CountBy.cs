namespace LaquaiLib.Extensions.ALinq;

// Provides parallel extensions for the System.Linq.Enumerable.CountBy family of methods.
public static partial class IEnumerableExtensions
{
    public static Task<IEnumerable<KeyValuePair<TKey, int>>> CountByAsync<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> keyComparer, CancellationToken cancellationToken = default)
        => Task.Run(() => source.CountBy(keySelector, keyComparer), cancellationToken);

}