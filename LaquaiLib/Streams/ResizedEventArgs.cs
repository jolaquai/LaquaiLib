namespace LaquaiLib.Streams;

/// <summary>
/// Provides data for the <see cref="ObservableStream{T}.Resized"/> event.
/// </summary>
public record class ResizedEventArgs
{
    public ResizedEventArgs(long oldLength, long newLength)
    {
        OldLength = oldLength;
        NewLength = newLength;
    }

    /// <summary>
    /// The old length of the stream before the resize operation.
    /// </summary>
    public long OldLength { get; init; }
    /// <summary>
    /// The new length of the stream after the resize operation.
    /// </summary>
    public long NewLength { get; init; }
}
