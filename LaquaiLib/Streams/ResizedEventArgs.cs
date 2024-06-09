namespace LaquaiLib.Streams;

/// <summary>
/// Provides data for the <see cref="ObservableStream{T}.Resized"/> event.
/// </summary>
/// <param name="OldLength">The old length of the stream before the resize operation.</param>
/// <param name="NewLength">The new length of the stream after the resize operation.</param>
public record class ResizedEventArgs(long OldLength, long NewLength);