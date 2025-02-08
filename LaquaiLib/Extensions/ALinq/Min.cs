namespace LaquaiLib.Extensions.ALinq;

// Provides parallel extensions for the System.Linq.Enumerable.Min family of methods.
public static partial class IEnumerableExtensions
{
    public static Task<int> MinAsync(this IEnumerable<int> source, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Min(), cancellationToken);

    public static Task<long> MinAsync(this IEnumerable<long> source, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Min(), cancellationToken);

    public static Task<int?> MinAsync(this IEnumerable<int?> source, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Min(), cancellationToken);

    public static Task<long?> MinAsync(this IEnumerable<long?> source, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Min(), cancellationToken);

    public static Task<float> MinAsync(this IEnumerable<float> source, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Min(), cancellationToken);

    public static Task<float?> MinAsync(this IEnumerable<float?> source, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Min(), cancellationToken);

    public static Task<double> MinAsync(this IEnumerable<double> source, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Min(), cancellationToken);

    public static Task<double?> MinAsync(this IEnumerable<double?> source, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Min(), cancellationToken);

    public static Task<decimal> MinAsync(this IEnumerable<decimal> source, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Min(), cancellationToken);

    public static Task<decimal?> MinAsync(this IEnumerable<decimal?> source, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Min(), cancellationToken);

    public static Task<TSource> MinAsync<TSource>(this IEnumerable<TSource> source, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Min(), cancellationToken);

    public static Task<TSource> MinAsync<TSource>(this IEnumerable<TSource> source, IComparer<TSource> comparer, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Min(comparer), cancellationToken);

    public static Task<int> MinAsync<TSource>(this IEnumerable<TSource> source, Func<TSource, int> selector, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Min(selector), cancellationToken);

    public static Task<int?> MinAsync<TSource>(this IEnumerable<TSource> source, Func<TSource, int?> selector, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Min(selector), cancellationToken);

    public static Task<long> MinAsync<TSource>(this IEnumerable<TSource> source, Func<TSource, long> selector, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Min(selector), cancellationToken);

    public static Task<long?> MinAsync<TSource>(this IEnumerable<TSource> source, Func<TSource, long?> selector, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Min(selector), cancellationToken);

    public static Task<float> MinAsync<TSource>(this IEnumerable<TSource> source, Func<TSource, float> selector, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Min(selector), cancellationToken);

    public static Task<float?> MinAsync<TSource>(this IEnumerable<TSource> source, Func<TSource, float?> selector, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Min(selector), cancellationToken);

    public static Task<double> MinAsync<TSource>(this IEnumerable<TSource> source, Func<TSource, double> selector, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Min(selector), cancellationToken);

    public static Task<double?> MinAsync<TSource>(this IEnumerable<TSource> source, Func<TSource, double?> selector, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Min(selector), cancellationToken);

    public static Task<decimal> MinAsync<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal> selector, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Min(selector), cancellationToken);

    public static Task<decimal?> MinAsync<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal?> selector, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Min(selector), cancellationToken);

    public static Task<TResult> MinAsync<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector, CancellationToken cancellationToken = default)
        => Task.Run(() => source.Min(selector), cancellationToken);

}