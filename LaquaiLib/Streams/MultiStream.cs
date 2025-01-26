using System.Diagnostics.CodeAnalysis;

using LaquaiLib.Extensions;

namespace LaquaiLib.Streams;

/// <summary>
/// Represents a wrapper for multiple <see cref="Stream"/> instances to be written to as one.
/// The order in which writes are performed is undefined.
/// </summary>
public class MultiStream : Stream, IDisposable
{
    private readonly Stream[] _streams;

    #region .ctors
    /// <summary>
    /// Initializes a new <see cref="MultiStream"/> with the given <see cref="Stream"/>s.
    /// </summary>
    /// <param name="streams">A collection of <see cref="Stream"/> instances that are to be written to simultaneously.</param>
    public MultiStream(params ReadOnlySpan<Stream> streams)
    {
        _streams = [.. streams];
    }
    /// <summary>
    /// Initializes a new <see cref="MultiStream"/> with the given <see cref="Stream"/>s.
    /// </summary>
    /// <param name="streams">A collection of <see cref="Stream"/> instances that are to be written to simultaneously.</param>
    public MultiStream(IEnumerable<Stream> streams)
    {
        _streams = [.. streams];
    }
    #endregion

    /// <summary>
    /// A value that indicates whether all <see cref="Stream"/>s wrapped by this <see cref="MultiStream"/> instance can be read from.
    /// </summary>
    public override bool CanRead => _streams.Select(static stream => stream.CanRead).All();
    /// <summary>
    /// A value that indicates whether all <see cref="Stream"/>s wrapped by this <see cref="MultiStream"/> instance can be seeked.
    /// </summary>
    public override bool CanSeek => _streams.Select(static stream => stream.CanSeek).All();
    /// <summary>
    /// A value that indicates whether all <see cref="Stream"/>s wrapped by this <see cref="MultiStream"/> instance can be written to.
    /// </summary>
    public override bool CanWrite => _streams.Select(static stream => stream.CanWrite).All();
    /// <summary>
    /// A collection of <see cref="long"/>s that indicate the lengths of the <see cref="Stream"/>s wrapped by this <see cref="MultiStream"/> instance.
    /// </summary>
    public long[] Lengths => [.. _streams.Select(static stream => stream.Length)];
    /// <inheritdoc/>
    public override long Length => throw new InvalidOperationException($"{nameof(LaquaiLib.Streams.MultiStream)} does not support using {nameof(Stream.Length)}. Use {nameof(Lengths)} instead.");
    /// <summary>
    /// A collection of <see cref="long"/>s taht indicate the current positions of the <see cref="Stream"/>s wrapped by this <see cref="MultiStream"/> instance.
    /// </summary>
    public long[] Positions => [.. _streams.Select(static stream => stream.Position)];
    /// <inheritdoc/>
    public override long Position
    {
        get => throw new InvalidOperationException($"{nameof(LaquaiLib.Streams.MultiStream)} does not support using {nameof(Stream.Position)}. Use {nameof(Positions)} instead.");

        set => throw new InvalidOperationException($"{nameof(LaquaiLib.Streams.MultiStream)} does not support using {nameof(Stream.Position)}. Use {nameof(Positions)} instead.");
    }
    /// <summary>
    /// Flushes all <see cref="Stream"/>s wrapped by this <see cref="MultiStream"/> instance.
    /// </summary>
    public override void Flush() => _streams.ForEach(static stream => stream.Flush());
    /// <summary>
    /// Seeks all <see cref="Stream"/>s wrapped by this <see cref="MultiStream"/> instance.
    /// </summary>
    /// <param name="offset">The offset to seek by.</param>
    /// <param name="origin">A <see cref="SeekOrigin"/> value that indicates the reference point used to obtain the new position.</param>
    /// <returns>-1. Use <see cref="Positions"/> to obtain the new positions of the <see cref="Stream"/>s wrapped by this <see cref="MultiStream"/> instance.</returns>
    public override long Seek(long offset, SeekOrigin origin)
    {
        _streams.ForEach(stream => stream.Seek(offset, origin));
        return -1;
    }
    /// <summary>
    /// Sets a new length for all <see cref="Stream"/>s wrapped by this <see cref="MultiStream"/> instance.
    /// </summary>
    /// <param name="value">The new length for the <see cref="Stream"/>s wrapped by this <see cref="MultiStream"/> instance.</param>
    public void SetLengths(long value) => _streams.ForEach(stream => stream.SetLength(value));
    /// <summary>
    /// Writes a sequence of bytes to all <see cref="Stream"/>s wrapped by this <see cref="MultiStream"/> instance and advances the current position within the <see cref="Stream"/>s by the number of <see cref="byte"/>s written.
    /// </summary>
    /// <param name="buffer">The buffer containing the data to write.</param>
    /// <param name="offset">The offset in the buffer at which to begin writing.</param>
    /// <param name="count">The number of <see cref="byte"/>s to write.</param>
    public override void Write(byte[] buffer, int offset, int count) => Write(buffer.AsSpan(offset, count));
    /// <summary>
    /// Writes a sequence of bytes to all <see cref="Stream"/>s wrapped by this <see cref="MultiStream"/> instance and advances the current position within the <see cref="Stream"/>s by the number of <see cref="byte"/>s written.
    /// </summary>
    /// <param name="buffer">A region of memory to copy to all <see cref="Stream"/>s wrapped by this <see cref="MultiStream"/> instance.</param>
    public override void Write(ReadOnlySpan<byte> buffer)
    {
        for (var i = _streams.Length - 1; i >= 0; i--)
        {
            _streams[i].Write(buffer);
        }
    }

    /// <summary>
    /// Unconditionally throws an <see cref="InvalidOperationException"/>.
    /// </summary>
    [DoesNotReturn]
    public override int Read(byte[] buffer, int offset, int count) => throw new InvalidOperationException($"{nameof(LaquaiLib.Streams.MultiStream)} does not support using {nameof(Stream.Read)}.");
    /// <inheritdoc cref="SetLengths(long)"/>
    public override void SetLength(long value) => SetLengths(value);

    #region Dispose pattern
    private bool _disposed;

    /// <summary>
    /// Releases the unmanaged and optionally the managed resources used by this <see cref="MultiStream"/> instance.
    /// </summary>
    /// <param name="disposing">Whether to release the managed resources used by this <see cref="MultiStream"/> instance.</param>
    protected override void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // Dispose of managed resources (Streams etc.)
                _streams.Dispose();
            }

            // Dispose of unmanaged resources (native allocated memory etc.)

            _disposed = true;
        }
    }

    /// <summary>
    /// Finalizes this <see cref="MultiStream"/> instance, releasing any unmanaged resources.
    /// </summary>
    ~MultiStream()
    {
        Dispose(false);
    }

    /// <summary>
    /// Releases the managed and unmanaged resources used by this <see cref="MultiStream"/> instance.
    /// </summary>
    public new void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    #endregion
}
