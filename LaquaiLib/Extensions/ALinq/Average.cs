namespace LaquaiLib.Extensions.ALinq;

/// <summary>
/// Provides parallel extensions for the Enumerable.Average family of methods.
/// Note that none of these methods parallelize the consumption of the query, it is merely started in a new <see cref="Task" />.
/// </summary>
public static partial class IEnumerableExtensions
{
    /// <inheritdoc cref="Enumerable.Average(IEnumerable{int})" />
    public static Task<double> AverageAsync(this IEnumerable<int> source, CancellationToken cancellationToken = default)
        => Task.Run(source.Average, cancellationToken);

    /// <inheritdoc cref="Enumerable.Average(IEnumerable{long})" />
    public static Task<double> AverageAsync(this IEnumerable<long> source, CancellationToken cancellationToken = default)
        => Task.Run(source.Average, cancellationToken);

    /// <inheritdoc cref="Enumerable.Average(IEnumerable{float})" />
    public static Task<float> AverageAsync(this IEnumerable<float> source, CancellationToken cancellationToken = default)
        => Task.Run(source.Average, cancellationToken);

    /// <inheritdoc cref="Enumerable.Average(IEnumerable{double})" />
    public static Task<double> AverageAsync(this IEnumerable<double> source, CancellationToken cancellationToken = default)
        => Task.Run(source.Average, cancellationToken);

    /// <inheritdoc cref="Enumerable.Average(IEnumerable{decimal})" />
    public static Task<decimal> AverageAsync(this IEnumerable<decimal> source, CancellationToken cancellationToken = default)
        => Task.Run(source.Average, cancellationToken);

    /// <inheritdoc cref="Enumerable.Average(IEnumerable{int?})" />
    public static Task<double?> AverageAsync(this IEnumerable<int?> source, CancellationToken cancellationToken = default)
        => Task.Run(source.Average, cancellationToken);

    /// <inheritdoc cref="Enumerable.Average(IEnumerable{long?})" />
    public static Task<double?> AverageAsync(this IEnumerable<long?> source, CancellationToken cancellationToken = default)
        => Task.Run(source.Average, cancellationToken);

    /// <inheritdoc cref="Enumerable.Average(IEnumerable{float?})" />
    public static Task<float?> AverageAsync(this IEnumerable<float?> source, CancellationToken cancellationToken = default)
        => Task.Run(source.Average, cancellationToken);

    /// <inheritdoc cref="Enumerable.Average(IEnumerable{double?})" />
    public static Task<double?> AverageAsync(this IEnumerable<double?> source, CancellationToken cancellationToken = default)
        => Task.Run(source.Average, cancellationToken);

    /// <inheritdoc cref="Enumerable.Average(IEnumerable{decimal?})" />
    public static Task<decimal?> AverageAsync(this IEnumerable<decimal?> source, CancellationToken cancellationToken = default)
        => Task.Run(source.Average, cancellationToken);

    /// <inheritdoc cref="Enumerable.Average{TSource}(IEnumerable{TSource}, Func{TSource, int})" />
    public static Task<double> AverageAsync<TSource>(this IEnumerable<TSource> source, Func<TSource, int> selector, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Average(selector), cancellationToken);

    /// <inheritdoc cref="Enumerable.Average{TSource}(IEnumerable{TSource}, Func{TSource, long})" />
    public static Task<double> AverageAsync<TSource>(this IEnumerable<TSource> source, Func<TSource, long> selector, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Average(selector), cancellationToken);

    /// <inheritdoc cref="Enumerable.Average{TSource}(IEnumerable{TSource}, Func{TSource, float})" />
    public static Task<float> AverageAsync<TSource>(this IEnumerable<TSource> source, Func<TSource, float> selector, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Average(selector), cancellationToken);

    /// <inheritdoc cref="Enumerable.Average{TSource}(IEnumerable{TSource}, Func{TSource, double})" />
    public static Task<double> AverageAsync<TSource>(this IEnumerable<TSource> source, Func<TSource, double> selector, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Average(selector), cancellationToken);

    /// <inheritdoc cref="Enumerable.Average{TSource}(IEnumerable{TSource}, Func{TSource, decimal})" />
    public static Task<decimal> AverageAsync<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal> selector, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Average(selector), cancellationToken);

    /// <inheritdoc cref="Enumerable.Average{TSource}(IEnumerable{TSource}, Func{TSource, int?})" />
    public static Task<double?> AverageAsync<TSource>(this IEnumerable<TSource> source, Func<TSource, int?> selector, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Average(selector), cancellationToken);

    /// <inheritdoc cref="Enumerable.Average{TSource}(IEnumerable{TSource}, Func{TSource, long?})" />
    public static Task<double?> AverageAsync<TSource>(this IEnumerable<TSource> source, Func<TSource, long?> selector, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Average(selector), cancellationToken);

    /// <inheritdoc cref="Enumerable.Average{TSource}(IEnumerable{TSource}, Func{TSource, float?})" />
    public static Task<float?> AverageAsync<TSource>(this IEnumerable<TSource> source, Func<TSource, float?> selector, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Average(selector), cancellationToken);

    /// <inheritdoc cref="Enumerable.Average{TSource}(IEnumerable{TSource}, Func{TSource, double?})" />
    public static Task<double?> AverageAsync<TSource>(this IEnumerable<TSource> source, Func<TSource, double?> selector, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Average(selector), cancellationToken);

    /// <inheritdoc cref="Enumerable.Average{TSource}(IEnumerable{TSource}, Func{TSource, decimal?})" />
    public static Task<decimal?> AverageAsync<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal?> selector, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Average(selector), cancellationToken);

}