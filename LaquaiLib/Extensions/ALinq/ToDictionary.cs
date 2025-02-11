namespace LaquaiLib.Extensions.ALinq;

/// <summary>
/// Provides parallel extensions for the Enumerable.ToDictionary family of methods.
/// Note that none of these methods parallelize the consumption of the query, it is merely started in a new <see cref="Task" />.
/// </summary>
public static partial class IEnumerableExtensions
{
    /// <inheritdoc cref="Enumerable.ToDictionary{TKey, TValue}(IEnumerable{KeyValuePair{TKey, TValue}})" />
    public static Task<Dictionary<TKey, TValue>> ToDictionaryAsync<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> source, CancellationToken cancellationToken = default)
        => Task.Run(() => source.ToDictionary(), cancellationToken);

    /// <inheritdoc cref="Enumerable.ToDictionary{TKey, TValue}(IEnumerable{KeyValuePair{TKey, TValue}}, IEqualityComparer{TKey})" />
    public static Task<Dictionary<TKey, TValue>> ToDictionaryAsync<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> source, IEqualityComparer<TKey> comparer, CancellationToken cancellationToken = default)
        => Task.Run(() => source.ToDictionary(comparer), cancellationToken);

    /// <inheritdoc cref="Enumerable.ToDictionary{TKey, TValue}(IEnumerable{ValueTuple{TKey, TValue}})" />
    public static Task<Dictionary<TKey, TValue>> ToDictionaryAsync<TKey, TValue>(this IEnumerable<ValueTuple<TKey, TValue>> source, CancellationToken cancellationToken = default)
        => Task.Run(() => source.ToDictionary(), cancellationToken);

    /// <inheritdoc cref="Enumerable.ToDictionary{TKey, TValue}(IEnumerable{ValueTuple{TKey, TValue}}, IEqualityComparer{TKey})" />
    public static Task<Dictionary<TKey, TValue>> ToDictionaryAsync<TKey, TValue>(this IEnumerable<ValueTuple<TKey, TValue>> source, IEqualityComparer<TKey> comparer, CancellationToken cancellationToken = default)
        => Task.Run(() => source.ToDictionary(comparer), cancellationToken);

    /// <inheritdoc cref="Enumerable.ToDictionary{TSource, TKey}(IEnumerable{TSource}, Func{TSource, TKey})" />
    public static Task<Dictionary<TKey, TSource>> ToDictionaryAsync<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, CancellationToken cancellationToken = default)
        => Task.Run(() => source.ToDictionary(keySelector), cancellationToken);

    /// <inheritdoc cref="Enumerable.ToDictionary{TSource, TKey}(IEnumerable{TSource}, Func{TSource, TKey}, IEqualityComparer{TKey})" />
    public static Task<Dictionary<TKey, TSource>> ToDictionaryAsync<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer, CancellationToken cancellationToken = default)
        => Task.Run(() => source.ToDictionary(keySelector, comparer), cancellationToken);

    /// <inheritdoc cref="Enumerable.ToDictionary{TSource, TKey, TElement}(IEnumerable{TSource}, Func{TSource, TKey}, Func{TSource, TElement})" />
    public static Task<Dictionary<TKey, TElement>> ToDictionaryAsync<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, CancellationToken cancellationToken = default)
        => Task.Run(() => source.ToDictionary(keySelector, elementSelector), cancellationToken);

    /// <inheritdoc cref="Enumerable.ToDictionary{TSource, TKey, TElement}(IEnumerable{TSource}, Func{TSource, TKey}, Func{TSource, TElement}, IEqualityComparer{TKey})" />
    public static Task<Dictionary<TKey, TElement>> ToDictionaryAsync<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey> comparer, CancellationToken cancellationToken = default)
        => Task.Run(() => source.ToDictionary(keySelector, elementSelector, comparer), cancellationToken);

}