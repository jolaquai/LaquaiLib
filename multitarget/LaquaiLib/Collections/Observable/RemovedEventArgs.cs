
namespace LaquaiLib.Collections.Observable;

/// <summary>
/// Represents the event arguments for the Removed event.
/// </summary>
/// <typeparam name="T">The type of the item being removed.</typeparam>
/// <remarks>
/// Initializes a new instance of the <see cref="RemovedEventArgs{T}"/> class.
/// </remarks>
/// <param name="item">The item that was removed.</param>
public class RemovedEventArgs<T>(T item)
{
    /// <summary>
    /// Gets the item that was removed.
    /// </summary>
    public T Item { get; } = item;
}
