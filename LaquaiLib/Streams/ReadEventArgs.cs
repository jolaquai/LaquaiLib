namespace LaquaiLib.Streams;

/// <summary>
/// Provides data for the <see cref="ObservableStream{T}.DataRead"/> event.
/// </summary>
/// <param name="Data">A readonly view of the data that was read.</param>
public record class ReadEventArgs(ReadOnlyMemory<byte> Data);