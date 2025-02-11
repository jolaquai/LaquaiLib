namespace LaquaiLib.Extensions.ALinq;

/// <summary>
/// Provides parallel extensions for the Enumerable.CountBy family of methods.
/// Note that none of these methods parallelize the consumption of the query, it is merely started in a new <see cref="Task" />.
/// </summary>
public static partial class IEnumerableExtensions
{
    /// <inheritdoc cref="Enumerable.CountBy{TSource, TKey}(IEnumerable{TSource}, Func{TSource, TKey}, IEqualityComparer{TKey})" />
    public static Task<IEnumerable<KeyValuePair<TKey, int>>> CountByAsync<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> keyComparer, CancellationToken cancellationToken = default)
        => Task.Run(() => source.CountBy(keySelector, keyComparer), cancellationToken);

}