namespace LaquaiLib.Extensions.ALinq;

/// <summary>
/// Provides parallel extensions for the Enumerable.ToHashSet family of methods.
/// Note that none of these methods parallelize the consumption of the query, it is merely started in a new <see cref="Task" />.
/// </summary>
public static partial class IEnumerableExtensions
{
    /// <inheritdoc cref="Enumerable.ToHashSet{TSource}(IEnumerable{TSource})" />
    public static Task<HashSet<TSource>> ToHashSetAsync<TSource>(this IEnumerable<TSource> source, CancellationToken cancellationToken = default)
        => Task.Run(() => source.ToHashSet(), cancellationToken);

    /// <inheritdoc cref="Enumerable.ToHashSet{TSource}(IEnumerable{TSource}, IEqualityComparer{TSource})" />
    public static Task<HashSet<TSource>> ToHashSetAsync<TSource>(this IEnumerable<TSource> source, IEqualityComparer<TSource> comparer, CancellationToken cancellationToken = default)
        => Task.Run(() => source.ToHashSet(comparer), cancellationToken);

}