namespace LaquaiLib.Extensions.ALinq;

// Provides parallel extensions for the System.Linq.Enumerable.ToDictionary family of methods.
public static partial class IEnumerableExtensions
{
    public static Task<Dictionary<TKey, TValue>> ToDictionaryAsync<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> source, CancellationToken cancellationToken = default)
        => Task.Run(() => source.ToDictionary(), cancellationToken);

    public static Task<Dictionary<TKey, TValue>> ToDictionaryAsync<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> source, IEqualityComparer<TKey> comparer, CancellationToken cancellationToken = default)
        => Task.Run(() => source.ToDictionary(comparer), cancellationToken);

    public static Task<Dictionary<TKey, TValue>> ToDictionaryAsync<TKey, TValue>(this IEnumerable<ValueTuple<TKey, TValue>> source, CancellationToken cancellationToken = default)
        => Task.Run(() => source.ToDictionary(), cancellationToken);

    public static Task<Dictionary<TKey, TValue>> ToDictionaryAsync<TKey, TValue>(this IEnumerable<ValueTuple<TKey, TValue>> source, IEqualityComparer<TKey> comparer, CancellationToken cancellationToken = default)
        => Task.Run(() => source.ToDictionary(comparer), cancellationToken);

    public static Task<Dictionary<TKey, TSource>> ToDictionaryAsync<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, CancellationToken cancellationToken = default)
        => Task.Run(() => source.ToDictionary(keySelector), cancellationToken);

    public static Task<Dictionary<TKey, TSource>> ToDictionaryAsync<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer, CancellationToken cancellationToken = default)
        => Task.Run(() => source.ToDictionary(keySelector, comparer), cancellationToken);

    public static Task<Dictionary<TKey, TElement>> ToDictionaryAsync<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, CancellationToken cancellationToken = default)
        => Task.Run(() => source.ToDictionary(keySelector, elementSelector), cancellationToken);

    public static Task<Dictionary<TKey, TElement>> ToDictionaryAsync<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey> comparer, CancellationToken cancellationToken = default)
        => Task.Run(() => source.ToDictionary(keySelector, elementSelector, comparer), cancellationToken);

}