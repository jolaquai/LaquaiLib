using System.Buffers;

namespace LaquaiLib.IO.Streams;

/// <summary>
/// Implements a <see cref="Stream"/> whose backing memory is source from an <see cref="ArrayPool{T}"/>.
/// </summary>
public sealed class ArrayPoolMemoryStream : Stream
{
    private static readonly ArrayPool<byte> _pool = ArrayPool<byte>.Shared;

    private readonly List<byte[]> _segments = [];
    private readonly int _smallSegmentSize, _largeSegmentSize;

    private int segment = -1, index = -1;
    private Memory<byte> currentSegment;
    private Task<int> _lastReadTask;
    private long length;

    public ArrayPoolMemoryStream(int smallSegmentSize = 2048, int largeSegmentSize = 16384, long capacity = 0)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(smallSegmentSize);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(largeSegmentSize);
        ArgumentOutOfRangeException.ThrowIfNegative(capacity, nameof(capacity));

        _smallSegmentSize = smallSegmentSize;
        _largeSegmentSize = largeSegmentSize;

        byte[] last = null;
        while (capacity > 0)
        {
            if (capacity > largeSegmentSize)
            {
                var arr = last = _pool.Rent(largeSegmentSize);
                _segments.Add(arr);
                capacity -= largeSegmentSize;
            }
            else
            {
                var arr = last = _pool.Rent(smallSegmentSize);
                _segments.Add(arr);
                break;
            }
        }

        segment = _segments.Count - 1;
        if (last is not null)
            currentSegment = last;
    }

    /// <inheritdoc/>
    public override bool CanRead => true;
    /// <inheritdoc/>
    public override bool CanSeek => true;
    /// <inheritdoc/>
    public override bool CanWrite => true;
    /// <inheritdoc/>
    public override long Length => length;
    /// <summary>
    /// Gets the maximum <see cref="Length"/> this instance can reach without having to rent more memory.
    /// </summary>
    public long Capacity { get; private set; }
    /// <inheritdoc/>
    public override long Position
    {
        get => SegmentedBufferHelpers.RelativeToAbsolute(SegmentsSpan(), segment, index);
        set
        {
            ArgumentOutOfRangeException.ThrowIfNegative(value, nameof(value));
            var (s, o) = SegmentedBufferHelpers.AbsoluteToRelative(SegmentsSpan(), value);
            segment = s;
            index = o;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)] private Span<byte[]> SegmentsSpan() => CollectionsMarshal.AsSpan(_segments);

    /// <inheritdoc/>
    public override int Read(byte[] buffer, int offset, int count) => Read(buffer.AsSpan(offset, count));
    /// <inheritdoc/>
    public override int Read(Span<byte> buffer)
    {
        var idx = index;
        var startSeg = segment;
        if (buffer.Length == 0 || startSeg == -1 || idx == -1 || currentSegment.IsEmpty)
            return 0;

        var count = buffer.Length;
        var segments = SegmentsSpan();
        var copied = 0;
        for (var i = startSeg; i < segments.Length; i++)
        {
            ref readonly var segment = ref segments[i];

            var wantToCopy = segment.Length - idx;
            var remaining = count - copied;
            var toCopy = remaining < wantToCopy ? remaining : wantToCopy;
            segment.AsSpan(idx, toCopy).CopyTo(buffer[copied..]);
            copied += toCopy;
        }
        return copied;
    }
    /// <inheritdoc/>
    public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        var read = Read(buffer.AsSpan(offset, count));
        ref var lrt = ref _lastReadTask;
        if (read == lrt?.Result)
            return lrt;
        return lrt = Task.FromResult(read);
    }
    /// <inheritdoc/>
    public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        var read = Read(buffer.Span);
        ref var lrt = ref _lastReadTask;
        if (read == lrt?.Result)
            return new ValueTask<int>(lrt);
        return new ValueTask<int>(lrt = Task.FromResult(read));
    }
    /// <inheritdoc/>
    public override int ReadByte()
    {
        var idx = index;
        var curSeg = currentSegment;
        if (segment == -1 || idx == -1 || curSeg.IsEmpty)
            return -1;
        var value = curSeg.Span[idx++];
        return value;
    }

    /// <inheritdoc/>
    public override void Write(byte[] buffer, int offset, int count) => Write(buffer.AsSpan(offset, count));
    /// <inheritdoc/>
    public override void Write(ReadOnlySpan<byte> buffer)
    {
        if (buffer.Length == 0)
            return;

        var writeFrom = 0;
        while (writeFrom < buffer.Length)
        {
            var remaining = buffer.Length - writeFrom;
            var dest = Next(remaining);
            var canWrite = dest.Length - index;
            var toWrite = remaining < canWrite ? remaining : canWrite;
            buffer.Slice(writeFrom, toWrite).CopyTo(dest.Span[index..]);
            writeFrom += toWrite;
            index = toWrite;
            length += toWrite;
        }
    }
    /// <inheritdoc/>
    public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        Write(buffer.AsSpan(offset, count));
        return Task.CompletedTask;
    }
    /// <inheritdoc/>
    public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
    {
        Write(buffer.Span);
        return ValueTask.CompletedTask;
    }
    /// <inheritdoc/>
    public override void WriteByte(byte value) => Next(1).Span[index] = value;

    /// <inheritdoc/>
    public override void CopyTo(Stream destination, int bufferSize)
    {
        ArgumentNullException.ThrowIfNull(destination);

        var segments = SegmentsSpan();
        for (var i = 0; i < segments.Length; i++)
            destination.Write(segments[i]);
    }
    /// <inheritdoc/>
    public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(destination);

        CopyTo(destination, bufferSize);
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public override long Seek(long offset, SeekOrigin origin) => origin switch
    {
        SeekOrigin.Begin => Position = offset,
        SeekOrigin.Current => Position += offset,
        SeekOrigin.End => Position = Length + offset,
        _ => throw new ArgumentOutOfRangeException(nameof(origin), origin, "Invalid seek origin."),
    };
    /// <inheritdoc/>
    public override void SetLength(long value)
    {
        if (value > length)
            throw new NotSupportedException("Cannot set length beyond the current length of the stream.");
        length = value;
    }

    /// <inheritdoc/>
    public override void Flush() { }
    /// <inheritdoc/>
    public override Task FlushAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private Memory<byte> Next(int sizeHint = 0)
    {
        // currentSegment isn't full, so advancing would violate contiguity of the data we receive
        if (!currentSegment.IsEmpty && index != currentSegment.Length)
            return currentSegment;

        index = 0;
        segment++;

        if (segment < _segments.Count)
        {
            // we already had a segment and got SetLength'd toward the front, so just reuse it
            return currentSegment = _segments[segment];
        }

        byte[] arr;
        if (sizeHint > _largeSegmentSize)
            arr = _pool.Rent(_largeSegmentSize);
        else
            arr = _pool.Rent(_smallSegmentSize);
        _segments.Add(arr);
        return currentSegment = arr;
    }
}
