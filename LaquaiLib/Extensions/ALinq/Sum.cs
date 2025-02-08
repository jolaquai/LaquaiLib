namespace LaquaiLib.Extensions.ALinq;

// Provides parallel extensions for the System.Linq.Enumerable.Sum family of methods.
public static partial class IEnumerableExtensions
{
    public static Task<int> SumAsync(this IEnumerable<int> source, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Sum(), cancellationToken);

    public static Task<long> SumAsync(this IEnumerable<long> source, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Sum(), cancellationToken);

    public static Task<float> SumAsync(this IEnumerable<float> source, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Sum(), cancellationToken);

    public static Task<double> SumAsync(this IEnumerable<double> source, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Sum(), cancellationToken);

    public static Task<decimal> SumAsync(this IEnumerable<decimal> source, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Sum(), cancellationToken);

    public static Task<int?> SumAsync(this IEnumerable<int?> source, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Sum(), cancellationToken);

    public static Task<long?> SumAsync(this IEnumerable<long?> source, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Sum(), cancellationToken);

    public static Task<float?> SumAsync(this IEnumerable<float?> source, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Sum(), cancellationToken);

    public static Task<double?> SumAsync(this IEnumerable<double?> source, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Sum(), cancellationToken);

    public static Task<decimal?> SumAsync(this IEnumerable<decimal?> source, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Sum(), cancellationToken);

    public static Task<int> SumAsync<TSource>(this IEnumerable<TSource> source, Func<TSource, int> selector, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Sum(selector), cancellationToken);

    public static Task<long> SumAsync<TSource>(this IEnumerable<TSource> source, Func<TSource, long> selector, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Sum(selector), cancellationToken);

    public static Task<float> SumAsync<TSource>(this IEnumerable<TSource> source, Func<TSource, float> selector, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Sum(selector), cancellationToken);

    public static Task<double> SumAsync<TSource>(this IEnumerable<TSource> source, Func<TSource, double> selector, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Sum(selector), cancellationToken);

    public static Task<decimal> SumAsync<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal> selector, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Sum(selector), cancellationToken);

    public static Task<int?> SumAsync<TSource>(this IEnumerable<TSource> source, Func<TSource, int?> selector, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Sum(selector), cancellationToken);

    public static Task<long?> SumAsync<TSource>(this IEnumerable<TSource> source, Func<TSource, long?> selector, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Sum(selector), cancellationToken);

    public static Task<float?> SumAsync<TSource>(this IEnumerable<TSource> source, Func<TSource, float?> selector, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Sum(selector), cancellationToken);

    public static Task<double?> SumAsync<TSource>(this IEnumerable<TSource> source, Func<TSource, double?> selector, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Sum(selector), cancellationToken);

    public static Task<decimal?> SumAsync<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal?> selector, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Sum(selector), cancellationToken);

}