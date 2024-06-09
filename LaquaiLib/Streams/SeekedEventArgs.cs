namespace LaquaiLib.Streams;

/// <summary>
/// Provides data about the <see cref="ObservableStream{T}.Seeked"/> event.
/// </summary>
/// <param name="OldPosition">The old position of the stream before the seek operation.</param>
/// <param name="NewPosition">The new position of the stream after the seek operation.</param>
public record class SeekedEventArgs(long OldPosition, long NewPosition);