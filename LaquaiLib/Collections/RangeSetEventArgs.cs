#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace LaquaiLib.Collections;

/// <summary>
/// Represents the event arguments for the RangeSet event.
/// </summary>
public class RangeSetEventArgs
{
    /// <summary>
    /// Gets the starting index of the range being accessed.
    /// </summary>
    public int Index { get; }

    /// <summary>
    /// Gets the number of items in the range being accessed.
    /// </summary>
    public int Count { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="RangeSetEventArgs"/> class.
    /// </summary>
    /// <param name="index">The starting index of the range being accessed.</param>
    /// <param name="count">The number of items in the range being accessed.</param>
    public RangeSetEventArgs(int index, int count)
    {
        Index = index;
        Count = count;
    }
}
