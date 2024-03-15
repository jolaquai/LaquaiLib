#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace LaquaiLib.Collections;

/// <summary>
/// Represents the event arguments for the IndexGet event.
/// </summary>
public class IndexGetEventArgs
{
    /// <summary>
    /// Gets the index being accessed.
    /// </summary>
    public int Index { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="IndexGetEventArgs"/> class.
    /// </summary>
    /// <param name="index">The index being accessed.</param>
    public IndexGetEventArgs(int index)
    {
        Index = index;
    }
}
