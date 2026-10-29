namespace LaquaiLib.IO.Streams;

/// <summary>
/// Implements a <see cref="Stream"/> that dispatches all calls to an inner <see cref="Stream"/>.
/// </summary>
/// <param name="inner">The inner <see cref="Stream"/> to dispatch to.</param>
public class DispatchStream(Stream inner) : Stream
{
    /// <inheritdoc/>
    public override bool CanRead => inner.CanRead;
    /// <inheritdoc/>
    public override bool CanSeek => inner.CanSeek;
    /// <inheritdoc/>
    public override bool CanWrite => inner.CanWrite;
    /// <inheritdoc/>
    public override long Length => inner.Length;
    /// <inheritdoc/>
    public override long Position { get => inner.Position; set => inner.Position = value; }
    /// <inheritdoc/>
    public override bool CanTimeout => inner.CanTimeout;
    /// <inheritdoc/>
    public override int ReadTimeout { get => inner.ReadTimeout; set => inner.ReadTimeout = value; }
    /// <inheritdoc/>
    public override int WriteTimeout { get => inner.WriteTimeout; set => inner.WriteTimeout = value; }

    /// <inheritdoc/>
    public override void Flush() => inner.Flush();
    /// <inheritdoc/>
    public override int Read(byte[] buffer, int offset, int count) => inner.Read(buffer, offset, count);
    /// <inheritdoc/>
    public override long Seek(long offset, SeekOrigin origin) => inner.Seek(offset, origin);
    /// <inheritdoc/>
    public override void SetLength(long value) => inner.SetLength(value);
    /// <inheritdoc/>
    public override void Write(byte[] buffer, int offset, int count) => inner.Write(buffer, offset, count);
    /// <inheritdoc/>
    public override bool Equals(object obj) => inner.Equals(obj);
    /// <inheritdoc/>
    public override int GetHashCode() => inner.GetHashCode();
    /// <inheritdoc/>
    public override string ToString() => inner.ToString();
    /// <inheritdoc/>
    public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state) => inner.BeginRead(buffer, offset, count, callback, state);
    /// <inheritdoc/>
    public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state) => inner.BeginWrite(buffer, offset, count, callback, state);
    /// <inheritdoc/>
    public override void Close() => inner.Close();
    /// <inheritdoc/>
    public override void CopyTo(Stream destination, int bufferSize) => inner.CopyTo(destination, bufferSize);
    /// <inheritdoc/>
    public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken) => inner.CopyToAsync(destination, bufferSize, cancellationToken);
    /// <inheritdoc/>
    protected override void Dispose(bool disposing) => base.Dispose(disposing);
    /// <inheritdoc/>
    public override async ValueTask DisposeAsync()
    {
        await inner.DisposeAsync().ConfigureAwait(false);
        GC.SuppressFinalize(this);
    }
    /// <inheritdoc/>
    public override int EndRead(IAsyncResult asyncResult) => inner.EndRead(asyncResult);
    /// <inheritdoc/>
    public override void EndWrite(IAsyncResult asyncResult) => inner.EndWrite(asyncResult);
    /// <inheritdoc/>
    public override Task FlushAsync(CancellationToken cancellationToken) => inner.FlushAsync(cancellationToken);
    /// <inheritdoc/>
    public override int Read(Span<byte> buffer) => inner.Read(buffer);
    /// <inheritdoc/>
    public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) => inner.ReadAsync(buffer, offset, count, cancellationToken);
    /// <inheritdoc/>
    public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default) => inner.ReadAsync(buffer, cancellationToken);
    /// <inheritdoc/>
    public override int ReadByte() => inner.ReadByte();
    /// <inheritdoc/>
    public override void Write(ReadOnlySpan<byte> buffer) => inner.Write(buffer);
    /// <inheritdoc/>
    public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) => inner.WriteAsync(buffer, offset, count, cancellationToken);
    /// <inheritdoc/>
    public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default) => inner.WriteAsync(buffer, cancellationToken);
    /// <inheritdoc/>
    public override void WriteByte(byte value) => inner.WriteByte(value);
}
