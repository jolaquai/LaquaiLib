namespace LaquaiLib.Extensions.ALinq;

/// <summary>
/// Provides parallel extensions for the Enumerable.ToArray family of methods.
/// Note that none of these methods parallelize the consumption of the query, it is merely started in a new <see cref="Task" />.
/// </summary>
public static partial class IEnumerableExtensions
{
    /// <inheritdoc cref="Enumerable.ToArray{TSource}(IEnumerable{TSource})" />
    public static Task<TSource[]> ToArrayAsync<TSource>(this IEnumerable<TSource> source, CancellationToken cancellationToken = default)
        => Task.Run(() => source.ToArray(), cancellationToken);

}