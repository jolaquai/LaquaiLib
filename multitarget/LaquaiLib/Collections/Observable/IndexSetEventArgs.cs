
namespace LaquaiLib.Collections.Observable;

/// <summary>
/// Represents the event arguments for the IndexSet event.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="IndexSetEventArgs"/> class.
/// </remarks>
/// <param name="index">The index being accessed.</param>
public class IndexSetEventArgs(int index)
{
    /// <summary>
    /// Gets the index being accessed.
    /// </summary>
    public int Index { get; } = index;
}
