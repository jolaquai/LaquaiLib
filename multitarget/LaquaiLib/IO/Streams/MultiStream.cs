using System.Diagnostics.CodeAnalysis;

namespace LaquaiLib.IO.Streams;

/// <summary>
/// Represents a wrapper for multiple <see cref="Stream"/> instances to be written to as one.
/// Writes are not guaranteed to be performed on each passed <see cref="Stream"/> in the order they were passed, however, no two writes on two <see cref="Stream"/> will ever occur concurrently.
/// </summary>
public sealed class MultiStream : Stream
{
    private readonly Stream[] _streams;
    private readonly bool _leaveOpen;

    #region .ctors
    /// <summary>
    /// Initializes a new <see cref="MultiStream"/> with the given <see cref="Stream"/>s.
    /// </summary>
    /// <param name="streams">A collection of <see cref="Stream"/> instances that are to be written to simultaneously.</param>
    /// <param name="leaveOpen">Whether to leave the underlying <see cref="Stream"/>s open after the <see cref="MultiStream"/> is disposed.</param>
    public MultiStream(ReadOnlySpan<Stream> streams, bool leaveOpen = false)
    {
        _streams = [.. streams];
        _leaveOpen = leaveOpen;

        foreach (var v in streams)
            if (!v.CanWrite)
                throw new NotSupportedException("Cannot wrap a stream that is not writable.");
    }
    /// <summary>
    /// Initializes a new <see cref="MultiStream"/> with the given <see cref="Stream"/>s.
    /// </summary>
    /// <param name="streams">A collection of <see cref="Stream"/> instances that are to be written to simultaneously.</param>
    /// <param name="leaveOpen">Whether to leave the underlying <see cref="Stream"/>s open after the <see cref="MultiStream"/> is disposed.</param>
    public MultiStream(IEnumerable<Stream> streams, bool leaveOpen = false)
    {
        _streams = [.. streams];
        _leaveOpen = leaveOpen;

        var strs = _streams;
        foreach (var v in strs)
            if (!v.CanWrite)
                throw new NotSupportedException("Cannot wrap a stream that is not writable.");
    }
    #endregion

    /// <summary>
    /// Unconditionally returns <see langword="false"/>; <see cref="MultiStream"/> does not support reading.
    /// </summary>
    public override bool CanRead
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => false;
    }
    /// <summary>
    /// Unconditionally returns <see langword="false"/>; <see cref="MultiStream"/> does not support seeking.
    /// </summary>
    public override bool CanSeek
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => false;
    }
    /// <summary>
    /// Unconditionally returns <see langword="true"/>; <see cref="MultiStream"/> broadcasts writes directly to the wrapped instances.
    /// </summary>
    public override bool CanWrite
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => true;
    }
    /// <inheritdoc/>
    public override long Length
    {
        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            ThrowSeekNotSupported();
            return default;
        }
    }
    /// <inheritdoc/>
    public override long Position
    {
        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            ThrowSeekNotSupported();
            return default;
        }
        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => ThrowSeekNotSupported();
    }
    /// <summary>
    /// Flushes all <see cref="Stream"/>s wrapped by this <see cref="MultiStream"/> instance.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void Flush()
    {
        foreach (var v in _streams)
            v.Flush();
    }
    /// <summary>
    /// Unconditionally throws a <see cref="NotSupportedException"/>. <see cref="MultiStream"/> does not support seeking.
    /// </summary>
    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override long Seek(long offset, SeekOrigin origin)
    {
        ThrowSeekNotSupported();
        return default;
    }

    /// <summary>
    /// Writes a sequence of bytes to all <see cref="Stream"/>s wrapped by this <see cref="MultiStream"/> instance and advances the current position within the <see cref="Stream"/>s by the number of <see cref="byte"/>s written.
    /// </summary>
    /// <param name="buffer">The buffer containing the data to write.</param>
    /// <param name="offset">The offset in the buffer at which to begin writing.</param>
    /// <param name="count">The number of <see cref="byte"/>s to write.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void Write(byte[] buffer, int offset, int count) => Write(buffer.AsSpan(offset, count));
    /// <summary>
    /// Writes a sequence of bytes to all <see cref="Stream"/>s wrapped by this <see cref="MultiStream"/> instance and advances the current position within the <see cref="Stream"/>s by the number of <see cref="byte"/>s written.
    /// </summary>
    /// <param name="buffer">A region of memory to copy to all <see cref="Stream"/>s wrapped by this <see cref="MultiStream"/> instance.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void Write(ReadOnlySpan<byte> buffer)
    {
        foreach (var v in _streams)
            v.Write(buffer);
    }
    /// <summary>
    /// Writes a single <see langword="byte"/> <paramref name="value"/> to all <see cref="Stream"/>s wrapped by this <see cref="MultiStream"/> instance and advances the current position within the <see cref="Stream"/>s by one.
    /// </summary>
    /// <param name="value">The <see langword="byte"/> to write.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void WriteByte(byte value)
    {
        scoped var b = new ReadOnlySpan<byte>(ref value);
        Write(b);
    }
    /// <summary>
    /// Asynchronously writes a sequence of bytes to all <see cref="Stream"/>s wrapped by this <see cref="MultiStream"/> instance and advances the current position within the <see cref="Stream"/>s by the number of <see langword="byte"/>s written.
    /// </summary>
    /// <param name="buffer">The buffer containing the data to write.</param>
    /// <param name="offset">The offset in the buffer at which to begin writing.</param>
    /// <param name="count">The number of <see langword="byte"/>s to write.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) => WriteAsync(buffer.AsMemory(offset, count), cancellationToken).AsTask();
    /// <summary>
    /// Asynchronously writes a sequence of bytes to all <see cref="Stream"/>s wrapped by this <see cref="MultiStream"/> instance and advances the current position within the <see cref="Stream"/>s by the number of <see langword="byte"/>s written.
    /// </summary>
    /// <param name="buffer">A region of memory to copy to all <see cref="Stream"/>s wrapped by this <see cref="MultiStream"/> instance.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous write operation. Cancelling may result in writes to some <see cref="Stream"/>s but not others.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
    {
        foreach (var v in _streams)
            await v.WriteAsync(buffer, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Unconditionally throws a <see cref="NotSupportedException"/>.
    /// </summary>
    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override int Read(byte[] buffer, int offset, int count)
    {
        ThrowReadNotSupported();
        return default;
    }
    /// <summary>
    /// Unconditionally throws a <see cref="NotSupportedException"/>.
    /// </summary>
    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override int Read(Span<byte> buffer)
    {
        ThrowReadNotSupported();
        return default;
    }
    /// <summary>
    /// Unconditionally throws a <see cref="NotSupportedException"/>.
    /// </summary>
    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override int ReadByte()
    {
        ThrowReadNotSupported();
        return default;
    }
    /// <summary>
    /// Unconditionally throws a <see cref="NotSupportedException"/>.
    /// </summary>
    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        ThrowReadNotSupported();
        return default;
    }
    /// <summary>
    /// Unconditionally throws a <see cref="NotSupportedException"/>.
    /// </summary>
    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        ThrowReadNotSupported();
        return default;
    }
    /// <summary>
    /// Unconditionally throws a <see cref="NotSupportedException"/>.
    /// </summary>
    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
    {
        ThrowReadNotSupported();
        return default;
    }
    /// <summary>
    /// Unconditionally throws a <see cref="NotSupportedException"/>. <see cref="MultiStream"/> does not support operations affecting the underlying streams directly (beyond broadcasted writes).
    /// </summary>
    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void SetLength(long value) => ThrowSeekNotSupported();

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (disposing && !_leaveOpen)
            foreach (var stream in _streams)
                stream.Dispose();
        base.Dispose(disposing);
    }
    /// <inheritdoc/>
    public override async ValueTask DisposeAsync()
    {
        if (!_leaveOpen)
            foreach (var stream in _streams)
                await stream.DisposeAsync().ConfigureAwait(false);
        await base.DisposeAsync().ConfigureAwait(false);
    }

    [DoesNotReturn][MethodImpl(MethodImplOptions.NoInlining)] private static void ThrowSeekNotSupported() => throw new NotSupportedException($"{nameof(MultiStream)} does not support seeking.");
    [DoesNotReturn][MethodImpl(MethodImplOptions.NoInlining)] private static void ThrowReadNotSupported() => throw new NotSupportedException($"{nameof(MultiStream)} does not support reading.");
}