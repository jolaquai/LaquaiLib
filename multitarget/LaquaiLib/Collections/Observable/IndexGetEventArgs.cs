
namespace LaquaiLib.Collections.Observable;

/// <summary>
/// Represents the event arguments for the IndexGet event.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="IndexGetEventArgs"/> class.
/// </remarks>
/// <param name="index">The index being accessed.</param>
public class IndexGetEventArgs(int index)
{
    /// <summary>
    /// Gets the index being accessed.
    /// </summary>
    public int Index { get; } = index;
}
