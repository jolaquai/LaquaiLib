namespace LaquaiLib.Streams;

/// <summary>
/// Implements <see cref="Stream"/> without a backing store. All operations are allowed and are no-ops.
/// This can be useful when authoring <see cref="Stream"/> types that only support particular operations.
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
    public override long Length { get; }
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

/// <summary>
/// Implements <see cref="Stream"/> without a backing store. Except for zero reads and writes, all operations throw <see cref="NotSupportedException"/>.
/// This can be useful when authoring <see cref="Stream"/> types that only support particular operations.
/// </summary>
public class ExceptStream : Stream
{
    public override bool CanRead { get; }
    public override bool CanSeek { get; }
    public override bool CanWrite { get; }
    public override long Length { get; }
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

    public override void Flush() => throw new NotSupportedException();
    public override int Read(byte[] buffer, int offset, int count)
    {
        if (count == 0)
        {
            return 0;
        }
        throw new NotSupportedException();
    }
    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
    public override void SetLength(long value) => throw new NotSupportedException();
    public override void Write(byte[] buffer, int offset, int count)
    {
        if (count == 0)
        {
            return;
        }
        throw new NotSupportedException();
    }
}