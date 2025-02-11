namespace LaquaiLib.Extensions.ALinq;

/// <summary>
/// Provides parallel extensions for the Enumerable.ElementAtOrDefault family of methods.
/// Note that none of these methods parallelize the consumption of the query, it is merely started in a new <see cref="Task" />.
/// </summary>
public static partial class IEnumerableExtensions
{
    /// <inheritdoc cref="Enumerable.ElementAtOrDefault{TSource}(IEnumerable{TSource}, int)" />
    public static Task<TSource> ElementAtOrDefaultAsync<TSource>(this IEnumerable<TSource> source, int index, CancellationToken cancellationToken = default)
        => Task.Run(() => source.ElementAtOrDefault(index), cancellationToken);

    /// <inheritdoc cref="Enumerable.ElementAtOrDefault{TSource}(IEnumerable{TSource}, Index)" />
    public static Task<TSource> ElementAtOrDefaultAsync<TSource>(this IEnumerable<TSource> source, Index index, CancellationToken cancellationToken = default)
        => Task.Run(() => source.ElementAtOrDefault(index), cancellationToken);

}