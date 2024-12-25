namespace LaquaiLib.Extensions;

partial class IEnumerableExtensions
{
    /// <summary>
    /// Sorts the elements of a sequence in ascending order according to a key extracted from each element.
    /// </summary>
    /// <typeparam name="T">The Type of the elements in the input sequence.</typeparam>
    /// <param name="source">The input sequence to sort.</param>
    /// <param name="keySelector">The <see cref="Func{T1, T2, TResult}"/> that is passed each element of the input sequence and its index in the original sequence and produces a key to use for sorting.</param>
    /// <returns>An <see cref="IOrderedEnumerable{TElement}"/> that iterates over the sorted input sequence.</returns>
    public static IOrderedEnumerable<T> OrderBy<T>(this IEnumerable<T> source, Func<T, int, T> keySelector)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(keySelector);

        var i = 0;
        return source.OrderBy(e => keySelector(e, i++));
    }
    /// <summary>
    /// Augments the sort order of a previously sorted sequence according to a key extracted from each element.
    /// </summary>
    /// <typeparam name="T">The Type of the elements in the input sequence.</typeparam>
    /// <param name="source">The input sequence to sort.</param>
    /// <param name="keySelector">The <see cref="Func{T1, T2, TResult}"/> that is passed each element of the input sequence and its index in the sequence and produces a key to use for sorting.</param>
    /// <returns>An <see cref="IOrderedEnumerable{TElement}"/> that iterates over the sorted input sequence.</returns>
    public static IOrderedEnumerable<T> ThenBy<T>(this IOrderedEnumerable<T> source, Func<T, int, T> keySelector)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(keySelector);

        var i = 0;
        return source.ThenBy(e => keySelector(e, i++));
    }
    /// <summary>
    /// Sorts the elements of a sequence in descending order according to a key extracted from each element.
    /// </summary>
    /// <typeparam name="T">The Type of the elements in the input sequence.</typeparam>
    /// <param name="source">The input sequence to sort.</param>
    /// <param name="keySelector">The <see cref="Func{T1, T2, TResult}"/> that is passed each element of the input sequence and its index in the original sequence and produces a key to use for sorting.</param>
    /// <returns>An <see cref="IOrderedEnumerable{TElement}"/> that iterates over the sorted input sequence.</returns>
    public static IOrderedEnumerable<T> OrderByDescending<T>(this IEnumerable<T> source, Func<T, int, T> keySelector)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(keySelector);

        var i = 0;
        return source.OrderByDescending(e => keySelector(e, i++));
    }
    /// <summary>
    /// Augments the sort order of a previously sorted sequence according to a key extracted from each element.
    /// </summary>
    /// <typeparam name="T">The Type of the elements in the input sequence.</typeparam>
    /// <param name="source">The input sequence to sort.</param>
    /// <param name="keySelector">The <see cref="Func{T1, T2, TResult}"/> that is passed each element of the input sequence and its index in the sequence and produces a key to use for sorting.</param>
    /// <returns>An <see cref="IOrderedEnumerable{TElement}"/> that iterates over the sorted input sequence.</returns>
    public static IOrderedEnumerable<T> ThenByDescending<T>(this IOrderedEnumerable<T> source, Func<T, int, T> keySelector)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(keySelector);

        var i = 0;
        return source.ThenByDescending(e => keySelector(e, i++));
    }

    /// <summary>
    /// Orders the elements of a sequence in ascending order according to a key extracted from each element.
    /// </summary>
    /// <typeparam name="T">The Type of the elements in the input sequence.</typeparam>
    /// <param name="source">The input sequence to sort.</param>
    /// <param name="keySelectors">The <see cref="Func{T1, T2, TResult}"/>s that are passed each element of the input sequence produce a key to use for sorting.</param>
    /// <returns>The ordered input sequence.</returns>
    public static IOrderedEnumerable<T> OrderByMultiple<T>(this IEnumerable<T> source, params ReadOnlySpan<Func<T, T>> keySelectors)
    {
        ArgumentNullException.ThrowIfNull(source);
        if (keySelectors.Length == 0)
        {
            return (IOrderedEnumerable<T>)source;
        }
        var ordered = source.OrderBy(keySelectors[0]);
        foreach (var selector in keySelectors[1..])
        {
            ordered = ordered.ThenBy(selector);
        }
        return ordered;
    }
    /// <summary>
    /// Orders the elements of a sequence in ascending order according to a key extracted from each element.
    /// Each selector is passed the element and the index of that element from the last iteration.
    /// </summary>
    /// <typeparam name="T">The Type of the elements in the input sequence.</typeparam>
    /// <param name="source">The input sequence to sort.</param>
    /// <param name="keySelectors">The <see cref="Func{T1, T2, TResult}"/>s that are passed each element of the input sequence and its position from the last iteration and produce a key to use for sorting.</param>
    /// <returns>The ordered input sequence.</returns>
    public static IOrderedEnumerable<T> OrderByMultiple<T>(this IEnumerable<T> source, params ReadOnlySpan<Func<T, int, T>> keySelectors)
    {
        ArgumentNullException.ThrowIfNull(source);
        var firstSelector = keySelectors[0];
        var ordered = source.Index().OrderBy(tuple => firstSelector(tuple.Item, tuple.Index));
        foreach (var selector in keySelectors[1..])
        {
            ordered = ordered.ThenBy(tuple => selector(tuple.Item, tuple.Index));
        }
        return (IOrderedEnumerable<T>)ordered.Select(tuple => tuple.Item);
    }

    /// <summary>
    /// Sorts the elements of a sequence in ascending order according to another sequence that specifies the keys to use for sorting.
    /// </summary>
    /// <typeparam name="T">The Type of the elements in the sequences.</typeparam>
    /// <param name="source">The input sequence to sort.</param>
    /// <param name="keys">The sequence that specifies the keys to use for sorting.</param>
    /// <returns>The sorted input sequence.</returns>
    public static IOrderedEnumerable<T> OrderBy<T>(this IEnumerable<T> source, IEnumerable<T> keys)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(keys);

        var enumeratedKeys = keys as IList<T> ?? keys.ToArray();
        return source.OrderBy((_, i) => enumeratedKeys[i]);
    }
    /// <summary>
    /// Augments the sort order of a previously sorted sequence using the specified sequence that specifies the keys to use for sorting.
    /// </summary>
    /// <typeparam name="T">The Type of the elements in the sequences.</typeparam>
    /// <param name="source">The input sequence to sort.</param>
    /// <param name="keys">The sequence that specifies the keys to use for sorting.</param>
    /// <returns>The sorted input sequence.</returns>
    public static IOrderedEnumerable<T> ThenBy<T>(this IOrderedEnumerable<T> source, IEnumerable<T> keys)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(keys);

        var enumeratedKeys = keys as IList<T> ?? keys.ToArray();
        return source.ThenBy((_, i) => enumeratedKeys[i]);
    }
    /// <summary>
    /// Sorts the elements of a sequence in descending order according to another sequence that specifies the keys to use for sorting.
    /// </summary>
    /// <typeparam name="T">The Type of the elements in the sequences.</typeparam>
    /// <param name="source">The input sequence to sort.</param>
    /// <param name="keys">The sequence that specifies the keys to use for sorting.</param>
    /// <returns>The sorted input sequence.</returns>
    public static IOrderedEnumerable<T> OrderByDescending<T>(this IEnumerable<T> source, IEnumerable<T> keys)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(keys);

        var enumeratedKeys = keys as IList<T> ?? keys.ToArray();
        return source.OrderByDescending((_, i) => enumeratedKeys[i]);
    }
    /// <summary>
    /// Augments the sort order of a previously sorted sequence using the specified sequence that specifies the keys to use for sorting.
    /// </summary>
    /// <typeparam name="T">The Type of the elements in the sequences.</typeparam>
    /// <param name="source">The input sequence to sort.</param>
    /// <param name="keys">The sequence that specifies the keys to use for sorting.</param>
    /// <returns>The sorted input sequence.</returns>
    public static IOrderedEnumerable<T> ThenByDescending<T>(this IOrderedEnumerable<T> source, IEnumerable<T> keys)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(keys);

        var enumeratedKeys = keys as IList<T> ?? keys.ToArray();
        return source.ThenByDescending((_, i) => enumeratedKeys[i]);
    }
}
