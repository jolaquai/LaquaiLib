namespace LaquaiLib.Streams;

/// <summary>
/// Provides data for the <see cref="ObservableStream{T}.DataRead"/> event.
/// </summary>
public record class ReadEventArgs
{
    public ReadEventArgs(ReadOnlyMemory<byte> data)
    {
        Data = data;
    }

    /// <summary>
    /// A readonly view of the data that was read.
    /// </summary>
    public ReadOnlyMemory<byte> Data { get; init; }
}
