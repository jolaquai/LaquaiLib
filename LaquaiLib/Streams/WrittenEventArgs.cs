namespace LaquaiLib.Streams;

/// <summary>
/// Provides data for the <see cref="ObservableStream{T}.DataWritten"/> event.
/// </summary>
public record class WrittenEventArgs
{
    public WrittenEventArgs(ReadOnlyMemory<byte> data)
    {
        Data = data;
    }

    /// <summary>
    /// A readonly view of the data that was written.
    /// </summary>
    public ReadOnlyMemory<byte> Data { get; init; }
}
