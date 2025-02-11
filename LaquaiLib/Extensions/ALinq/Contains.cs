namespace LaquaiLib.Extensions.ALinq;

/// <summary>
/// Provides parallel extensions for the Enumerable.Contains family of methods.
/// Note that none of these methods parallelize the consumption of the query, it is merely started in a new <see cref="Task" />.
/// </summary>
public static partial class IEnumerableExtensions
{
    /// <inheritdoc cref="Enumerable.Contains{TSource}(IEnumerable{TSource}, TSource)" />
    public static Task<bool> ContainsAsync<TSource>(this IEnumerable<TSource> source, TSource value, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Contains(value), cancellationToken);

    /// <inheritdoc cref="Enumerable.Contains{TSource}(IEnumerable{TSource}, TSource, IEqualityComparer{TSource})" />
    public static Task<bool> ContainsAsync<TSource>(this IEnumerable<TSource> source, TSource value, IEqualityComparer<TSource> comparer, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Contains(value, comparer), cancellationToken);

}