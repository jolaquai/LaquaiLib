
namespace LaquaiLib.Collections.Observable;

/// <summary>
/// Represents the event arguments for the RangeGet event.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="RangeGetEventArgs"/> class.
/// </remarks>
/// <param name="index">The starting index of the range being accessed.</param>
/// <param name="count">The number of items in the range being accessed.</param>
public class RangeGetEventArgs(int index, int count)
{
    /// <summary>
    /// Gets the starting index of the range being accessed.
    /// </summary>
    public int Index { get; } = index;

    /// <summary>
    /// Gets the number of items in the range being accessed.
    /// </summary>
    public int Count { get; } = count;
}
