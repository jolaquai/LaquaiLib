using System;
using System.IO;

namespace LaquaiLib.Classes.Streams;

/// <summary>
/// Represents a <see cref="Stream"/> that can be either a <see cref="MemoryStream"/> or a <see cref="FileStream"/>, depending on the size of the data expected to be written to it.
/// </summary>
public class MemoryOrFileStream : Stream
{
    /// <summary>
    /// The number of bytes at which the stream will switch from a <see cref="MemoryStream"/> to a <see cref="FileStream"/>.
    /// </summary>
    public static int Cutoff = 512 * 1024 * 1024; // 512 MB

    private Stream _stream;

    /// <summary>
    /// Instantiates a new <see cref="MemoryOrFileStream"/> with the given expected payload size.
    /// </summary>
    /// <param name="payloadSize">The expected size of the payload to be written to this stream. If it exceeds a set <see cref="Cutoff"/>, the internal <see cref="Stream"/> is created as a <see cref="FileStream"/>.</param>
    public MemoryOrFileStream(int payloadSize)
    {
        _stream = payloadSize >= Cutoff
            ? new FileStream(Path.GetTempFileName(), FileMode.Create, FileAccess.ReadWrite, FileShare.Read, 4096, FileOptions.DeleteOnClose | FileOptions.Asynchronous)
            : new MemoryStream(payloadSize);
    }

    /// <inheritdoc/>
    public override bool CanRead => _stream.CanRead;
    /// <inheritdoc/>
    public override bool CanSeek => _stream.CanSeek;
    /// <inheritdoc/>
    public override bool CanWrite => _stream.CanWrite;
    /// <inheritdoc/>
    public override long Length => _stream.Length;
    /// <inheritdoc/>
    public override long Position {
        get => _stream.Position;
        set => _stream.Position = value;
    }
    /// <inheritdoc/>
    public override void Flush() => _stream.Flush();
    /// <inheritdoc/>
    public override int Read(byte[] buffer, int offset, int count) => _stream.Read(buffer, offset, count);
    /// <inheritdoc/>
    public override long Seek(long offset, SeekOrigin origin) => _stream.Seek(offset, origin);
    /// <inheritdoc/>
    public override void SetLength(long value) => _stream.SetLength(value);
    /// <inheritdoc/>
    public override void Write(byte[] buffer, int offset, int count) => _stream.Write(buffer, offset, count);

    #region Dispose pattern
    private bool _disposed;

    /// <summary>
    /// Releases the unmanaged and optionally the managed resources used by this <see cref="MemoryOrFileStream"/> instance.
    /// </summary>
    /// <param name="disposing">Whether to release the managed resources used by this <see cref="MemoryOrFileStream"/> instance.</param>
    protected override void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // Dispose of managed resources (Streams etc.)
                _stream.Dispose();
            }

            // Dispose of unmanaged resources (native allocated memory etc.)

            _disposed = true;
        }
    }

    /// <summary>
    /// Finalizes this <see cref="MemoryOrFileStream"/> instance, releasing any unmanaged resources.
    /// </summary>
    ~MemoryOrFileStream()
    {
        Dispose(false);
    }

    /// <summary>
    /// Releases the managed and unmanaged resources used by this <see cref="MemoryOrFileStream"/> instance.
    /// </summary>
    public new void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    #endregion
}
