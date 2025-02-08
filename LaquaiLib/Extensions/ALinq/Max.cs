namespace LaquaiLib.Extensions.ALinq;

// Provides parallel extensions for the System.Linq.Enumerable.Max family of methods.
public static partial class IEnumerableExtensions
{
    public static Task<int> MaxAsync(this IEnumerable<int> source, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Max(), cancellationToken);

    public static Task<long> MaxAsync(this IEnumerable<long> source, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Max(), cancellationToken);

    public static Task<int?> MaxAsync(this IEnumerable<int?> source, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Max(), cancellationToken);

    public static Task<long?> MaxAsync(this IEnumerable<long?> source, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Max(), cancellationToken);

    public static Task<double> MaxAsync(this IEnumerable<double> source, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Max(), cancellationToken);

    public static Task<double?> MaxAsync(this IEnumerable<double?> source, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Max(), cancellationToken);

    public static Task<float> MaxAsync(this IEnumerable<float> source, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Max(), cancellationToken);

    public static Task<float?> MaxAsync(this IEnumerable<float?> source, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Max(), cancellationToken);

    public static Task<decimal> MaxAsync(this IEnumerable<decimal> source, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Max(), cancellationToken);

    public static Task<decimal?> MaxAsync(this IEnumerable<decimal?> source, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Max(), cancellationToken);

    public static Task<TSource> MaxAsync<TSource>(this IEnumerable<TSource> source, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Max(), cancellationToken);

    public static Task<TSource> MaxAsync<TSource>(this IEnumerable<TSource> source, IComparer<TSource> comparer, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Max(comparer), cancellationToken);

    public static Task<int> MaxAsync<TSource>(this IEnumerable<TSource> source, Func<TSource, int> selector, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Max(selector), cancellationToken);

    public static Task<int?> MaxAsync<TSource>(this IEnumerable<TSource> source, Func<TSource, int?> selector, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Max(selector), cancellationToken);

    public static Task<long> MaxAsync<TSource>(this IEnumerable<TSource> source, Func<TSource, long> selector, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Max(selector), cancellationToken);

    public static Task<long?> MaxAsync<TSource>(this IEnumerable<TSource> source, Func<TSource, long?> selector, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Max(selector), cancellationToken);

    public static Task<float> MaxAsync<TSource>(this IEnumerable<TSource> source, Func<TSource, float> selector, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Max(selector), cancellationToken);

    public static Task<float?> MaxAsync<TSource>(this IEnumerable<TSource> source, Func<TSource, float?> selector, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Max(selector), cancellationToken);

    public static Task<double> MaxAsync<TSource>(this IEnumerable<TSource> source, Func<TSource, double> selector, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Max(selector), cancellationToken);

    public static Task<double?> MaxAsync<TSource>(this IEnumerable<TSource> source, Func<TSource, double?> selector, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Max(selector), cancellationToken);

    public static Task<decimal> MaxAsync<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal> selector, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Max(selector), cancellationToken);

    public static Task<decimal?> MaxAsync<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal?> selector, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Max(selector), cancellationToken);

    public static Task<TResult> MaxAsync<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Max(selector), cancellationToken);

}