using System.Runtime.InteropServices;

namespace LaquaiLib.Extensions;

public static partial class IEnumerableExtensions
{
    /// <summary>
    /// Determines if a sequence contains less than the specified number of elements.
    /// </summary>
    /// <typeparam name="T">The Type of the elements in the input sequence.</typeparam>
    /// <param name="source">The input sequence.</param>
    /// <param name="n">The number of elements to check for.</param>
    /// <returns><see langword="true"/> if the input sequence contains less than <paramref name="n"/> elements, otherwise <see langword="false"/>.</returns>
    public static bool HasLessThan<T>(this IEnumerable<T> source, int n) => (source.TryGetNonEnumeratedCount(out var count) ? count : source.Count()) < n;
    /// <summary>
    /// Determines if a sequence contains less than or exactly the specified number of elements.
    /// </summary>
    /// <typeparam name="T">The Type of the elements in the input sequence.</typeparam>
    /// <param name="source">The input sequence.</param>
    /// <param name="n">The number of elements to check for.</param>
    /// <returns><see langword="true"/> if the input sequence contains less than or exactly <paramref name="n"/> elements, otherwise <see langword="false"/>.</returns>
    public static bool HasLessThanOr<T>(this IEnumerable<T> source, int n) => (source.TryGetNonEnumeratedCount(out var count) ? count : source.Count()) <= n;
    /// <summary>
    /// Determines if a sequence contains exactly the specified number of elements.
    /// An attempt is made to avoid enumerating the input sequence, but if this fails, it is done regardless.
    /// </summary>
    /// <typeparam name="T">The Type of the elements in the input sequence.</typeparam>
    /// <param name="source">The input sequence.</param>
    /// <param name="n">The number of elements to check for.</param>
    /// <returns>The result of the check.</returns>
    public static bool HasExactly<T>(this IEnumerable<T> source, int n) => (source.TryGetNonEnumeratedCount(out var count) ? count : source.Count()) == n;
    /// <summary>
    /// Determines if a sequence contains more than or exactly the specified number of elements.
    /// </summary>
    /// <typeparam name="T">The Type of the elements in the input sequence.</typeparam>
    /// <param name="source">The input sequence.</param>
    /// <param name="n">The number of elements to check for.</param>
    /// <returns><see langword="true"/> if the input sequence contains more than or exactly <paramref name="n"/> elements, otherwise <see langword="false"/>.</returns>
    public static bool HasMoreThanOr<T>(this IEnumerable<T> source, int n) => (source.TryGetNonEnumeratedCount(out var count) ? count : source.Count()) >= n;
    /// <summary>
    /// Determines if a sequence contains more than the specified number of elements.
    /// </summary>
    /// <typeparam name="T">The Type of the elements in the input sequence.</typeparam>
    /// <param name="source">The input sequence.</param>
    /// <param name="n">The number of elements to check for.</param>
    /// <returns><see langword="true"/> if the input sequence contains more than <paramref name="n"/> elements, otherwise <see langword="false"/>.</returns>
    public static bool HasMoreThan<T>(this IEnumerable<T> source, int n) => (source.TryGetNonEnumeratedCount(out var count) ? count : source.Count()) > n;

    /// <summary>
    /// Builds a <see cref="Dictionary{TKey, TValue}"/>, mapping the item to the number of times it appears in the input sequence.
    /// </summary>
    /// <typeparam name="TItem">The Type of the items in the input sequence.</typeparam>
    /// <param name="source">The input sequence.</param>
    /// <param name="comparer">The <see cref="IEqualityComparer{T}"/> implementation to use when comparing items.</param>
    /// <returns>The resulting <see cref="Dictionary{TKey, TValue}"/>.</returns>
    public static Dictionary<TItem, int> Counts<TItem>(this IEnumerable<TItem> source, IEqualityComparer<TItem> comparer = null)
    {
        comparer ??= EqualityComparer<TItem>.Default;
        var counts = new Dictionary<TItem, int>(comparer);

        foreach (var item in source)
        {
            CollectionsMarshal.GetValueRefOrAddDefault(counts, item, out _)++;
        }
        return counts;
    }
    /// <summary>
    /// Builds a <see cref="Dictionary{TKey, TValue}"/>, mapping the item to the number of times it appears as counted using a <paramref name="selector"/> function in the input sequence.
    /// </summary>
    /// <typeparam name="TItem">The Type of the items in the input sequence.</typeparam>
    /// <param name="source">The input sequence.</param>
    /// <param name="selector">A <see cref="Func{T, TResult}"/> that produces the keys to count.</param>
    /// <param name="comparer">The <see cref="IEqualityComparer{T}"/> implementation to use when comparing items.</param>
    /// <returns>The resulting <see cref="Dictionary{TKey, TValue}"/>.</returns>
    public static Dictionary<TItem, int> CountsBy<TItem, TSelect>(this IEnumerable<TItem> source, Func<TItem, TSelect> selector, IEqualityComparer<TSelect> comparer = null)
    {
        ArgumentNullException.ThrowIfNull(selector);

        comparer ??= EqualityComparer<TSelect>.Default;
        var counts = new Dictionary<TSelect, (TItem, int)>(comparer);

        foreach (var item in source)
        {
            var key = selector(item);
            ref var theRef = ref CollectionsMarshal.GetValueRefOrAddDefault(counts, key, out _);
            theRef.Item1 ??= item;
            theRef.Item2++;
        }
        return counts.ToDictionary(t => t.Value.Item1, t => t.Value.Item2);
    }
}