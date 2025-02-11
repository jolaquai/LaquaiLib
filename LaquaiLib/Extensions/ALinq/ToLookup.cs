namespace LaquaiLib.Extensions.ALinq;

/// <summary>
/// Provides parallel extensions for the Enumerable.ToLookup family of methods.
/// Note that none of these methods parallelize the consumption of the query, it is merely started in a new <see cref="Task" />.
/// </summary>
public static partial class IEnumerableExtensions
{
    /// <inheritdoc cref="Enumerable.ToLookup{TSource, TKey}(IEnumerable{TSource}, Func{TSource, TKey})" />
    public static Task<ILookup<TKey, TSource>> ToLookupAsync<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, CancellationToken cancellationToken = default)
        => Task.Run(() => source.ToLookup(keySelector), cancellationToken);

    /// <inheritdoc cref="Enumerable.ToLookup{TSource, TKey}(IEnumerable{TSource}, Func{TSource, TKey}, IEqualityComparer{TKey})" />
    public static Task<ILookup<TKey, TSource>> ToLookupAsync<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer, CancellationToken cancellationToken = default)
        => Task.Run(() => source.ToLookup(keySelector, comparer), cancellationToken);

    /// <inheritdoc cref="Enumerable.ToLookup{TSource, TKey, TElement}(IEnumerable{TSource}, Func{TSource, TKey}, Func{TSource, TElement})" />
    public static Task<ILookup<TKey, TElement>> ToLookupAsync<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, CancellationToken cancellationToken = default)
        => Task.Run(() => source.ToLookup(keySelector, elementSelector), cancellationToken);

    /// <inheritdoc cref="Enumerable.ToLookup{TSource, TKey, TElement}(IEnumerable{TSource}, Func{TSource, TKey}, Func{TSource, TElement}, IEqualityComparer{TKey})" />
    public static Task<ILookup<TKey, TElement>> ToLookupAsync<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey> comparer, CancellationToken cancellationToken = default)
        => Task.Run(() => source.ToLookup(keySelector, elementSelector, comparer), cancellationToken);

}