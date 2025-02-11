namespace LaquaiLib.Extensions.ALinq;

/// <summary>
/// Provides parallel extensions for the Enumerable.SequenceEqual family of methods.
/// Note that none of these methods parallelize the consumption of the query, it is merely started in a new <see cref="Task" />.
/// </summary>
public static partial class IEnumerableExtensions
{
    /// <inheritdoc cref="Enumerable.SequenceEqual{TSource}(IEnumerable{TSource}, IEnumerable{TSource})" />
    public static Task<bool> SequenceEqualAsync<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second, CancellationToken cancellationToken = default)
        => Task.Run(() => first.SequenceEqual(second), cancellationToken);

    /// <inheritdoc cref="Enumerable.SequenceEqual{TSource}(IEnumerable{TSource}, IEnumerable{TSource}, IEqualityComparer{TSource})" />
    public static Task<bool> SequenceEqualAsync<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource> comparer, CancellationToken cancellationToken = default)
        => Task.Run(() => first.SequenceEqual(second, comparer), cancellationToken);

}