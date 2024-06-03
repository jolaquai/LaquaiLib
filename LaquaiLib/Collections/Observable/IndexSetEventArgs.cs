
namespace LaquaiLib.Collections.Observable;

/// <summary>
/// Represents the event arguments for the IndexSet event.
/// </summary>
public class IndexSetEventArgs
{
    /// <summary>
    /// Gets the index being accessed.
    /// </summary>
    public int Index { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="IndexSetEventArgs"/> class.
    /// </summary>
    /// <param name="index">The index being accessed.</param>
    public IndexSetEventArgs(int index)
    {
        Index = index;
    }
}
