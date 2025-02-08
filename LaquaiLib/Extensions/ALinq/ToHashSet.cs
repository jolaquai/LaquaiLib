namespace LaquaiLib.Extensions.ALinq;

// Provides parallel extensions for the System.Linq.Enumerable.ToHashSet family of methods.
public static partial class IEnumerableExtensions
{
    public static Task<HashSet<TSource>> ToHashSetAsync<TSource>(this IEnumerable<TSource> source, CancellationToken cancellationToken = default)
        => Task.Run(() => source.ToHashSet(), cancellationToken);

    public static Task<HashSet<TSource>> ToHashSetAsync<TSource>(this IEnumerable<TSource> source, IEqualityComparer<TSource> comparer, CancellationToken cancellationToken = default)
        => Task.Run(() => source.ToHashSet(comparer), cancellationToken);

}