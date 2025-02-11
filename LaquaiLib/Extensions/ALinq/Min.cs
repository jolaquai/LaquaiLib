namespace LaquaiLib.Extensions.ALinq;

/// <summary>
/// Provides parallel extensions for the Enumerable.Min family of methods.
/// Note that none of these methods parallelize the consumption of the query, it is merely started in a new <see cref="Task" />.
/// </summary>
public static partial class IEnumerableExtensions
{
    /// <inheritdoc cref="Enumerable.Min(IEnumerable{int})" />
    public static Task<int> MinAsync(this IEnumerable<int> source, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Min(), cancellationToken);

    /// <inheritdoc cref="Enumerable.Min(IEnumerable{long})" />
    public static Task<long> MinAsync(this IEnumerable<long> source, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Min(), cancellationToken);

    /// <inheritdoc cref="Enumerable.Min(IEnumerable{int?})" />
    public static Task<int?> MinAsync(this IEnumerable<int?> source, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Min(), cancellationToken);

    /// <inheritdoc cref="Enumerable.Min(IEnumerable{long?})" />
    public static Task<long?> MinAsync(this IEnumerable<long?> source, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Min(), cancellationToken);

    /// <inheritdoc cref="Enumerable.Min(IEnumerable{float})" />
    public static Task<float> MinAsync(this IEnumerable<float> source, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Min(), cancellationToken);

    /// <inheritdoc cref="Enumerable.Min(IEnumerable{float?})" />
    public static Task<float?> MinAsync(this IEnumerable<float?> source, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Min(), cancellationToken);

    /// <inheritdoc cref="Enumerable.Min(IEnumerable{double})" />
    public static Task<double> MinAsync(this IEnumerable<double> source, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Min(), cancellationToken);

    /// <inheritdoc cref="Enumerable.Min(IEnumerable{double?})" />
    public static Task<double?> MinAsync(this IEnumerable<double?> source, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Min(), cancellationToken);

    /// <inheritdoc cref="Enumerable.Min(IEnumerable{decimal})" />
    public static Task<decimal> MinAsync(this IEnumerable<decimal> source, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Min(), cancellationToken);

    /// <inheritdoc cref="Enumerable.Min(IEnumerable{decimal?})" />
    public static Task<decimal?> MinAsync(this IEnumerable<decimal?> source, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Min(), cancellationToken);

    /// <inheritdoc cref="Enumerable.Min{TSource}(IEnumerable{TSource})" />
    public static Task<TSource> MinAsync<TSource>(this IEnumerable<TSource> source, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Min(), cancellationToken);

    /// <inheritdoc cref="Enumerable.Min{TSource}(IEnumerable{TSource}, IComparer{TSource})" />
    public static Task<TSource> MinAsync<TSource>(this IEnumerable<TSource> source, IComparer<TSource> comparer, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Min(comparer), cancellationToken);

    /// <inheritdoc cref="Enumerable.Min{TSource}(IEnumerable{TSource}, Func{TSource, int})" />
    public static Task<int> MinAsync<TSource>(this IEnumerable<TSource> source, Func<TSource, int> selector, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Min(selector), cancellationToken);

    /// <inheritdoc cref="Enumerable.Min{TSource}(IEnumerable{TSource}, Func{TSource, int?})" />
    public static Task<int?> MinAsync<TSource>(this IEnumerable<TSource> source, Func<TSource, int?> selector, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Min(selector), cancellationToken);

    /// <inheritdoc cref="Enumerable.Min{TSource}(IEnumerable{TSource}, Func{TSource, long})" />
    public static Task<long> MinAsync<TSource>(this IEnumerable<TSource> source, Func<TSource, long> selector, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Min(selector), cancellationToken);

    /// <inheritdoc cref="Enumerable.Min{TSource}(IEnumerable{TSource}, Func{TSource, long?})" />
    public static Task<long?> MinAsync<TSource>(this IEnumerable<TSource> source, Func<TSource, long?> selector, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Min(selector), cancellationToken);

    /// <inheritdoc cref="Enumerable.Min{TSource}(IEnumerable{TSource}, Func{TSource, float})" />
    public static Task<float> MinAsync<TSource>(this IEnumerable<TSource> source, Func<TSource, float> selector, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Min(selector), cancellationToken);

    /// <inheritdoc cref="Enumerable.Min{TSource}(IEnumerable{TSource}, Func{TSource, float?})" />
    public static Task<float?> MinAsync<TSource>(this IEnumerable<TSource> source, Func<TSource, float?> selector, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Min(selector), cancellationToken);

    /// <inheritdoc cref="Enumerable.Min{TSource}(IEnumerable{TSource}, Func{TSource, double})" />
    public static Task<double> MinAsync<TSource>(this IEnumerable<TSource> source, Func<TSource, double> selector, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Min(selector), cancellationToken);

    /// <inheritdoc cref="Enumerable.Min{TSource}(IEnumerable{TSource}, Func{TSource, double?})" />
    public static Task<double?> MinAsync<TSource>(this IEnumerable<TSource> source, Func<TSource, double?> selector, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Min(selector), cancellationToken);

    /// <inheritdoc cref="Enumerable.Min{TSource}(IEnumerable{TSource}, Func{TSource, decimal})" />
    public static Task<decimal> MinAsync<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal> selector, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Min(selector), cancellationToken);

    /// <inheritdoc cref="Enumerable.Min{TSource}(IEnumerable{TSource}, Func{TSource, decimal?})" />
    public static Task<decimal?> MinAsync<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal?> selector, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Min(selector), cancellationToken);

    /// <inheritdoc cref="Enumerable.Min{TSource, TResult}(IEnumerable{TSource}, Func{TSource, TResult})" />
    public static Task<TResult> MinAsync<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Min(selector), cancellationToken);

}