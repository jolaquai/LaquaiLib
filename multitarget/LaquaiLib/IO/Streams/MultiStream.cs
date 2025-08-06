using System.Diagnostics.CodeAnalysis;

using LaquaiLib.Extensions;

namespace LaquaiLib.IO.Streams;

/// <summary>
/// Represents a wrapper for multiple <see cref="Stream"/> instances to be written to as one.
/// Writes are not guaranteed to be performed on each passed <see cref="Stream"/> in the order they were passed, however, no two writes on two <see cref="Stream"/> will ever occur concurrently.
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
        for (var i = 0; i < streams.Length; i++)
        {
            if (!streams[i].CanWrite)
            {
                throw new InvalidOperationException("Cannot wrap a stream that is not writable.");
            }
        }
    }
    /// <summary>
    /// Initializes a new <see cref="MultiStream"/> with the given <see cref="Stream"/>s.
    /// </summary>
    /// <param name="streams">A collection of <see cref="Stream"/> instances that are to be written to simultaneously.</param>
    public MultiStream(IEnumerable<Stream> streams)
    {
        _streams = [.. streams];

        var strs = _streams;
        for (var i = 0; i < strs.Length; i++)
        {
            if (!strs[i].CanWrite)
            {
                throw new InvalidOperationException("Cannot wrap a stream that is not writable.");
            }
        }
    }
    #endregion

    /// <summary>
    /// Unconditionally returns <see langword="false"/>; <see cref="MultiStream"/> does not support reading.
    /// </summary>
    public override bool CanRead => false;
    /// <summary>
    /// Unconditionally returns <see langword="false"/>; <see cref="MultiStream"/> does not support seeking.
    /// </summary>
    public override bool CanSeek => false;
    /// <summary>
    /// Unconditionally returns <see langword="true"/>; <see cref="MultiStream"/> broadcasts writes directly to the wrapped instances.
    /// </summary>
    public override bool CanWrite => true;
    /// <inheritdoc/>
    public override long Length => throw new InvalidOperationException($"{nameof(MultiStream)} does not support using {nameof(Stream.Length)}.");
    /// <inheritdoc/>
    public override long Position
    {
        get => throw new InvalidOperationException($"{nameof(MultiStream)} does not support seeking.");
        set => throw new InvalidOperationException($"{nameof(MultiStream)} does not support seeking.");
    }
    /// <summary>
    /// Flushes all <see cref="Stream"/>s wrapped by this <see cref="MultiStream"/> instance.
    /// </summary>
    public override void Flush()
    {
        var streams = _streams;
        for (var i = 0; i < streams.Length; i++)
        {
            streams[i].Flush();
        }
    }
    /// <summary>
    /// Unconditionally throws an <see cref="InvalidOperationException"/>. <see cref="MultiStream"/> does not support seeking.
    /// </summary>
    [DoesNotReturn]
    public override long Seek(long offset, SeekOrigin origin) => throw new InvalidOperationException($"{nameof(MultiStream)} does not support seeking.");

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
#warning TODO: Implement the other Write overloads and add WriteAsync support

    /// <summary>
    /// Unconditionally throws an <see cref="InvalidOperationException"/>.
    /// </summary>
    [DoesNotReturn]
    public override int Read(byte[] buffer, int offset, int count) => throw new InvalidOperationException($"{nameof(MultiStream)} does not support reading.");
    /// <summary>
    /// Unconditionally throws an <see cref="InvalidOperationException"/>. <see cref="MultiStream"/> does not support operations affecting the underlying streams directly (beyond broadcasted writes).
    /// </summary>
    [DoesNotReturn]
    public override void SetLength(long value) => throw new InvalidOperationException($"{nameof(MultiStream)} does not support changing the underlying streams' lengths.");

    public new void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}