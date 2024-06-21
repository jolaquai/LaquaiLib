using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace LaquaiLib.Collections;

/// <summary>
/// Contains factory and extension methods that create <see cref="FilterableEnumerator{T}"/> instances.
/// </summary>
public static class FilterableEnumerator
{
    /// <summary>
    /// Creates a <see cref="FilterableEnumerator{T}"/> that may be used to iterate over only the items that match a given predicate.
    /// </summary>
    /// <typeparam name="T">The type of the items to iterate over.</typeparam>
    /// <param name="items">The items to iterate over.</param>
    /// <param name="predicate">The predicate to filter the items by. If <see langword="null"/>, this instance will behave exactly like a regular <see cref="IEnumerator{T}"/>.</param>
    /// <returns>The created <see cref="FilterableEnumerator{T}"/>.</returns>
    public static FilterableEnumerator<T> GetEnumerator<T>(this IEnumerable<T> items, [DisallowNull] Func<T, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(predicate);
        return new FilterableEnumerator<T>(items, predicate);
    }
}

/// <summary>
/// Implements an <see cref="IEnumerator{T}"/> that iterates over only the items that match a given predicate.
/// This implementation is stateless; all caching is done through the internals of <see cref="IEnumerable{T}"/>, meaning this enumerator may be reused.
/// </summary>
/// <typeparam name="T">The type of the items to iterate over.</typeparam>
public readonly struct FilterableEnumerator<T> : IEnumerator<T>
{
    private readonly IEnumerable<T> items;
    private readonly IEnumerator<T> filteredEnumerator;

    /// <summary>
    /// Initializes a new <see cref="FilterableEnumerator{T}"/> with no items to iterate over.
    /// </summary>
    public FilterableEnumerator()
    {
        items = [];
        filteredEnumerator = items.GetEnumerator();
    }
    /// <summary>
    /// Initializes a new <see cref="FilterableEnumerator{T}"/> that behaves exactly like a regular <see cref="IEnumerator{T}"/>.
    /// </summary>
    /// <param name="items">The items to iterate over.</param>
    public FilterableEnumerator(IEnumerable<T> items)
    {
        this.items = items;
        filteredEnumerator = items.GetEnumerator();
    }
    /// <summary>
    /// Initializes a new <see cref="FilterableEnumerator{T}"/>.
    /// </summary>
    /// <param name="items">The items to iterate over.</param>
    /// <param name="predicate">The predicate to filter the items by. If <see langword="null"/>, this instance will behave exactly like a regular <see cref="IEnumerator{T}"/>.</param>
    public FilterableEnumerator(IEnumerable<T> items, Func<T, bool>? predicate)
    {
        this.items = predicate is null ? items : items.Where(predicate);
        filteredEnumerator = this.items.GetEnumerator();
    }
    /// <inheritdoc cref="FilterableEnumerator{T}.FilterableEnumerator(IEnumerable{T}, Func{T, bool}?)"/>
    public FilterableEnumerator(IEnumerable<T> items, Func<T, int, bool>? predicate)
    {
        this.items = predicate is null ? items : items.Where(predicate);
        filteredEnumerator = this.items.GetEnumerator();
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
