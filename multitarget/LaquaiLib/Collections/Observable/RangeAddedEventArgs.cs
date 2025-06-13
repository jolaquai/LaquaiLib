
namespace LaquaiLib.Collections.Observable;

/// <summary>
/// Represents the event arguments for the RangeAdded event.
/// </summary>
/// <typeparam name="T">The type of the items being added.</typeparam>
/// <remarks>
/// Initializes a new instance of the <see cref="RangeAddedEventArgs{T}"/> class.
/// </remarks>
/// <param name="items">The items that were added.</param>
public class RangeAddedEventArgs<T>(IEnumerable<T> items)
{
    /// <summary>
    /// Gets the items that were added.
    /// </summary>
    public IEnumerable<T> Items { get; } = items;
}
