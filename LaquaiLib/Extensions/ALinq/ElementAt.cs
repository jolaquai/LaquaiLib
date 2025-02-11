namespace LaquaiLib.Extensions.ALinq;

/// <summary>
/// Provides parallel extensions for the Enumerable.ElementAt family of methods.
/// Note that none of these methods parallelize the consumption of the query, it is merely started in a new <see cref="Task" />.
/// </summary>
public static partial class IEnumerableExtensions
{
    /// <inheritdoc cref="Enumerable.ElementAt{TSource}(IEnumerable{TSource}, int)" />
    public static Task<TSource> ElementAtAsync<TSource>(this IEnumerable<TSource> source, int index, CancellationToken cancellationToken = default)
        => Task.Run(() => source.ElementAt(index), cancellationToken);

    /// <inheritdoc cref="Enumerable.ElementAt{TSource}(IEnumerable{TSource}, Index)" />
    public static Task<TSource> ElementAtAsync<TSource>(this IEnumerable<TSource> source, Index index, CancellationToken cancellationToken = default)
        => Task.Run(() => source.ElementAt(index), cancellationToken);

}