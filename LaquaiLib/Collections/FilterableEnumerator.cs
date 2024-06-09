using System.Collections;

namespace LaquaiLib.Collections;

/// <summary>
/// Implements an <see cref="IEnumerator{T}"/> that iterates over only the items that match a given predicate.
/// This implementation is essentially stateless; all caching is done through the internals of <see cref="IEnumerable{T}"/>.
/// </summary>
/// <typeparam name="T">The type of the items to iterate over.</typeparam>
public class FilterableEnumerator<T> : IEnumerator<T>
{
    private readonly IEnumerable<T> items;
    private readonly IEnumerator<T> filteredEnumerator;
    private readonly Func<T, bool> predicate;

    /// <summary>
    /// Initializes a new <see cref="FilterableEnumerator{T}"/>.
    /// </summary>
    /// <param name="items">The items to iterate over.</param>
    /// <param name="predicate">The predicate to filter the items by. If <see langword="null"/>, this instance will behave exactly like a regular <see cref="IEnumerator{T}"/>.</param>
    public FilterableEnumerator(IEnumerable<T> items, Func<T, bool>? predicate)
    {
        this.items = predicate is null ? items : items.Where(predicate);
        this.filteredEnumerator = this.items.GetEnumerator();
        this.predicate = predicate;
    }

    /// <inheritdoc/>
    public T Current => filteredEnumerator.Current;
    object IEnumerator.Current => Current;

    /// <inheritdoc/>
    public bool MoveNext() => filteredEnumerator.MoveNext();
    /// <inheritdoc/>
    public void Reset() => filteredEnumerator.Reset();
    /// <inheritdoc/>
    public void Dispose()
    {
        GC.SuppressFinalize(this);
        filteredEnumerator.Dispose();
    }
}
