namespace LaquaiLib.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="IGrouping{TKey, TElement}"/> Type.
/// </summary>
public static class IGroupingExtensions
{
    /// <summary>
    /// Deconstructs an <see cref="IGrouping{TKey, TElement}"/> into its <see cref="IGrouping{TKey, TElement}.Key"/> and elements as an <see cref="IEnumerable{T}"/>.
    /// </summary>
    /// <typeparam name="TKey">The Type of the key of the <see cref="IGrouping{TKey, TElement}"/>.</typeparam>
    /// <typeparam name="TElement">The Type of the elements of the <see cref="IGrouping{TKey, TElement}"/>.</typeparam>
    /// <param name="grouping">The <see cref="IGrouping{TKey, TElement}"/> to deconstruct.</param>
    /// <param name="key">An <see langword="out"/> variable that will be assigned the <see cref="IGrouping{TKey, TElement}.Key"/> of the <see cref="IGrouping{TKey, TElement}"/>.</param>
    /// <param name="elements">An <see langword="out"/> variable that will be assigned the elements of the <see cref="IGrouping{TKey, TElement}"/> as an <see cref="IEnumerable{T}"/>.</param>
    public static void Deconstruct<TKey, TElement>(this IGrouping<TKey, TElement> grouping, out TKey key, out IEnumerable<TElement> elements)
        => (key, elements) = (grouping.Key, grouping);

    /// <summary>
    /// Constructs a <see cref="Dictionary{TKey, TValue}"/> from an <see cref="IEnumerable{T}"/> of <see cref="IGrouping{TKey, TElement}"/>. The keys of the dictionary are the <see cref="IGrouping{TKey, TElement}.Key"/>s of the <see cref="IGrouping{TKey, TElement}"/>s, and the values are the values of the <see cref="IGrouping{TKey, TElement}"/>s as <see cref="List{T}"/>s to allow for adding more values.
    /// </summary>
    /// <typeparam name="TKey">The Type of the keys of the <see cref="IGrouping{TKey, TElement}"/>s.</typeparam>
    /// <typeparam name="TElement">The Type of the values of the <see cref="IGrouping{TKey, TElement}"/>s.</typeparam>
    /// <param name="source">The <see cref="IEnumerable{T}"/> of <see cref="IGrouping{TKey, TElement}"/>s to construct the <see cref="Dictionary{TKey, TValue}"/> from.</param>
    /// <returns>The constructed <see cref="Dictionary{TKey, TValue}"/>.</returns>
    public static Dictionary<TKey, List<TElement>> ToListDictionary<TKey, TElement>(this IEnumerable<IGrouping<TKey, TElement>> source)
        where TKey : notnull
    {
        var result = new Dictionary<TKey, List<TElement>>();
        foreach (var grouping in source)
        {
            result.Add(grouping.Key, [.. grouping]);
        }
        return result;
    }
    /// <summary>
    /// Constructs a <see cref="Dictionary{TKey, TValue}"/> from an <see cref="IEnumerable{T}"/> of <see cref="IGrouping{TKey, TElement}"/>. The keys of the dictionary are the <see cref="IGrouping{TKey, TElement}.Key"/>s of the <see cref="IGrouping{TKey, TElement}"/>s, and the values are the values of the <see cref="IGrouping{TKey, TElement}"/>s as <see cref="Array"/>s.
    /// </summary>
    /// <typeparam name="TKey">The Type of the keys of the <see cref="IGrouping{TKey, TElement}"/>s.</typeparam>
    /// <typeparam name="TElement">The Type of the values of the <see cref="IGrouping{TKey, TElement}"/>s.</typeparam>
    /// <param name="source">The <see cref="IEnumerable{T}"/> of <see cref="IGrouping{TKey, TElement}"/>s to construct the <see cref="Dictionary{TKey, TValue}"/> from.</param>
    /// <returns>The constructed <see cref="Dictionary{TKey, TValue}"/>.</returns>
    public static Dictionary<TKey, TElement[]> ToArrayDictionary<TKey, TElement>(this IEnumerable<IGrouping<TKey, TElement>> source)
        where TKey : notnull
    {
        var result = new Dictionary<TKey, TElement[]>();
        foreach (var grouping in source)
        {
            result.Add(grouping.Key, [.. grouping]);
        }
        return result;
    }

    /// <summary>
    /// Constructs a <see cref="Dictionary{TKey, TValue}"/> from an <see cref="IEnumerable{T}"/> of <see cref="IGrouping{TKey, TElement}"/>. The keys of the dictionary are the <see cref="IGrouping{TKey, TElement}.Key"/>s of the <see cref="IGrouping{TKey, TElement}"/>s, and the values are the values of the <see cref="IGrouping{TKey, TElement}"/>s as <typeparamref name="TCollection"/>s.
    /// </summary>
    /// <typeparam name="TKey">The Type of the keys of the <see cref="IGrouping{TKey, TElement}"/>s.</typeparam>
    /// <typeparam name="TCollection">The Type of the collections that will be used to store the values of the <see cref="IGrouping{TKey, TElement}"/>s.</typeparam>
    /// <typeparam name="TElement">The Type of the values of the <typeparamref name="TCollection"/>s.</typeparam>
    /// <param name="source">The <see cref="IEnumerable{T}"/> of <see cref="IGrouping{TKey, TElement}"/>s to construct the <see cref="Dictionary{TKey, TValue}"/> from.</param>
    /// <returns>The constructed <see cref="Dictionary{TKey, TValue}"/>.</returns>
    public static Dictionary<TKey, TCollection> ToDictionary<TKey, TCollection, TElement>(this IEnumerable<IGrouping<TKey, TElement>> source)
        where TKey : notnull
        where TCollection : ICollection<TElement>, new()
    {
        var result = new Dictionary<TKey, TCollection>();

        foreach (var grouping in source)
        {
            var collection = new TCollection();
            foreach (var element in grouping)
            {
                collection.Add(element);
            }
            result.Add(grouping.Key, collection);
        }
        return result;
    }

    /// <summary>
    /// Constructs a <see cref="Dictionary{TKey, TValue}"/> from an <see cref="IEnumerable{T}"/> of <see cref="IGrouping{TKey, TElement}"/>. The keys of the dictionary are the <see cref="IGrouping{TKey, TElement}.Key"/>s of the <see cref="IGrouping{TKey, TElement}"/>s, and the values are the values of the <see cref="IGrouping{TKey, TElement}"/>s transformed using a selector <see cref="Func{T, TResult}"/> as <see cref="List{T}"/>s to allow for adding more values.
    /// </summary>
    /// <typeparam name="TKey">The Type of the keys of the <see cref="IGrouping{TKey, TElement}"/>s.</typeparam>
    /// <typeparam name="TElement">The Type of the values of the <see cref="IGrouping{TKey, TElement}"/>s.</typeparam>
    /// <typeparam name="TTransform">The Type of the values of the <see cref="List{T}"/>s.</typeparam>
    /// <param name="source">The <see cref="IEnumerable{T}"/> of <see cref="IGrouping{TKey, TElement}"/>s to construct the <see cref="Dictionary{TKey, TValue}"/> from.</param>
    /// <param name="selector">A <see cref="Func{T, TResult}"/> that is passed each <typeparamref name="TElement"/> in the <see cref="IGrouping{TKey, TElement}"/>s and returns an instance of <typeparamref name="TTransform"/>.</param>
    /// <returns>The constructed <see cref="Dictionary{TKey, TValue}"/>.</returns>
    public static Dictionary<TKey, List<TTransform>> ToListDictionary<TKey, TElement, TTransform>(this IEnumerable<IGrouping<TKey, TElement>> source, Func<TElement, TTransform> selector)
        where TKey : notnull
    {
        var result = new Dictionary<TKey, List<TTransform>>();
        foreach (var grouping in source)
        {
            result.Add(grouping.Key, [.. grouping.Select(selector)]);
        }
        return result;
    }
    /// <summary>
    /// Constructs a <see cref="Dictionary{TKey, TValue}"/> from an <see cref="IEnumerable{T}"/> of <see cref="IGrouping{TKey, TElement}"/>. The keys of the dictionary are the <see cref="IGrouping{TKey, TElement}.Key"/>s of the <see cref="IGrouping{TKey, TElement}"/>s, and the values are the values of the <see cref="IGrouping{TKey, TElement}"/>s as <see cref="Array"/>s.
    /// </summary>
    /// <typeparam name="TKey">The Type of the keys of the <see cref="IGrouping{TKey, TElement}"/>s.</typeparam>
    /// <typeparam name="TElement">The Type of the values of the <see cref="IGrouping{TKey, TElement}"/>s.</typeparam>
    /// <typeparam name="TTransform">The Type of the values of the <see cref="List{T}"/>s.</typeparam>
    /// <param name="source">The <see cref="IEnumerable{T}"/> of <see cref="IGrouping{TKey, TElement}"/>s to construct the <see cref="Dictionary{TKey, TValue}"/> from.</param>
    /// <param name="selector">A <see cref="Func{T, TResult}"/> that is passed each <typeparamref name="TElement"/> in the <see cref="IGrouping{TKey, TElement}"/>s and returns an instance of <typeparamref name="TTransform"/>.</param>
    /// <returns>The constructed <see cref="Dictionary{TKey, TValue}"/>.</returns>
    public static Dictionary<TKey, TTransform[]> ToArrayDictionary<TKey, TElement, TTransform>(this IEnumerable<IGrouping<TKey, TElement>> source, Func<TElement, TTransform> selector)
        where TKey : notnull
    {
        var result = new Dictionary<TKey, TTransform[]>();
        foreach (var grouping in source)
        {
            result.Add(grouping.Key, [.. grouping.Select(selector)]);
        }
        return result;
    }

    /// <summary>
    /// Constructs a <see cref="Dictionary{TKey, TValue}"/> from an <see cref="IEnumerable{T}"/> of <see cref="IGrouping{TKey, TElement}"/>. The keys of the dictionary are the <see cref="IGrouping{TKey, TElement}.Key"/>s of the <see cref="IGrouping{TKey, TElement}"/>s, and the values are the values of the <see cref="IGrouping{TKey, TElement}"/>s as <typeparamref name="TCollection"/>s.
    /// </summary>
    /// <typeparam name="TKey">The Type of the keys of the <see cref="IGrouping{TKey, TElement}"/>s.</typeparam>
    /// <typeparam name="TCollection">The Type of the collections that will be used to store the values of the <see cref="IGrouping{TKey, TElement}"/>s.</typeparam>
    /// <typeparam name="TElement">The Type of the values of the <typeparamref name="TCollection"/>s.</typeparam>
    /// <typeparam name="TTransform">The Type of the values of the <see cref="List{T}"/>s.</typeparam>
    /// <param name="source">The <see cref="IEnumerable{T}"/> of <see cref="IGrouping{TKey, TElement}"/>s to construct the <see cref="Dictionary{TKey, TValue}"/> from.</param>
    /// <param name="selector">A <see cref="Func{T, TResult}"/> that is passed each <typeparamref name="TElement"/> in the <see cref="IGrouping{TKey, TElement}"/>s and returns an instance of <typeparamref name="TTransform"/>.</param>
    /// <returns>The constructed <see cref="Dictionary{TKey, TValue}"/>.</returns>
    public static Dictionary<TKey, TCollection> ToDictionary<TKey, TCollection, TElement, TTransform>(this IEnumerable<IGrouping<TKey, TElement>> source, Func<TElement, TTransform> selector)
        where TKey : notnull
        where TCollection : ICollection<TTransform>, new()
    {
        var result = new Dictionary<TKey, TCollection>();

        foreach (var grouping in source)
        {
            var collection = new TCollection();
            foreach (var element in grouping.Select(selector))
            {
                collection.Add(element);
            }
            result.Add(grouping.Key, collection);
        }
        return result;
    }
}
