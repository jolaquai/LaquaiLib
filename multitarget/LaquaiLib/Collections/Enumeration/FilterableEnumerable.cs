namespace LaquaiLib.Collections.Enumeration;

/// <summary>
/// Implements the enumerator pattern to iterate over only the items that match a given predicate.
/// This implementation is stateless; all caching is done through the internals of <see cref="IEnumerable{T}"/>, meaning this enumerator may be reused.
/// </summary>
/// <typeparam name="T">The type of the items to iterate over.</typeparam>
/// <remarks>
/// Initializes a new <see cref="FilterableEnumerable{T}"/> that iterates over all items in the given collection.
/// </remarks>
/// <param name="items">The type of the items to iterate over.</param>
public struct FilterableEnumerable<T>(IEnumerable<T> items)
{
    private readonly IEnumerable<T> _items = items;

    /// <summary>
    /// Retrieves the current item at which the enumerator is positioned.
    /// </summary>
    public T Current { get; private set; }

    /// <summary>
    /// Returns the current instance. For use in <see langword="foreach"/> statements.
    /// </summary>
    public readonly IEnumerator<T> GetEnumerator()
    {
        foreach (var item in _items)
        {
            yield return item;
        }
    }

    /// <summary>
    /// Initializes a new <see cref="FilterableEnumerable{T}"/> with no items to iterate over.
    /// </summary>
    public FilterableEnumerable() : this([])
    {
    }

    /// <summary>
    /// Initializes a new <see cref="FilterableEnumerable{T}"/>.
    /// </summary>
    /// <param name="items">The items to iterate over.</param>
    /// <param name="predicate">The predicate to filter the items by. If <see langword="null"/>, this instance will iterate over all items in the collection.</param>
    public FilterableEnumerable(IEnumerable<T> items, Func<T, bool> predicate) : this(predicate is null ? items : items.Where(predicate))
    {
    }
    /// <inheritdoc cref="FilterableEnumerable{T}.FilterableEnumerable(IEnumerable{T}, Func{T, bool}?)"/>
    public FilterableEnumerable(IEnumerable<T> items, Func<T, int, bool> predicate) : this(predicate is null ? items : items.Where(predicate))
    {
    }
}
