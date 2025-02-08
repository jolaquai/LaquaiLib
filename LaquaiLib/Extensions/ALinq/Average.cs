namespace LaquaiLib.Extensions.ALinq;

// Provides parallel extensions for the System.Linq.Enumerable.Average family of methods.
public static partial class IEnumerableExtensions
{
    public static Task<double> AverageAsync(this IEnumerable<int> source, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Average(), cancellationToken);

    public static Task<double> AverageAsync(this IEnumerable<long> source, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Average(), cancellationToken);

    public static Task<float> AverageAsync(this IEnumerable<float> source, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Average(), cancellationToken);

    public static Task<double> AverageAsync(this IEnumerable<double> source, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Average(), cancellationToken);

    public static Task<decimal> AverageAsync(this IEnumerable<decimal> source, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Average(), cancellationToken);

    public static Task<double?> AverageAsync(this IEnumerable<int?> source, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Average(), cancellationToken);

    public static Task<double?> AverageAsync(this IEnumerable<long?> source, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Average(), cancellationToken);

    public static Task<float?> AverageAsync(this IEnumerable<float?> source, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Average(), cancellationToken);

    public static Task<double?> AverageAsync(this IEnumerable<double?> source, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Average(), cancellationToken);

    public static Task<decimal?> AverageAsync(this IEnumerable<decimal?> source, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Average(), cancellationToken);

    public static Task<double> AverageAsync<TSource>(this IEnumerable<TSource> source, Func<TSource, int> selector, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Average(selector), cancellationToken);

    public static Task<double> AverageAsync<TSource>(this IEnumerable<TSource> source, Func<TSource, long> selector, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Average(selector), cancellationToken);

    public static Task<float> AverageAsync<TSource>(this IEnumerable<TSource> source, Func<TSource, float> selector, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Average(selector), cancellationToken);

    public static Task<double> AverageAsync<TSource>(this IEnumerable<TSource> source, Func<TSource, double> selector, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Average(selector), cancellationToken);

    public static Task<decimal> AverageAsync<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal> selector, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Average(selector), cancellationToken);

    public static Task<double?> AverageAsync<TSource>(this IEnumerable<TSource> source, Func<TSource, int?> selector, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Average(selector), cancellationToken);

    public static Task<double?> AverageAsync<TSource>(this IEnumerable<TSource> source, Func<TSource, long?> selector, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Average(selector), cancellationToken);

    public static Task<float?> AverageAsync<TSource>(this IEnumerable<TSource> source, Func<TSource, float?> selector, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Average(selector), cancellationToken);

    public static Task<double?> AverageAsync<TSource>(this IEnumerable<TSource> source, Func<TSource, double?> selector, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Average(selector), cancellationToken);

    public static Task<decimal?> AverageAsync<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal?> selector, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Average(selector), cancellationToken);

}