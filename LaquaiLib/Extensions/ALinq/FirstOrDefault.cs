namespace LaquaiLib.Extensions.ALinq;

/// <summary>
/// Provides parallel extensions for the Enumerable.FirstOrDefault family of methods.
/// Note that none of these methods parallelize the consumption of the query, it is merely started in a new <see cref="Task" />.
/// </summary>
public static partial class IEnumerableExtensions
{
    /// <inheritdoc cref="Enumerable.FirstOrDefault{TSource}(IEnumerable{TSource})" />
    public static Task<TSource> FirstOrDefaultAsync<TSource>(this IEnumerable<TSource> source, CancellationToken cancellationToken = default)
        => Task.Run(() => source.FirstOrDefault(), cancellationToken);

    /// <inheritdoc cref="Enumerable.FirstOrDefault{TSource}(IEnumerable{TSource}, TSource)" />
    public static Task<TSource> FirstOrDefaultAsync<TSource>(this IEnumerable<TSource> source, TSource defaultValue, CancellationToken cancellationToken = default)
        => Task.Run(() => source.FirstOrDefault(defaultValue), cancellationToken);

    /// <inheritdoc cref="Enumerable.FirstOrDefault{TSource}(IEnumerable{TSource}, Func{TSource, bool})" />
    public static Task<TSource> FirstOrDefaultAsync<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate, CancellationToken cancellationToken = default)
        => Task.Run(() => source.FirstOrDefault(predicate), cancellationToken);

    /// <inheritdoc cref="Enumerable.FirstOrDefault{TSource}(IEnumerable{TSource}, Func{TSource, bool}, TSource)" />
    public static Task<TSource> FirstOrDefaultAsync<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate, TSource defaultValue, CancellationToken cancellationToken = default)
        => Task.Run(() => source.FirstOrDefault(predicate, defaultValue), cancellationToken);

}