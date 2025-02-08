namespace LaquaiLib.Extensions.ALinq;

// Provides parallel extensions for the System.Linq.Enumerable.SequenceEqual family of methods.
public static partial class IEnumerableExtensions
{
    public static Task<bool> SequenceEqualAsync<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second, CancellationToken cancellationToken = default)
        => Task.Run(() => first.SequenceEqual(second), cancellationToken);

    public static Task<bool> SequenceEqualAsync<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource> comparer, CancellationToken cancellationToken = default)
        => Task.Run(() => first.SequenceEqual(second, comparer), cancellationToken);

}