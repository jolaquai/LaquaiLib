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

    /// <inheritdoc/>
    public override bool CanRead { get; } = true;
    /// <inheritdoc/>
    public override bool CanSeek { get; } = true;
    /// <inheritdoc/>
    public override bool CanWrite { get; } = true;
    /// <inheritdoc/>
    public override long Length { get; }
    /// <inheritdoc/>
    public override long Position
    {
        get => 0;
        set { }
    }
    /// <inheritdoc/>
    public override bool CanTimeout { get; }
    /// <inheritdoc/>
    public override int ReadTimeout
    {
        get => 0;
        set { }
    }
    /// <inheritdoc/>
    public override int WriteTimeout
    {
        get => 0;
        set { }
    }

    /// <inheritdoc/>
    public override void Flush() { }
    /// <inheritdoc/>
    public override int Read(byte[] buffer, int offset, int count) => 0;
    /// <inheritdoc/>
    public override long Seek(long offset, SeekOrigin origin) => 0;
    /// <inheritdoc/>
    public override void SetLength(long value) { }
    /// <inheritdoc/>
    public override void Write(byte[] buffer, int offset, int count) { }
}

/// <summary>
/// Implements <see cref="Stream"/> without a backing store. Except for zero reads and writes, all operations throw <see cref="NotSupportedException"/>.
/// This can be useful when authoring <see cref="Stream"/> types that only support particular operations.
/// </summary>
public class ExceptStream : Stream
{
    /// <inheritdoc/>
    public override bool CanRead { get; }
    /// <inheritdoc/>
    public override bool CanSeek { get; }
    /// <inheritdoc/>
    public override bool CanWrite { get; }
    /// <inheritdoc/>
    public override long Length { get; }
    /// <inheritdoc/>
    public override long Position
    {
        get => throw new NotSupportedException();
        set => throw new NotSupportedException();
    }
    /// <inheritdoc/>
    public override bool CanTimeout { get; }
    /// <inheritdoc/>
    public override int ReadTimeout
    {
        get => throw new NotSupportedException();
        set => throw new NotSupportedException();
    }
    /// <inheritdoc/>
    public override int WriteTimeout
    {
        get => throw new NotSupportedException();
        set => throw new NotSupportedException();
    }

    /// <inheritdoc/>
    public override void Flush() => throw new NotSupportedException();
    /// <inheritdoc/>
    public override int Read(byte[] buffer, int offset, int count)
    {
        if (count == 0)
        {
            return 0;
        }
        throw new NotSupportedException();
    }
    /// <inheritdoc/>
    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
    /// <inheritdoc/>
    public override void SetLength(long value) => throw new NotSupportedException();
    /// <inheritdoc/>
    public override void Write(byte[] buffer, int offset, int count)
    {
        if (count == 0)
        {
            return;
        }
        throw new NotSupportedException();
    }
}