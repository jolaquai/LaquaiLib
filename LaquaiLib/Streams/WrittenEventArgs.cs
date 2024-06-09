namespace LaquaiLib.Streams;

/// <summary>
/// Provides data for the <see cref="ObservableStream{T}.DataWritten"/> event.
/// </summary>
/// <param name="Data">A readonly view of the data that was written.</param>
public record class WrittenEventArgs(ReadOnlyMemory<byte> Data);