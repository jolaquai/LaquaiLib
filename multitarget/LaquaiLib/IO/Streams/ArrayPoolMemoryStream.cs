using System.Buffers;

namespace LaquaiLib.IO.Streams;

/// <summary>
/// Implements a <see cref="Stream"/> whose backing memory is source from an <see cref="ArrayPool{T}"/>.
/// </summary>
/// <remarks>
/// This is by no means meant to replace an implementation such as <see href="https://github.com/microsoft/Microsoft.IO.RecyclableMemoryStream"/>. This type's design is much simpler.
/// </remarks>
public sealed class ArrayPoolMemoryStream : Stream, IBufferWriter<byte>
{
    private struct CachedInt32Task
    {
        private Task<int> task;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<int> Get(int value)
        {
            if (task != null && task.Result == value)
                return task;
            return task = Task.FromResult(value);
        }
    }

    private readonly ArrayPool<byte> _pool;
    private readonly List<byte[]> _segments = [];
    private readonly int _minimumSegmentSize;
    private readonly int _maxSegmentSize;
    private readonly bool _skipZeroing;

    private CachedInt32Task _lastReadTask;
    private long position, length, capacity;
    private bool _disposed;

    /// <summary>
    /// Initializes a new <see cref="ArrayPoolMemoryStream"/>.
    /// </summary>
    /// <param name="minimumSegmentSize">Smallest size any single segment will be rented at.</param>
    /// <param name="capacity">Initial capacity to rent up front.</param>
    /// <param name="skipZeroing">If <see langword="true"/>, memory exposed by seeking or <see cref="SetLength(long)"/> past the current length is not zeroed and may contain arbitrary prior contents. Only set this if all such memory is overwritten before being read.</param>
    /// <param name="pool">The <see cref="ArrayPool{T}"/> to rent segments from, or <see langword="null"/> to use <see cref="ArrayPool{T}.Shared"/>.</param>
    /// <param name="disallowLohRenting">If <see langword="true"/>, no single segment is rented larger than 65536 bytes, the largest pool bucket below the 85000-byte Large Object Heap threshold. This only bounds individual rents made to grow <see cref="Capacity"/>; a single <see cref="GetMemory(int)"/>/<see cref="GetSpan(int)"/> call whose <c>sizeHint</c> exceeds 65536 still requires a larger contiguous rent and is not subject to this cap.</param>
    public ArrayPoolMemoryStream(int minimumSegmentSize = 2048, long capacity = 0, bool skipZeroing = false, ArrayPool<byte> pool = null, bool disallowLohRenting = false)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(minimumSegmentSize);
        ArgumentOutOfRangeException.ThrowIfNegative(capacity, nameof(capacity));

        _maxSegmentSize = disallowLohRenting ? 65536 : Array.MaxLength;
        ArgumentOutOfRangeException.ThrowIfGreaterThan(minimumSegmentSize, _maxSegmentSize, nameof(minimumSegmentSize));

        _minimumSegmentSize = minimumSegmentSize;
        _skipZeroing = skipZeroing;
        _pool = pool ?? ArrayPool<byte>.Shared;

        EnsureCapacity(capacity);
    }

    /// <inheritdoc/>
    public override bool CanRead => !_disposed;
    /// <inheritdoc/>
    public override bool CanSeek => !_disposed;
    /// <inheritdoc/>
    public override bool CanWrite => !_disposed;
    /// <inheritdoc/>
    public override long Length => length;
    /// <summary>
    /// Gets the maximum <see cref="Length"/> this instance can reach without having to rent more memory.
    /// </summary>
    public long Capacity => capacity;
    /// <inheritdoc/>
    public override long Position
    {
        get => position;
        set
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            ArgumentOutOfRangeException.ThrowIfNegative(value, nameof(value));
            position = value;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)] private Span<byte[]> SegmentsSpan() => CollectionsMarshal.AsSpan(_segments);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] private (int Segment, int Offset) Locate(long absolute) => SegmentedBufferHelpers.AbsoluteToRelative<byte>(SegmentsSpan(), absolute);

    // one rent covers the entire gap unless it exceeds what a single array can hold; the minimum keeps many tiny writes from degenerating into rent-per-write
    private void EnsureCapacity(long required)
    {
        while (capacity < required)
        {
            var arr = _pool.Rent((int)long.Min(long.Max(required - capacity, _minimumSegmentSize), _maxSegmentSize));
            _segments.Add(arr);
            capacity += arr.Length;
        }
    }
    // seeking or SetLength past the end leaves a gap that would otherwise expose whatever the pool handed us
    private void ZeroRange(long start, long count)
    {
        var segments = SegmentsSpan();
        var (seg, off) = Locate(start);
        while (count > 0)
        {
            var current = segments[seg];
            var take = (int)long.Min(current.Length - off, count);
            current.AsSpan(off, take).ZeroMemory();
            count -= take;
            off += take;
            if (off == current.Length)
            {
                seg++;
                off = 0;
            }
        }
    }

    /// <inheritdoc/>
    public override int Read(byte[] buffer, int offset, int count)
    {
        ValidateBufferArguments(buffer, offset, count);
        return ReadCore(buffer.AsSpan(offset, count));
    }
    /// <inheritdoc/>
    public override int Read(Span<byte> buffer) => ReadCore(buffer);
    // public entry points validate their own arguments and hand off here, so no path validates twice
    private int ReadCore(Span<byte> buffer)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        var available = length - position;
        if (available <= 0 || buffer.IsEmpty)
            return 0;

        var count = (int)long.Min(buffer.Length, available);
        var segments = SegmentsSpan();
        var (seg, off) = Locate(position);

        var copied = 0;
        while (copied < count)
        {
            var current = segments[seg];
            var take = int.Min(current.Length - off, count - copied);
            current.AsSpan(off, take).CopyTo(buffer[copied..]);
            copied += take;
            off += take;
            if (off == current.Length)
            {
                seg++;
                off = 0;
            }
        }

        position += copied;
        return copied;
    }
    /// <inheritdoc/>
    public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        ValidateBufferArguments(buffer, offset, count);

        if (cancellationToken.IsCancellationRequested)
            return Task.FromCanceled<int>(cancellationToken);

        var read = ReadCore(buffer.AsSpan(offset, count));
        return _lastReadTask.Get(read);
    }
    /// <inheritdoc/>
    public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default) => cancellationToken.IsCancellationRequested
        ? ValueTask.FromCanceled<int>(cancellationToken)
        : new ValueTask<int>(ReadCore(buffer.Span));
    /// <inheritdoc/>
    public override int ReadByte()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (position >= length)
            return -1;

        var (seg, off) = Locate(position);
        position++;
        return _segments[seg][off];
    }

    /// <inheritdoc/>
    public override void Write(byte[] buffer, int offset, int count)
    {
        ValidateBufferArguments(buffer, offset, count);
        WriteCore(buffer.AsSpan(offset, count));
    }
    /// <inheritdoc/>
    public override void Write(ReadOnlySpan<byte> buffer) => WriteCore(buffer);
    // public entry points validate their own arguments and hand off here, so no path validates twice
    private void WriteCore(ReadOnlySpan<byte> buffer)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        // MemoryStream grows to Position even for a zero-byte write past the end, and callers written against it rely on that
        if (buffer.IsEmpty)
        {
            if (position > length)
            {
                EnsureCapacity(position);
                if (!_skipZeroing)
                    ZeroRange(length, position - length);
                length = position;
            }
            return;
        }

        var end = position + buffer.Length;
        EnsureCapacity(end);
        if (position > length && !_skipZeroing)
            ZeroRange(length, position - length);

        var segments = SegmentsSpan();
        var (seg, off) = Locate(position);

        var written = 0;
        while (written < buffer.Length)
        {
            var current = segments[seg];
            var put = int.Min(current.Length - off, buffer.Length - written);
            buffer.Slice(written, put).CopyTo(current.AsSpan(off));
            written += put;
            off += put;
            if (off == current.Length)
            {
                seg++;
                off = 0;
            }
        }

        position = end;
        if (position > length)
            length = position;
    }
    /// <inheritdoc/>
    public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        ValidateBufferArguments(buffer, offset, count);

        if (cancellationToken.IsCancellationRequested)
            return Task.FromCanceled(cancellationToken);

        WriteCore(buffer.AsSpan(offset, count));
        return Task.CompletedTask;
    }
    /// <inheritdoc/>
    public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return ValueTask.FromCanceled(cancellationToken);

        WriteCore(buffer.Span);
        return ValueTask.CompletedTask;
    }
    /// <inheritdoc/>
    public override void WriteByte(byte value) => WriteCore(new ReadOnlySpan<byte>(in value));

    // Stream.ValidateCopyToArguments minus its bufferSize check, which is meaningless here because neither copy path buffers
    private static void ValidateDestination(Stream destination)
    {
        ArgumentNullException.ThrowIfNull(destination);
        if (destination.CanWrite)
            return;
        // a destination that can do neither is closed, not merely read-only
        if (!destination.CanRead)
            throw new ObjectDisposedException(destination.GetType().Name, "Cannot access a closed stream.");
        throw new NotSupportedException("The destination stream does not support writing.");
    }
    /// <inheritdoc/>
    public override void CopyTo(Stream destination, int bufferSize)
    {
        ValidateDestination(destination);
        ObjectDisposedException.ThrowIf(_disposed, this);

        var remaining = length - position;
        if (remaining <= 0)
            return;

        var (seg, off) = Locate(position);
        while (remaining > 0)
        {
            var current = _segments[seg];
            var take = (int)long.Min(current.Length - off, remaining);
            destination.Write(current, off, take);
            remaining -= take;
            off += take;
            if (off == current.Length)
            {
                seg++;
                off = 0;
            }
        }
        position = length;
    }
    /// <inheritdoc/>
    public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
    {
        ValidateDestination(destination);
        ObjectDisposedException.ThrowIf(_disposed, this);
        return CopyToAsyncCore(destination, cancellationToken);
    }
    // split so the argument checks above throw synchronously instead of surfacing as a faulted task
    private async Task CopyToAsyncCore(Stream destination, CancellationToken cancellationToken)
    {
        var remaining = length - position;
        if (remaining <= 0)
            return;

        var (seg, off) = Locate(position);
        while (remaining > 0)
        {
            var current = _segments[seg];
            var take = (int)long.Min(current.Length - off, remaining);
            await destination.WriteAsync(current.AsMemory(off, take), cancellationToken).ConfigureAwait(false);
            remaining -= take;
            off += take;
            if (off == current.Length)
            {
                seg++;
                off = 0;
            }
        }
        position = length;
    }

    /// <inheritdoc/>
    public override long Seek(long offset, SeekOrigin origin)
    {
        Position = origin switch
        {
            SeekOrigin.Begin => offset,
            SeekOrigin.Current => position + offset,
            SeekOrigin.End => length + offset,
            _ => throw new ArgumentOutOfRangeException(nameof(origin), origin, "Invalid seek origin."),
        };
        return position;
    }
    /// <inheritdoc/>
    public override void SetLength(long value)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentOutOfRangeException.ThrowIfNegative(value);

        if (value > length)
        {
            EnsureCapacity(value);
            if (!_skipZeroing)
                ZeroRange(length, value - length);
        }

        length = value;
        if (position > length)
            position = length;
    }
    /// <summary>
    /// Returns every segment that lies entirely beyond <see cref="Length"/> to the pool.
    /// </summary>
    /// <remarks>
    /// Only whole segments can be released, so the segment <see cref="Length"/> falls inside is kept and <see cref="Capacity"/> may remain above <see cref="Length"/>. <see cref="Position"/> is left alone; writing past the end afterwards simply rents again. Any buffer previously handed out by <see cref="GetMemory(int)"/> or <see cref="GetSpan(int)"/> is invalidated.
    /// </remarks>
    public void TrimExcess()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        long kept = 0;
        var keep = 0;
        while (keep < _segments.Count && kept < length)
        {
            kept += _segments[keep].Length;
            keep++;
        }

        for (var i = keep; i < _segments.Count; i++)
            _pool.Return(_segments[i]);
        _segments.RemoveRange(keep, _segments.Count - keep);
        capacity = kept;
    }

    /// <inheritdoc/>
    public override void Flush() { }
    /// <inheritdoc/>
    public override Task FlushAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            foreach (var segment in _segments)
                _pool.Return(segment);
            _segments.Clear();

            position = length = capacity = 0;
            _disposed = true;
        }
        base.Dispose(disposing);
    }

    /// <summary>
    /// Gets a <see cref="ReadOnlySequence{T}"/> over the memory that has been written so far.
    /// This is a snapshot; mutating calls, especially ones that return memory to the pool, invalidate the sequence. Reading from it after such a mutation is undefined behavior.
    /// </summary>
    /// <returns>The created <see cref="ReadOnlySequence{T}"/>.</returns>
    public ReadOnlySequence<byte> AsReadOnlySequence()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (length == 0)
            return ReadOnlySequence<byte>.Empty;

        // walking up to length instead of locating it keeps the length == capacity case in range and never emits an empty trailing segment
        Segment first = null, prev = null;
        long running = 0;
        for (var i = 0; running < length; i++)
        {
            var take = (int)long.Min(_segments[i].Length, length - running);
            var seg = new Segment(_segments[i].AsMemory(0, take), running);
            running += take;
            if (prev is null)
                first = seg;
            else
                prev.SetNext(seg);
            prev = seg;
        }
        return new ReadOnlySequence<byte>(first, 0, prev, prev.Memory.Length);
    }
    private sealed class Segment : ReadOnlySequenceSegment<byte>
    {
        public Segment(ReadOnlyMemory<byte> memory, long index)
        {
            Memory = memory;
            RunningIndex = index;
        }
        public void SetNext(Segment next) => Next = next;
    }

    #region IBufferWriter<byte>
    /// <inheritdoc/>
    /// <remarks>
    /// The write head is <see cref="Position"/>, so this behaves exactly as if <paramref name="count"/> bytes had been written through <see cref="Write(ReadOnlySpan{byte})"/>.
    /// </remarks>
    public void Advance(int count)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentOutOfRangeException.ThrowIfNegative(count);

        if (count == 0)
            return;
        // subtraction rather than position + count, which overflows for a Position near long.MaxValue and would leave position negative
        if (count > capacity - position)
            throw new InvalidOperationException("Cannot advance past the end of the rented capacity.");

        if (position > length && !_skipZeroing)
            ZeroRange(length, position - length);

        position += count;
        if (position > length)
            length = position;
    }
    /// <inheritdoc/>
    /// <remarks>
    /// The buffer starts at <see cref="Position"/>. It may be longer than <paramref name="sizeHint"/> and, where it overlaps existing content, exposes that content rather than blank memory.
    /// </remarks>
    public Memory<byte> GetMemory(int sizeHint = 0)
    {
        var (array, offset, count) = GetWritableSegment(sizeHint);
        return array.AsMemory(offset, count);
    }
    /// <inheritdoc/>
    /// <remarks>
    /// The buffer starts at <see cref="Position"/>. It may be longer than <paramref name="sizeHint"/> and, where it overlaps existing content, exposes that content rather than blank memory.
    /// </remarks>
    public Span<byte> GetSpan(int sizeHint = 0)
    {
        var (array, offset, count) = GetWritableSegment(sizeHint);
        return array.AsSpan(offset, count);
    }
    private (byte[] Array, int Offset, int Count) GetWritableSegment(int sizeHint)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentOutOfRangeException.ThrowIfNegative(sizeHint);

        // the contract forbids handing back an empty buffer, so 0 means "whatever is left, but at least one byte"
        if (sizeHint == 0)
            sizeHint = 1;

        EnsureCapacity(position + sizeHint);
        if (position > length && !_skipZeroing)
            ZeroRange(length, position - length);

        var (seg, off) = Locate(position);
        var current = _segments[seg];
        if (current.Length - off < sizeHint)
        {
            Consolidate(seg, off + (long)sizeHint);
            current = _segments[seg]; // merging preserves offsets relative to the start of seg, so off still points at position
        }
        return (current, off, current.Length - off);
    }
    // IBufferWriter demands one contiguous run, which a segment chain only provides by accident; merging whole segments keeps every address outside the merged run where it was
    private void Consolidate(int first, long needed)
    {
        var segments = SegmentsSpan();
        long merged = 0;
        var last = first;
        // the sole caller runs EnsureCapacity(position + sizeHint) beforehand, so the tail always reaches needed
        while (merged < needed)
            merged += segments[last++].Length;

        var buffer = RentContiguous(merged);
        // the pool rounds up to bucket sizes, and surplus anywhere but the very end would shift every following segment, so the run grows until it absorbs the slack
        while (last < segments.Length && merged != buffer.Length)
        {
            merged += segments[last++].Length;
            if (merged > buffer.Length)
            {
                _pool.Return(buffer);
                buffer = RentContiguous(merged);
            }
        }

        var copied = 0;
        for (var i = first; i < last; i++)
        {
            var current = segments[i];
            current.AsSpan().CopyTo(buffer.AsSpan(copied));
            copied += current.Length;
            _pool.Return(current);
        }

        _segments[first] = buffer;
        _segments.RemoveRange(first + 1, last - first - 1);
        capacity += buffer.Length - merged;
    }
    private byte[] RentContiguous(long length) => length <= Array.MaxLength ? _pool.Rent((int)length) : throw new OutOfMemoryException("A contiguous buffer of the requested size cannot be rented.");
    #endregion
}
