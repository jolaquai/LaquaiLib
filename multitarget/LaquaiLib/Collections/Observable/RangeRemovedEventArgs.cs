
namespace LaquaiLib.Collections.Observable;

/// <summary>
/// Represents the event arguments for the RangeRemoved event.
/// </summary>
/// <typeparam name="T">The type of the items being removed.</typeparam>
/// <remarks>
/// Initializes a new instance of the <see cref="RangeRemovedEventArgs{T}"/> class.
/// </remarks>
/// <param name="items">The items that were removed.</param>
public class RangeRemovedEventArgs<T>(IEnumerable<T> items)
{
    /// <summary>
    /// Gets the items that were removed.
    /// </summary>
    public IEnumerable<T> Items { get; } = items;
}
