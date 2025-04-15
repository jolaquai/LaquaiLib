
namespace LaquaiLib.Collections.Observable;

/// <summary>
/// Represents the event arguments for the RangeRemoved event.
/// </summary>
/// <typeparam name="T">The type of the items being removed.</typeparam>
public class RangeRemovedEventArgs<T>
{
    /// <summary>
    /// Gets the items that were removed.
    /// </summary>
    public IEnumerable<T> Items { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="RangeRemovedEventArgs{T}"/> class.
    /// </summary>
    /// <param name="items">The items that were removed.</param>
    public RangeRemovedEventArgs(IEnumerable<T> items)
    {
        Items = items;
    }
}
