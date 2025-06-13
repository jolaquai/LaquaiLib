
namespace LaquaiLib.Collections.Observable;

/// <summary>
/// Represents the event arguments for the Added event.
/// </summary>
/// <typeparam name="T">The type of the item being added.</typeparam>
/// <remarks>
/// Initializes a new instance of the <see cref="AddedEventArgs{T}"/> class.
/// </remarks>
/// <param name="item">The item that was added.</param>
public class AddedEventArgs<T>(T item)
{
    /// <summary>
    /// Gets the item that was added.
    /// </summary>
    public T Item { get; } = item;
}
