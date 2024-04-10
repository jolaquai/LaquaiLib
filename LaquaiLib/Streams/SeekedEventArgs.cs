namespace LaquaiLib.Streams;

/// <summary>
/// Provides data about the <see cref="ObservableStream{T}.Seeked"/> event.
/// </summary>
public record class SeekedEventArgs
{
    public SeekedEventArgs(long oldPosition, long newPosition)
    {
        OldPosition = oldPosition;
        NewPosition = newPosition;
    }

    /// <summary>
    /// The old position of the stream before the seek operation.
    /// </summary>
    public long OldPosition { get; init; }
    /// <summary>
    /// The new position of the stream after the seek operation.
    /// </summary>
    public long NewPosition { get; init; }
}
