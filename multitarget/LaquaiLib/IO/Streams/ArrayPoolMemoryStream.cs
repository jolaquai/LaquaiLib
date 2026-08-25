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
    private readonly List<BufferSegment<byte>> _segments = [];
    private readonly int _minimumSegmentSize;
    private readonly int _maxSegmentSize;
    private readonly bool _skipZeroing;

    private CachedInt32Task _lastReadTask;
    private long position, length, capacity;
    private int _disposed;

    /// <summary>
    /// Initializes a new <see cref="ArrayPoolMemoryStream"/>.
    /// </summary>
    /// <param name="minimumSegmentSize">Smallest size any single segment will be rented at.</param>
    /// <param name="capacity">Initial capacity to rent up front.</param>
    /// <param name="skipZeroing">If <see langword="true"/>, memory exposed by seeking or <see cref="SetLength(long)"/> past the current length is not zeroed and may contain arbitrary prior contents. Only set this if all such memory is overwritten before being read.</param>
    /// <param name="pool">The <see cref="ArrayPool{T}"/> to rent segments from, or <see langword="null"/> to use <see cref="ArrayPool{T}.Shared"/>.</param>
    /// <param name="disallowLohRenting">If <see langword="true"/>, no single segment is rented larger than 65536 bytes, the largest pool bucket below the 85000-byte Large Object Heap threshold. Every <see cref="Stream"/> member honours this, as does appending through <see cref="GetMemory(int)"/>/<see cref="GetSpan(int)"/>. The one exception is asking those two for a run that starts before <see cref="Length"/> and crosses a segment boundary: the run has to be contiguous and has to start at <see cref="Position"/>, and ending a segment early to arrange that would renumber the data stored past it, so the segments are merged into a single rent that may exceed the cap. A <c>sizeHint</c> smaller than the cap is enough to trigger it; only its reaching past the end of the segment <see cref="Position"/> sits in matters.</param>
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ThrowIfDisposed() => ObjectDisposedException.ThrowIf(Volatile.Read(ref _disposed) != 0, this);

    /// <inheritdoc/>
    public override bool CanRead => Volatile.Read(ref _disposed) == 0;
    /// <inheritdoc/>
    public override bool CanSeek => Volatile.Read(ref _disposed) == 0;
    /// <inheritdoc/>
    public override bool CanWrite => Volatile.Read(ref _disposed) == 0;
    /// <inheritdoc/>
    public override long Length
    {
        get
        {
            ThrowIfDisposed();
            return length;
        }
    }
    /// <summary>
    /// Gets the maximum <see cref="Length"/> this instance can reach without having to rent more memory.
    /// </summary>
    public long Capacity
    {
        get
        {
            ThrowIfDisposed();
            return capacity;
        }
    }
    /// <inheritdoc/>
    public override long Position
    {
        get
        {
            ThrowIfDisposed();
            return position;
        }
        set
        {
            ArgumentOutOfRangeException.ThrowIfNegative(value, nameof(value));
            ThrowIfDisposed();
            position = value;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)] private Span<BufferSegment<byte>> SegmentsSpan() => CollectionsMarshal.AsSpan(_segments);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] private (int Segment, int Offset) Locate(long absolute) => SegmentedBufferHelpers.AbsoluteToRelative<byte>(SegmentsSpan(), absolute);

    // one rent covers the entire gap unless it exceeds what a single array can hold; the minimum keeps many tiny writes from degenerating into rent-per-write
    private void EnsureCapacity(long required)
    {
        while (capacity < required)
        {
            var arr = _pool.Rent((int)long.Min(long.Max(required - capacity, _minimumSegmentSize), _maxSegmentSize));
            _segments.Add(BufferSegment<byte>.Full(arr));
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
            current.Array.AsSpan(off, take).ZeroMemory();
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
        ThrowIfDisposed();
        return ReadCore(buffer.AsSpan(offset, count));
    }
    /// <inheritdoc/>
    public override int Read(Span<byte> buffer)
    {
        ThrowIfDisposed();
        return ReadCore(buffer);
    }
    // public entry points check disposal and validate their own arguments and hand off here, so no path does either twice
    private int ReadCore(Span<byte> buffer)
    {
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
            current.Array.AsSpan(off, take).CopyTo(buffer[copied..]);
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
        ThrowIfDisposed();

        if (cancellationToken.IsCancellationRequested)
            return Task.FromCanceled<int>(cancellationToken);

        var read = ReadCore(buffer.AsSpan(offset, count));
        return _lastReadTask.Get(read);
    }
    /// <inheritdoc/>
    public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        return cancellationToken.IsCancellationRequested
            ? ValueTask.FromCanceled<int>(cancellationToken)
            : new ValueTask<int>(ReadCore(buffer.Span));
    }
    /// <inheritdoc/>
    public override int ReadByte()
    {
        ThrowIfDisposed();

        if (position >= length)
            return -1;

        var (seg, off) = Locate(position);
        position++;
        return _segments[seg].Array[off];
    }

    /// <inheritdoc/>
    public override void Write(byte[] buffer, int offset, int count)
    {
        ValidateBufferArguments(buffer, offset, count);
        ThrowIfDisposed();
        WriteCore(buffer.AsSpan(offset, count));
    }
    /// <inheritdoc/>
    public override void Write(ReadOnlySpan<byte> buffer)
    {
        ThrowIfDisposed();
        WriteCore(buffer);
    }
    // public entry points check disposal and validate their own arguments and hand off here, so no path does either twice
    private void WriteCore(ReadOnlySpan<byte> buffer)
    {
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
            buffer.Slice(written, put).CopyTo(current.Array.AsSpan(off));
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
        ThrowIfDisposed();

        if (cancellationToken.IsCancellationRequested)
            return Task.FromCanceled(cancellationToken);

        WriteCore(buffer.AsSpan(offset, count));
        return Task.CompletedTask;
    }
    /// <inheritdoc/>
    public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        if (cancellationToken.IsCancellationRequested)
            return ValueTask.FromCanceled(cancellationToken);

        WriteCore(buffer.Span);
        return ValueTask.CompletedTask;
    }
    /// <inheritdoc/>
    public override void WriteByte(byte value)
    {
        ThrowIfDisposed();
        WriteCore(new ReadOnlySpan<byte>(in value));
    }

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
        ThrowIfDisposed();

        var remaining = length - position;
        if (remaining <= 0)
            return;

        var (seg, off) = Locate(position);
        while (remaining > 0)
        {
            var current = _segments[seg];
            var take = (int)long.Min(current.Length - off, remaining);
            destination.Write(current.Array, off, take);
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
        ThrowIfDisposed();
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
            // unlike every other path, this one yields mid-walk, so a dispose can land between iterations and empty the segment list out from under it
            ThrowIfDisposed();
            var current = _segments[seg];
            var take = (int)long.Min(current.Length - off, remaining);
            await destination.WriteAsync(current.Array.AsMemory(off, take), cancellationToken).ConfigureAwait(false);
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
        ThrowIfDisposed();
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
        ArgumentOutOfRangeException.ThrowIfNegative(value);
        ThrowIfDisposed();

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
        ThrowIfDisposed();

        long kept = 0;
        var keep = 0;
        while (keep < _segments.Count && kept < length)
        {
            kept += _segments[keep].Length;
            keep++;
        }

        for (var i = keep; i < _segments.Count; i++)
            _pool.Return(_segments[i].Array);
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
        // claiming the flag up front means a second call, from either this thread or another, cannot return the same segments twice
        if (Interlocked.Exchange(ref _disposed, 1) == 0)
        {
            foreach (var segment in _segments)
                _pool.Return(segment.Array);
            _segments.Clear();

            position = length = capacity = 0;
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
        ThrowIfDisposed();
        if (length == 0)
            return ReadOnlySequence<byte>.Empty;

        // walking up to length instead of locating it keeps the length == capacity case in range and never emits an empty trailing segment
        SequenceSegment first = null, prev = null;
        long running = 0;
        for (var i = 0; running < length; i++)
        {
            var take = (int)long.Min(_segments[i].Length, length - running);
            var seg = new SequenceSegment(_segments[i].Array.AsMemory(0, take), running);
            running += take;
            if (prev is null)
                first = seg;
            else
                prev.SetNext(seg);
            prev = seg;
        }
        return new ReadOnlySequence<byte>(first, 0, prev, prev.Memory.Length);
    }
    /// <summary>
    /// Copies the memory that has been written so far into a new array sized exactly to <see cref="Length"/>.
    /// </summary>
    /// <remarks>
    /// Unlike <see cref="AsReadOnlySequence"/>, the returned array is owned by the caller and is unaffected by later mutations of this instance. <see cref="Position"/> is not advanced.
    /// </remarks>
    /// <returns>The created array.</returns>
    /// <exception cref="OutOfMemoryException">Thrown if <see cref="Length"/> exceeds what a single array can hold.</exception>
    public byte[] ToArray()
    {
        ThrowIfDisposed();
        if (length == 0)
            return [];
        if (length > Array.MaxLength)
            throw new OutOfMemoryException("A contiguous array of the requested size cannot be allocated.");

        // every byte is overwritten by the copy below, so the runtime's zeroing pass would be wasted work
        var result = GC.AllocateUninitializedArray<byte>((int)length);
        var destination = result.AsSpan();
        var segments = SegmentsSpan();
        // the final segment is only partially filled whenever length falls inside it, so the destination drives the loop rather than the segment lengths
        for (var i = 0; !destination.IsEmpty; i++)
        {
            var current = segments[i];
            var take = int.Min(current.Length, destination.Length);
            current.Array.AsSpan(0, take).CopyTo(destination);
            destination = destination[take..];
        }
        return result;
    }
    private sealed class SequenceSegment : ReadOnlySequenceSegment<byte>
    {
        public SequenceSegment(ReadOnlyMemory<byte> memory, long index)
        {
            Memory = memory;
            RunningIndex = index;
        }
        public void SetNext(SequenceSegment next) => Next = next;
    }

    #region IBufferWriter<byte>
    /// <inheritdoc/>
    /// <remarks>
    /// The write head is <see cref="Position"/>, so this behaves exactly as if <paramref name="count"/> bytes had been written through <see cref="Write(ReadOnlySpan{byte})"/>.
    /// </remarks>
    public void Advance(int count)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        ThrowIfDisposed();

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
        ArgumentOutOfRangeException.ThrowIfNegative(sizeHint);
        ThrowIfDisposed();

        // the contract forbids handing back an empty buffer, so 0 means "whatever is left, but at least one byte"
        if (sizeHint == 0)
            sizeHint = 1;

        EnsureCapacity(position + sizeHint);
        if (position > length && !_skipZeroing)
            ZeroRange(length, position - length);

        var (seg, off) = Locate(position);
        var current = _segments[seg];
        if (current.Length - off >= sizeHint)
            return (current.Array, off, current.Length - off);

        // nothing is addressed at or past the write head, so this segment's tail can simply be abandoned rather than copied somewhere larger
        if (position >= length)
        {
            var appended = AppendWritableSegment(seg, off, sizeHint);
            return (appended.Array, 0, appended.Length);
        }

        // there is live data past the write head, and moving it would renumber it, so the run has to be made contiguous the expensive way
        Consolidate(seg, off + (long)sizeHint);
        current = _segments[seg]; // merging preserves offsets relative to the start of seg, so off still points at position
        return (current.Array, off, current.Length - off);
    }
    // ends the segment holding the write head at `off`, so whatever follows starts exactly at position and can serve the run without anything being moved
    // only legal while position >= length: it renumbers every address past position, which is harmless only because nothing is stored there
    private BufferSegment<byte> AppendWritableSegment(int head, int off, int sizeHint)
    {
        var current = _segments[head];
        if (off == 0)
        {
            // the segment would address nothing at all, so it is dropped rather than left behind as a zero-length hole
            capacity -= current.Length;
            _pool.Return(current.Array);
            _segments.RemoveAt(head);
        }
        else
        {
            // the abandoned tail stays rented until the segment is released; wasting it is the entire point, since the alternative is copying the run
            capacity -= current.Length - off;
            _segments[head] = current.Truncate(off);
            head++;
        }

        // EnsureCapacity already rented the tail, and the segment that now starts at the head usually covers the run on its own
        if (head < _segments.Count && _segments[head].Length >= sizeHint)
            return _segments[head];

        // it does not, and nothing from the head on holds anything, so it can all go back
        for (var i = head; i < _segments.Count; i++)
        {
            capacity -= _segments[i].Length;
            _pool.Return(_segments[i].Array);
        }
        if (head < _segments.Count)
            _segments.RemoveRange(head, _segments.Count - head);

        // _minimumSegmentSize is capped at _maxSegmentSize by the constructor, so this exceeds the cap only when the caller asks for more than a capped segment could ever hold
        var arr = _pool.Rent(int.Max(sizeHint, _minimumSegmentSize));
        var appended = BufferSegment<byte>.Full(arr);
        _segments.Add(appended);
        capacity += appended.Length;
        return appended;
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
        var copied = 0;
        for (var i = first; i < last; i++)
        {
            var current = segments[i];
            current.Span.CopyTo(buffer.AsSpan(copied));
            copied += current.Length;
            _pool.Return(current.Array);
        }

        // the pool's rounding surplus can only be addressed when the merge ran to the very end, since anywhere else it would shift every following segment
        var addressed = last == _segments.Count ? buffer.Length : (int)merged;
        _segments[first] = new BufferSegment<byte>(buffer, addressed);
        _segments.RemoveRange(first + 1, last - first - 1);
        capacity += addressed - merged;
    }
    // deliberately not bounded by _maxSegmentSize: the run has to be contiguous and has to start at position, which no capped segment can offer once position sits deep enough inside one
    private byte[] RentContiguous(long length) => length <= Array.MaxLength ? _pool.Rent((int)length) : throw new OutOfMemoryException("A contiguous buffer of the requested size cannot be rented.");
    #endregion
}
