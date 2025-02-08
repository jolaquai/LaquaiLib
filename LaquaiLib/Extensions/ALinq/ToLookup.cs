namespace LaquaiLib.Extensions.ALinq;

// Provides parallel extensions for the System.Linq.Enumerable.ToLookup family of methods.
public static partial class IEnumerableExtensions
{
    public static Task<ILookup<TKey, TSource>> ToLookupAsync<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, CancellationToken cancellationToken = default)
        => Task.Run(() => source.ToLookup(keySelector), cancellationToken);

    public static Task<ILookup<TKey, TSource>> ToLookupAsync<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer, CancellationToken cancellationToken = default)
        => Task.Run(() => source.ToLookup(keySelector, comparer), cancellationToken);

    public static Task<ILookup<TKey, TElement>> ToLookupAsync<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, CancellationToken cancellationToken = default)
        => Task.Run(() => source.ToLookup(keySelector, elementSelector), cancellationToken);

    public static Task<ILookup<TKey, TElement>> ToLookupAsync<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey> comparer, CancellationToken cancellationToken = default)
        => Task.Run(() => source.ToLookup(keySelector, elementSelector, comparer), cancellationToken);

}