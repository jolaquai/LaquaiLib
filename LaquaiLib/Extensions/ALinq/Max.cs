namespace LaquaiLib.Extensions.ALinq;

/// <summary>
/// Provides parallel extensions for the Enumerable.Max family of methods.
/// Note that none of these methods parallelize the consumption of the query, it is merely started in a new <see cref="Task" />.
/// </summary>
public static partial class IEnumerableExtensions
{
    /// <inheritdoc cref="Enumerable.Max(IEnumerable{int})" />
    public static Task<int> MaxAsync(this IEnumerable<int> source, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Max(), cancellationToken);

    /// <inheritdoc cref="Enumerable.Max(IEnumerable{long})" />
    public static Task<long> MaxAsync(this IEnumerable<long> source, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Max(), cancellationToken);

    /// <inheritdoc cref="Enumerable.Max(IEnumerable{int?})" />
    public static Task<int?> MaxAsync(this IEnumerable<int?> source, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Max(), cancellationToken);

    /// <inheritdoc cref="Enumerable.Max(IEnumerable{long?})" />
    public static Task<long?> MaxAsync(this IEnumerable<long?> source, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Max(), cancellationToken);

    /// <inheritdoc cref="Enumerable.Max(IEnumerable{double})" />
    public static Task<double> MaxAsync(this IEnumerable<double> source, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Max(), cancellationToken);

    /// <inheritdoc cref="Enumerable.Max(IEnumerable{double?})" />
    public static Task<double?> MaxAsync(this IEnumerable<double?> source, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Max(), cancellationToken);

    /// <inheritdoc cref="Enumerable.Max(IEnumerable{float})" />
    public static Task<float> MaxAsync(this IEnumerable<float> source, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Max(), cancellationToken);

    /// <inheritdoc cref="Enumerable.Max(IEnumerable{float?})" />
    public static Task<float?> MaxAsync(this IEnumerable<float?> source, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Max(), cancellationToken);

    /// <inheritdoc cref="Enumerable.Max(IEnumerable{decimal})" />
    public static Task<decimal> MaxAsync(this IEnumerable<decimal> source, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Max(), cancellationToken);

    /// <inheritdoc cref="Enumerable.Max(IEnumerable{decimal?})" />
    public static Task<decimal?> MaxAsync(this IEnumerable<decimal?> source, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Max(), cancellationToken);

    /// <inheritdoc cref="Enumerable.Max{TSource}(IEnumerable{TSource})" />
    public static Task<TSource> MaxAsync<TSource>(this IEnumerable<TSource> source, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Max(), cancellationToken);

    /// <inheritdoc cref="Enumerable.Max{TSource}(IEnumerable{TSource}, IComparer{TSource})" />
    public static Task<TSource> MaxAsync<TSource>(this IEnumerable<TSource> source, IComparer<TSource> comparer, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Max(comparer), cancellationToken);

    /// <inheritdoc cref="Enumerable.Max{TSource}(IEnumerable{TSource}, Func{TSource, int})" />
    public static Task<int> MaxAsync<TSource>(this IEnumerable<TSource> source, Func<TSource, int> selector, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Max(selector), cancellationToken);

    /// <inheritdoc cref="Enumerable.Max{TSource}(IEnumerable{TSource}, Func{TSource, int?})" />
    public static Task<int?> MaxAsync<TSource>(this IEnumerable<TSource> source, Func<TSource, int?> selector, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Max(selector), cancellationToken);

    /// <inheritdoc cref="Enumerable.Max{TSource}(IEnumerable{TSource}, Func{TSource, long})" />
    public static Task<long> MaxAsync<TSource>(this IEnumerable<TSource> source, Func<TSource, long> selector, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Max(selector), cancellationToken);

    /// <inheritdoc cref="Enumerable.Max{TSource}(IEnumerable{TSource}, Func{TSource, long?})" />
    public static Task<long?> MaxAsync<TSource>(this IEnumerable<TSource> source, Func<TSource, long?> selector, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Max(selector), cancellationToken);

    /// <inheritdoc cref="Enumerable.Max{TSource}(IEnumerable{TSource}, Func{TSource, float})" />
    public static Task<float> MaxAsync<TSource>(this IEnumerable<TSource> source, Func<TSource, float> selector, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Max(selector), cancellationToken);

    /// <inheritdoc cref="Enumerable.Max{TSource}(IEnumerable{TSource}, Func{TSource, float?})" />
    public static Task<float?> MaxAsync<TSource>(this IEnumerable<TSource> source, Func<TSource, float?> selector, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Max(selector), cancellationToken);

    /// <inheritdoc cref="Enumerable.Max{TSource}(IEnumerable{TSource}, Func{TSource, double})" />
    public static Task<double> MaxAsync<TSource>(this IEnumerable<TSource> source, Func<TSource, double> selector, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Max(selector), cancellationToken);

    /// <inheritdoc cref="Enumerable.Max{TSource}(IEnumerable{TSource}, Func{TSource, double?})" />
    public static Task<double?> MaxAsync<TSource>(this IEnumerable<TSource> source, Func<TSource, double?> selector, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Max(selector), cancellationToken);

    /// <inheritdoc cref="Enumerable.Max{TSource}(IEnumerable{TSource}, Func{TSource, decimal})" />
    public static Task<decimal> MaxAsync<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal> selector, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Max(selector), cancellationToken);

    /// <inheritdoc cref="Enumerable.Max{TSource}(IEnumerable{TSource}, Func{TSource, decimal?})" />
    public static Task<decimal?> MaxAsync<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal?> selector, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Max(selector), cancellationToken);

    /// <inheritdoc cref="Enumerable.Max{TSource, TResult}(IEnumerable{TSource}, Func{TSource, TResult})" />
    public static Task<TResult> MaxAsync<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Max(selector), cancellationToken);

}