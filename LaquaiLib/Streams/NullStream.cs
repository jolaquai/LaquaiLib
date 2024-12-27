namespace LaquaiLib.Streams;

/// <summary>
/// Represents a <see cref="Stream"/> without a backing store.
/// All operations are allowed and are no-ops.
/// </summary>
public class NullStream : Stream
{
    /// <summary>
    /// Returns a singleton instance of <see cref="NullStream"/>.
    /// </summary>
    public static Stream Instance { get; } = new NullStream();

    public override bool CanRead { get; } = true;
    public override bool CanSeek { get; } = true;
    public override bool CanWrite { get; } = true;
    public override long Length { get; } = 0;
    public override long Position
    {
        get => 0;
        set { }
    }
    public override bool CanTimeout { get; }
    public override int ReadTimeout
    {
        get => 0;
        set { }
    }

    public override void Flush() { }
    public override int Read(byte[] buffer, int offset, int count) => 0;
    public override long Seek(long offset, SeekOrigin origin) => 0;
    public override void SetLength(long value) { }
    public override void Write(byte[] buffer, int offset, int count) { }
}
