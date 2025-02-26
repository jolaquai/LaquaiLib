namespace LaquaiLib.Extensions.ALinq;

/// <summary>
/// Provides parallel extensions for the Enumerable.Sum family of methods.
/// Note that none of these methods parallelize the consumption of the query, it is merely started in a new <see cref="Task" />.
/// </summary>
public static partial class IEnumerableExtensions
{
    /// <inheritdoc cref="Enumerable.Sum(IEnumerable{int})" />
    public static Task<int> SumAsync(this IEnumerable<int> source, CancellationToken cancellationToken = default)
        => Task.Run(source.Sum, cancellationToken);

    /// <inheritdoc cref="Enumerable.Sum(IEnumerable{long})" />
    public static Task<long> SumAsync(this IEnumerable<long> source, CancellationToken cancellationToken = default)
        => Task.Run(source.Sum, cancellationToken);

    /// <inheritdoc cref="Enumerable.Sum(IEnumerable{float})" />
    public static Task<float> SumAsync(this IEnumerable<float> source, CancellationToken cancellationToken = default)
        => Task.Run(source.Sum, cancellationToken);

    /// <inheritdoc cref="Enumerable.Sum(IEnumerable{double})" />
    public static Task<double> SumAsync(this IEnumerable<double> source, CancellationToken cancellationToken = default)
        => Task.Run(source.Sum, cancellationToken);

    /// <inheritdoc cref="Enumerable.Sum(IEnumerable{decimal})" />
    public static Task<decimal> SumAsync(this IEnumerable<decimal> source, CancellationToken cancellationToken = default)
        => Task.Run(source.Sum, cancellationToken);

    /// <inheritdoc cref="Enumerable.Sum(IEnumerable{int?})" />
    public static Task<int?> SumAsync(this IEnumerable<int?> source, CancellationToken cancellationToken = default)
        => Task.Run(source.Sum, cancellationToken);

    /// <inheritdoc cref="Enumerable.Sum(IEnumerable{long?})" />
    public static Task<long?> SumAsync(this IEnumerable<long?> source, CancellationToken cancellationToken = default)
        => Task.Run(source.Sum, cancellationToken);

    /// <inheritdoc cref="Enumerable.Sum(IEnumerable{float?})" />
    public static Task<float?> SumAsync(this IEnumerable<float?> source, CancellationToken cancellationToken = default)
        => Task.Run(source.Sum, cancellationToken);

    /// <inheritdoc cref="Enumerable.Sum(IEnumerable{double?})" />
    public static Task<double?> SumAsync(this IEnumerable<double?> source, CancellationToken cancellationToken = default)
        => Task.Run(source.Sum, cancellationToken);

    /// <inheritdoc cref="Enumerable.Sum(IEnumerable{decimal?})" />
    public static Task<decimal?> SumAsync(this IEnumerable<decimal?> source, CancellationToken cancellationToken = default)
        => Task.Run(source.Sum, cancellationToken);

    /// <inheritdoc cref="Enumerable.Sum{TSource}(IEnumerable{TSource}, Func{TSource, int})" />
    public static Task<int> SumAsync<TSource>(this IEnumerable<TSource> source, Func<TSource, int> selector, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Sum(selector), cancellationToken);

    /// <inheritdoc cref="Enumerable.Sum{TSource}(IEnumerable{TSource}, Func{TSource, long})" />
    public static Task<long> SumAsync<TSource>(this IEnumerable<TSource> source, Func<TSource, long> selector, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Sum(selector), cancellationToken);

    /// <inheritdoc cref="Enumerable.Sum{TSource}(IEnumerable{TSource}, Func{TSource, float})" />
    public static Task<float> SumAsync<TSource>(this IEnumerable<TSource> source, Func<TSource, float> selector, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Sum(selector), cancellationToken);

    /// <inheritdoc cref="Enumerable.Sum{TSource}(IEnumerable{TSource}, Func{TSource, double})" />
    public static Task<double> SumAsync<TSource>(this IEnumerable<TSource> source, Func<TSource, double> selector, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Sum(selector), cancellationToken);

    /// <inheritdoc cref="Enumerable.Sum{TSource}(IEnumerable{TSource}, Func{TSource, decimal})" />
    public static Task<decimal> SumAsync<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal> selector, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Sum(selector), cancellationToken);

    /// <inheritdoc cref="Enumerable.Sum{TSource}(IEnumerable{TSource}, Func{TSource, int?})" />
    public static Task<int?> SumAsync<TSource>(this IEnumerable<TSource> source, Func<TSource, int?> selector, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Sum(selector), cancellationToken);

    /// <inheritdoc cref="Enumerable.Sum{TSource}(IEnumerable{TSource}, Func{TSource, long?})" />
    public static Task<long?> SumAsync<TSource>(this IEnumerable<TSource> source, Func<TSource, long?> selector, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Sum(selector), cancellationToken);

    /// <inheritdoc cref="Enumerable.Sum{TSource}(IEnumerable{TSource}, Func{TSource, float?})" />
    public static Task<float?> SumAsync<TSource>(this IEnumerable<TSource> source, Func<TSource, float?> selector, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Sum(selector), cancellationToken);

    /// <inheritdoc cref="Enumerable.Sum{TSource}(IEnumerable{TSource}, Func{TSource, double?})" />
    public static Task<double?> SumAsync<TSource>(this IEnumerable<TSource> source, Func<TSource, double?> selector, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Sum(selector), cancellationToken);

    /// <inheritdoc cref="Enumerable.Sum{TSource}(IEnumerable{TSource}, Func{TSource, decimal?})" />
    public static Task<decimal?> SumAsync<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal?> selector, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Sum(selector), cancellationToken);

}