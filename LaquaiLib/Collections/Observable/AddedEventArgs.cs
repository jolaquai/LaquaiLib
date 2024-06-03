
namespace LaquaiLib.Collections.Observable;

/// <summary>
/// Represents the event arguments for the Added event.
/// </summary>
/// <typeparam name="T">The type of the item being added.</typeparam>
public class AddedEventArgs<T>
{
    /// <summary>
    /// Gets the item that was added.
    /// </summary>
    public T Item { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AddedEventArgs{T}"/> class.
    /// </summary>
    /// <param name="item">The item that was added.</param>
    public AddedEventArgs(T item)
    {
        Item = item;
    }
}
