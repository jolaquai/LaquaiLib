namespace LaquaiLib.Extensions;

public static partial class IEnumerableExtensions
{
    /// <summary>
    /// Sorts the elements of a sequence in ascending order according to a key extracted from each element.
    /// </summary>
    /// <typeparam name="T">The Type of the elements in the input sequence.</typeparam>
    /// <param name="source">The input sequence to sort.</param>
    /// <param name="keySelector">The <see cref="Func{T1, T2, TResult}"/> that is passed each element of the input sequence and its index in the original sequence and produces a key to use for sorting.</param>
    /// <param name="comparer">An <see cref="IComparer{T}"/> implementation to use for comparing the keys.</param>
    /// <returns>An <see cref="IOrderedEnumerable{TElement}"/> that iterates over the sorted input sequence.</returns>
    public static IOrderedEnumerable<TItem> OrderBy<TItem, TKey>(this IEnumerable<TItem> source, Func<TItem, int, TKey> keySelector, IComparer<TKey> comparer = null)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(keySelector);

        var i = 0;
        return source.OrderBy(e => keySelector(e, i++), comparer);
    }
    /// <summary>
    /// Augments the sort order of a previously sorted sequence according to a key extracted from each element.
    /// </summary>
    /// <typeparam name="T">The Type of the elements in the input sequence.</typeparam>
    /// <param name="source">The input sequence to sort.</param>
    /// <param name="keySelector">The <see cref="Func{T1, T2, TResult}"/> that is passed each element of the input sequence and its index in the sequence and produces a key to use for sorting.</param>
    /// <param name="comparer">An <see cref="IComparer{T}"/> implementation to use for comparing the keys.</param>
    /// <returns>An <see cref="IOrderedEnumerable{TElement}"/> that iterates over the sorted input sequence.</returns>
    public static IOrderedEnumerable<TItem> ThenBy<TItem, TKey>(this IOrderedEnumerable<TItem> source, Func<TItem, int, TKey> keySelector, IComparer<TKey> comparer = null)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(keySelector);

        var i = 0;
        return source.ThenBy(e => keySelector(e, i++), comparer);
    }
    /// <summary>
    /// Sorts the elements of a sequence in descending order according to a key extracted from each element.
    /// </summary>
    /// <typeparam name="T">The Type of the elements in the input sequence.</typeparam>
    /// <param name="source">The input sequence to sort.</param>
    /// <param name="keySelector">The <see cref="Func{T1, T2, TResult}"/> that is passed each element of the input sequence and its index in the original sequence and produces a key to use for sorting.</param>
    /// <param name="comparer">An <see cref="IComparer{T}"/> implementation to use for comparing the keys.</param>
    /// <returns>An <see cref="IOrderedEnumerable{TElement}"/> that iterates over the sorted input sequence.</returns>
    public static IOrderedEnumerable<TItem> OrderByDescending<TItem, TKey>(this IEnumerable<TItem> source, Func<TItem, int, TKey> keySelector, IComparer<TKey> comparer = null)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(keySelector);

        var i = 0;
        return source.OrderByDescending(e => keySelector(e, i++), comparer);
    }
    /// <summary>
    /// Augments the sort order of a previously sorted sequence according to a key extracted from each element.
    /// </summary>
    /// <typeparam name="T">The Type of the elements in the input sequence.</typeparam>
    /// <param name="source">The input sequence to sort.</param>
    /// <param name="keySelector">The <see cref="Func{T1, T2, TResult}"/> that is passed each element of the input sequence and its index in the sequence and produces a key to use for sorting.</param>
    /// <param name="comparer">An <see cref="IComparer{T}"/> implementation to use for comparing the keys.</param>
    /// <returns>An <see cref="IOrderedEnumerable{TElement}"/> that iterates over the sorted input sequence.</returns>
    public static IOrderedEnumerable<TItem> ThenByDescending<TItem, TKey>(this IOrderedEnumerable<TItem> source, Func<TItem, int, TKey> keySelector, IComparer<TKey> comparer = null)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(keySelector);

        var i = 0;
        return source.ThenByDescending(e => keySelector(e, i++), comparer);
    }

    /// <summary>
    /// Sorts the elements of a sequence in ascending order according to another sequence that specifies the keys to use for sorting.
    /// </summary>
    /// <typeparam name="T">The Type of the elements in the sequences.</typeparam>
    /// <param name="source">The input sequence to sort.</param>
    /// <param name="keys">The sequence that specifies the keys to use for sorting.</param>
    /// <param name="comparer">An <see cref="IComparer{T}"/> implementation to use for comparing the keys.</param>
    /// <returns>The sorted input sequence.</returns>
    public static IOrderedEnumerable<TItem> OrderBy<TItem, TKey>(this IEnumerable<TItem> source, IEnumerable<TKey> keys, IComparer<TKey> comparer = null)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(keys);

        var enumeratedKeys = keys as IList<TKey> ?? keys.ToArray();
        return source.OrderBy((_, i) => enumeratedKeys[i], comparer);
    }
    /// <summary>
    /// Augments the sort order of a previously sorted sequence using the specified sequence that specifies the keys to use for sorting.
    /// </summary>
    /// <typeparam name="T">The Type of the elements in the sequences.</typeparam>
    /// <param name="source">The input sequence to sort.</param>
    /// <param name="keys">The sequence that specifies the keys to use for sorting.</param>
    /// <param name="comparer">An <see cref="IComparer{T}"/> implementation to use for comparing the keys.</param>
    /// <returns>The sorted input sequence.</returns>
    public static IOrderedEnumerable<TItem> ThenBy<TItem, TKey>(this IOrderedEnumerable<TItem> source, IEnumerable<TKey> keys, IComparer<TKey> comparer = null)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(keys);

        var enumeratedKeys = keys as IList<TKey> ?? keys.ToArray();
        return source.ThenBy((_, i) => enumeratedKeys[i], comparer);
    }
    /// <summary>
    /// Sorts the elements of a sequence in descending order according to another sequence that specifies the keys to use for sorting.
    /// </summary>
    /// <typeparam name="T">The Type of the elements in the sequences.</typeparam>
    /// <param name="source">The input sequence to sort.</param>
    /// <param name="keys">The sequence that specifies the keys to use for sorting.</param>
    /// <param name="comparer">An <see cref="IComparer{T}"/> implementation to use for comparing the keys.</param>
    /// <returns>The sorted input sequence.</returns>
    public static IOrderedEnumerable<TItem> OrderByDescending<TItem, TKey>(this IEnumerable<TItem> source, IEnumerable<TKey> keys, IComparer<TKey> comparer = null)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(keys);

        var enumeratedKeys = keys as IList<TKey> ?? keys.ToArray();
        return source.OrderByDescending((_, i) => enumeratedKeys[i], comparer);
    }
    /// <summary>
    /// Augments the sort order of a previously sorted sequence using the specified sequence that specifies the keys to use for sorting.
    /// </summary>
    /// <typeparam name="T">The Type of the elements in the sequences.</typeparam>
    /// <param name="source">The input sequence to sort.</param>
    /// <param name="keys">The sequence that specifies the keys to use for sorting.</param>
    /// <param name="comparer">An <see cref="IComparer{T}"/> implementation to use for comparing the keys.</param>
    /// <returns>The sorted input sequence.</returns>
    public static IOrderedEnumerable<TItem> ThenByDescending<TItem, TKey>(this IOrderedEnumerable<TItem> source, IEnumerable<TKey> keys, IComparer<TKey> comparer = null)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(keys);

        var enumeratedKeys = keys as IList<TKey> ?? keys.ToArray();
        return source.ThenByDescending((_, i) => enumeratedKeys[i], comparer);
    }
}
