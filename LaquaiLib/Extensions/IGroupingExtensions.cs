namespace LaquaiLib.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="IGrouping{TKey, TElement}"/> Type.
/// </summary>
public static class IGroupingExtensions
{
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
}
