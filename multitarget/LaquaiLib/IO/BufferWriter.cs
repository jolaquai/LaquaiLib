using System.Buffers;

using LaquaiLib.Extensions;
using LaquaiLib.UnsafeUtils.Accessors;

namespace LaquaiLib.IO;

/// <summary>
/// Represents a position in a segmented chain of buffers.
/// </summary>
/// <typeparam name="T">The type of elements in the buffer.</typeparam>
/// <param name="SegmentIndex">The index of the segment in the chain.</param>
/// <param name="Segment">A <see cref="Memory{T}"/> representing the segment.</param>
/// <param name="Offset">The offset within the segment.</param>
public readonly record struct SegmentOffset<T>(int SegmentIndex, Memory<T> Segment, int Offset);

/// <summary>
/// Implements a base class for expandable <see cref="IBufferWriter{T}"/> implementations.
/// </summary>
/// <typeparam name="T">The type of elements written to the buffer.</typeparam>
public abstract class BufferWriterBase<T> : IBufferWriter<T>, IDisposable
{
    /// <inheritdoc/>
    public abstract void Advance(int count);
    /// <inheritdoc/>
    public abstract Memory<T> GetMemory(int sizeHint = 0);
    /// <inheritdoc/>
    public virtual Span<T> GetSpan(int sizeHint = 0) => GetMemory(sizeHint).Span;
    /// <summary>
    /// For a contiguous backing memory model: Returns the current buffer of the writer, potentially grown so that it can fit at least <paramref name="sizeHint"/> more instances of <typeparamref name="T"/>.
    /// <para/>For a segmented backing memory model: Returns either the current segment of the buffer if it can fit at least <paramref name="sizeHint"/> more instances of <typeparamref name="T"/>, or the next segment of the buffer, created so that it can fit at least <paramref name="sizeHint"/> more instances of <typeparamref name="T"/>.
    /// </summary>
    /// <param name="sizeHint">The minimum number of <typeparamref name="T"/> instances that the returned segment should be able to accommodate.</param>
    /// <returns>A <see cref="Memory{T}"/> that represents either the entire new buffer or the next segment of the buffer, depending on the backing memory model.</returns>
    protected abstract Memory<T> Next(int sizeHint = 0);

    /// <summary>
    /// Truncates the contents of the buffer to the specified length.
    /// </summary>
    /// <param name="length">The new length of the buffer.</param>
    public abstract void SetLength(long length);
    /// <summary>
    /// Clears the contents of the buffer, optionally zeroing out the memory.
    /// </summary>
    /// <param name="zero">Whether to zero out the memory of the buffer. If <typeparamref name="T"/> is a reference type, the memory will always be zeroed to prevent memory leaks.</param>
    public abstract void Clear(bool zero = false);

    /// <summary>
    /// Disposes of the buffer writer and releases any resources associated with it.
    /// </summary>
    public abstract void Dispose();
}

/// <summary>
/// Implements an <see cref="IBufferWriter{T}"/> that writes to caller supplied buffers with support for segmented backing storage.
/// </summary>
/// <typeparam name="T">The type of elements written to the buffer.</typeparam>
public abstract class SegmentedBufferWriterBase<T> : BufferWriterBase<T>
{
    /// <summary>
    /// The index of <see cref="currentSegment"/> in the segmented buffer.
    /// </summary>
    protected int segment = -1;
    /// <summary>
    /// The offset of the next write in <see cref="currentSegment"/>.
    /// </summary>
    protected int index = -1;
    /// <summary>
    /// The current segment of the segmented buffer.
    /// </summary>
    protected Memory<T> currentSegment;

    /// <summary>
    /// Gets a <see cref="SegmentOffset{T}"/> that represents the position in the segmented buffer where the next write will occur.
    /// </summary>
    public SegmentOffset<T> Position => new(segment, currentSegment, index);
    /// <summary>
    /// Gets a <see langword="long"/> that represents the absolute position from the start of the segment chain.
    /// </summary>
    public abstract long AbsoluteLength { get; }

    /// <summary>
    /// Notifies the writer that <paramref name="count"/> elements have been written to the buffer.
    /// </summary>
    /// <param name="count">The number of elements written to the buffer.</param>
    public override void Advance(int count)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        if (segment == -1 || index == -1 || currentSegment.IsEmpty)
            throw new InvalidOperationException("BufferWriter is not initialized.");
        if (index + count > currentSegment.Length)
            throw new ArgumentOutOfRangeException(nameof(count), "Cannot advance past the end of the current segment.");

        index += count;
    }
}
/// <summary>
/// Implements an <see cref="IBufferWriter{T}"/> that writes to a contiguous buffer with support for segmented backing storage.
/// </summary>
/// <typeparam name="T">The type of elements written to the buffer.</typeparam>
public abstract class ContiguousBufferWriterBase<T> : BufferWriterBase<T>
{
    /// <summary>
    /// The contiguous underlying buffer of the writer.
    /// </summary>
    protected virtual Memory<T> Buffer { get; set; }

    /// <summary>
    /// Gets the position in the buffer where the next write will occur.
    /// </summary>
    public virtual int Length { get; protected set; }

    /// <inheritdoc/>
    public override void Advance(int count)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        if (Length == -1 || Buffer.IsEmpty)
            throw new InvalidOperationException("BufferWriter is not initialized.");
        if (Length + count > Buffer.Length)
            throw new ArgumentOutOfRangeException(nameof(count), "Cannot advance past the end of the buffer.");

        Length += count;
    }
    /// <inheritdoc/>
    public override Memory<T> GetMemory(int sizeHint = 0)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(sizeHint);

        var length = Length;
        var buffer = Buffer;
        // IBufferWriter guarantees a non-empty return even for sizeHint 0, so an exhausted buffer must grow either way
        if (buffer.Length - length < int.Max(sizeHint, 1))
            buffer = Next(sizeHint);
        return buffer[length..];
    }
    /// <inheritdoc/>
    public override void Clear(bool zero = false)
    {
        if (zero || RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            Buffer[..Length].Span.ZeroMemory();
        SetLength(0);
    }
}

/// <summary>
/// Implements <see cref="BufferWriterBase{T}"/> that grows by renting segments from an <see cref="ArrayPool{T}"/>.
/// </summary>
/// <typeparam name="T">The type of elements written to the buffer.</typeparam>
/// <remarks>
/// A request the current segment cannot satisfy seals that segment where it stands and chains a new one behind it, so data that has already been written is never copied. A sealed segment may therefore retain an unused tail, which means the total rented capacity can exceed <see cref="SegmentedBufferWriterBase{T}.AbsoluteLength"/>.
/// </remarks>
public sealed class PooledBufferWriter<T>(ArrayPool<T> pool = null, bool zeroOnDispose = false) : SegmentedBufferWriterBase<T>
{
    /// <summary>
    /// Enumerates the segments of the <see cref="PooledBufferWriter{T}"/> as <see cref="Memory{T}"/> instances.
    /// </summary>
    public struct SegmentEnumerator : IEnumerator<Memory<T>>
    {
        private readonly List<T[]> _segments;
        private readonly List<int> _lengths;
        private readonly int _count;
        private int _i;

        internal SegmentEnumerator(List<T[]> segments, List<int> lengths, int count)
        {
            _segments = segments;
            _lengths = lengths;
            _count = count;
            _i = -1;
        }

        /// <summary>
        /// Gets the current segment of the buffer as a <see cref="Memory{T}"/> instance. Only the written portion of the segment is exposed; the unused tail a sealed segment may carry is never visible.
        /// </summary>
        public readonly Memory<T> Current => _segments[_i].AsMemory(0, _lengths[_i]);

        /// <summary>
        /// Advances the enumerator to the next segment of the buffer.
        /// </summary>
        /// <returns><see langword="true"/> if the enumerator was successfully advanced to the next segment; <see langword="false"/> if the enumerator has passed the end of the buffer.</returns>
        public bool MoveNext()
        {
            if (_i >= _count)
                return false;
            return ++_i < _count;
        }
        /// <summary>
        /// Resets the enumerator to its initial position, which is before the first segment of the buffer.
        /// </summary>
        public void Reset() => _i = -1;

        readonly object IEnumerator.Current => Current;

        /// <summary>
        /// Disposes the enumerator and releases any resources associated with it.
        /// </summary>
        public readonly void Dispose() { }
    }

    private const int DefaultSegmentSize = 2048;
    /// <summary>
    /// The maximum number of elements that can be stored in a single segment, calculated based on the size of the element type <typeparamref name="T"/> and a fixed segment size of <see cref="DefaultSegmentSize"/> bytes.
    /// </summary>
    private static readonly int _maxElems = DefaultSegmentSize / Unsafe.SizeOf<T>();
    private readonly ArrayPool<T> _pool = pool ?? ArrayPool<T>.Shared;
    private readonly bool _zeroOnDispose = zeroOnDispose;
    private readonly List<T[]> _segments = [];
    // the number of elements actually written to each segment, index-aligned with _segments; entries for segments before the current one are authoritative, the entry for the current segment is stale until it is sealed or explicitly flushed from index
    private readonly List<int> _lengths = [];
    // the sum of _lengths[0..segment-1]; maintained incrementally so AbsoluteLength stays O(1) instead of walking the chain
    private long _priorLength;
    private bool _disposed;

    /// <inheritdoc/>
    public override long AbsoluteLength => segment < 0 ? 0 : _priorLength + index;

    /// <inheritdoc/>
    public override Memory<T> GetMemory(int sizeHint = 0)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentOutOfRangeException.ThrowIfNegative(sizeHint);
        return Next(sizeHint)[index..];
    }
    /// <inheritdoc/>
    // seal-and-chain, like the writer inside System.IO.Pipelines.Pipe: a segment that cannot satisfy the request is sealed where it stands and a new one is chained behind it, so committed data is never copied. The price is the unused tail a sealed segment keeps, which is far cheaper than an O(n) copy.
    protected sealed override Memory<T> Next(int sizeHint = 0)
    {
        var need = sizeHint > 0 ? sizeHint : 1;

        if (!currentSegment.IsEmpty && currentSegment.Length - index >= need)
            return currentSegment;

        if (segment >= 0)
        {
            // seal: index is the real written length, whatever capacity follows it is abandoned (but still owned, so Dispose returns it)
            _lengths[segment] = index;
            _priorLength += index;
        }

        index = 0;
        segment++;

        if (segment < _segments.Count && _segments[segment].Length >= need)
        {
            // we got SetLength'd toward the front and walked back out to a segment we already own, so just reuse it
            _lengths[segment] = 0;
            return currentSegment = _segments[segment];
        }

        var arr = _pool.Rent(int.Max(need, _maxElems));
        if (segment < _segments.Count)
        {
            // the segment we own here is too small for the request; its contents are past the write position and therefore already discarded
            _pool.Return(_segments[segment], _zeroOnDispose);
            _segments[segment] = arr;
            _lengths[segment] = 0;
        }
        else
        {
            _segments.Add(arr);
            _lengths.Add(0);
        }
        return currentSegment = arr;
    }
    /// <inheritdoc/>
    public override void SetLength(long length)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentOutOfRangeException.ThrowIfNegative(length);

        if (segment >= 0)
            _lengths[segment] = index;

        var current = AbsoluteLength;
        if (length > current)
            throw new ArgumentOutOfRangeException(nameof(length), $"Cannot set length beyond the current absolute length of the buffer. Use {nameof(Advance)}.");
        if (length == current)
            return;

        // segments are no longer guaranteed to be packed, so the walk has to consult the written lengths rather than the rented capacities. length < current bounds this to idx <= segment, so the indexing below is always in range.
        var remaining = length;
        var idx = 0;
        while (idx < _lengths.Count && remaining > _lengths[idx])
        {
            remaining -= _lengths[idx];
            idx++;
        }

        segment = idx;
        index = (int)remaining;
        // without this, subsequent writes would land in whatever segment happened to be current before the rewind
        currentSegment = _segments[idx];
        _priorLength = length - remaining;
    }
    /// <inheritdoc/>
    public override void Clear(bool zero = false)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (zero || RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            foreach (var segment in _segments)
                segment.AsSpan().ZeroMemory();
        SetLength(0);
    }

    /// <summary>
    /// Gets an enumerator that iterates through the segments of the buffer as <see cref="Memory{T}"/> instances.
    /// </summary>
    /// <returns>The enumerator for the segments of the buffer.</returns>
    /// <remarks>
    /// Each segment is exposed trimmed to the number of elements written to it, and enumeration stops at the segment holding the write position. Segments that exist only because the buffer was previously longer are not enumerated.
    /// </remarks>
    public SegmentEnumerator GetSegments()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (segment >= 0)
            _lengths[segment] = index;
        return new(_segments, _lengths, segment + 1);
    }
    /// <summary>
    /// Copies the data written so far to a new array and returns it.
    /// </summary>
    /// <returns>The created array.</returns>
    public T[] ToArray()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (segment >= 0)
            _lengths[segment] = index;

        var ret = new T[AbsoluteLength];
        var offset = 0;
        for (var i = 0; i <= segment; i++)
        {
            var len = _lengths[i];
            _segments[i].AsSpan(0, len).CopyTo(ret.AsSpan(offset));
            offset += len;
        }
        return ret;
    }

    /// <inheritdoc/>
    public override void Dispose()
    {
        if (_disposed)
            return;

        var zod = _zeroOnDispose;
        foreach (var segment in _segments)
            _pool.Return(segment, zod);
        _segments.Clear();
        _lengths.Clear();

        currentSegment = default;
        segment = -1;
        index = -1;
        _priorLength = 0;
        _disposed = true;
    }
}

/// <summary>
/// Implements a <see cref="BufferWriterBase{T}"/> that writes directly into the backing storage of a <see cref="MemoryStream"/>, bypassing the stream's own copy-based write path.
/// </summary>
/// <remarks>
/// The writer does not maintain a cursor of its own; <see cref="ContiguousBufferWriterBase{T}.Length"/> projects onto the stream's <see cref="Stream.Position"/> and advancing extends <see cref="Stream.Length"/> exactly as <see cref="Stream.Write(ReadOnlySpan{byte})"/> would. Writes through the writer and through the stream may therefore be interleaved freely.
/// <para/>Since the memory handed out aliases the stream's internal array, any previously returned <see cref="Memory{T}"/> or <see cref="Span{T}"/> is invalidated by whatever may reallocate that array, including a subsequent <see cref="GetMemory(int)"/>. Data written but not <see cref="BufferWriterBase{T}.Advance(int)"/>d is not carried across such a reallocation, matching <see cref="ArrayBufferWriter{T}"/>.
/// </remarks>
public sealed class MemoryStreamBufferWriter : ContiguousBufferWriterBase<byte>
{
    private readonly MemoryStream _ms;
    private readonly bool _leaveOpen;
    private bool _disposed;

    /// <summary>
    /// Gets the <see cref="MemoryStream"/> this instance writes to.
    /// </summary>
    public MemoryStream BaseStream => _ms;

    /// <summary>
    /// Gets the usable region of the stream's backing array. The span ends at the stream's capacity, not at the length of the array, so a stream created over a slice of a caller-supplied array cannot be written past its bounds.
    /// </summary>
    /// <exception cref="NotSupportedException">Thrown by the setter unconditionally; the backing storage belongs to the <see cref="MemoryStream"/>.</exception>
    protected override Memory<byte> Buffer
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            var origin = MemoryStreamAccessors._origin(_ms);
            return MemoryStreamAccessors._buffer(_ms).AsMemory(origin, MemoryStreamAccessors._capacity(_ms) - origin);
        }
        set => throw new NotSupportedException($"The backing storage of a {nameof(MemoryStreamBufferWriter)} is owned by its {nameof(MemoryStream)}.");
    }

    /// <inheritdoc/>
    public override int Length
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => MemoryStreamAccessors._position(_ms) - MemoryStreamAccessors._origin(_ms);
        protected set
        {
            var position = MemoryStreamAccessors._origin(_ms) + value;
            MemoryStreamAccessors._position(_ms) = position;
            ref var length = ref MemoryStreamAccessors._length(_ms);
            if (position > length)
                length = position;
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MemoryStreamBufferWriter"/> class that writes to a new, expandable <see cref="MemoryStream"/> exposed through <see cref="BaseStream"/> and disposed along with this instance.
    /// </summary>
    public MemoryStreamBufferWriter()
    {
        _ms = new();
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="MemoryStreamBufferWriter"/> class that writes to the specified <see cref="MemoryStream"/>, beginning at its current <see cref="Stream.Position"/>.
    /// </summary>
    /// <param name="stream">The <see cref="MemoryStream"/> to write to.</param>
    /// <param name="leaveOpen">Whether to leave the <see cref="MemoryStream"/> open after the <see cref="MemoryStreamBufferWriter"/> is disposed.</param>
    /// <exception cref="ArgumentException">Thrown if <paramref name="stream"/> is not writable.</exception>
    public MemoryStreamBufferWriter(MemoryStream stream, bool leaveOpen = false)
    {
        ArgumentNullException.ThrowIfNull(stream);
        if (!stream.CanWrite)
            throw new ArgumentException("The stream must be writable.", nameof(stream));

        _ms = stream;
        _leaveOpen = leaveOpen;
    }

    /// <inheritdoc/>
    public override Memory<byte> GetMemory(int sizeHint = 0)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        // a position seeked past the end of the stream leaves a gap of bytes that are not part of the stream; MemoryStream.Write zeroes it rather than publish whatever the array happened to hold, so do the same. Position may also sit past the capacity, in which case Next allocates a fresh (zeroed) array anyway and only the existing tail needs scrubbing.
        var position = int.Min(MemoryStreamAccessors._position(_ms), MemoryStreamAccessors._capacity(_ms));
        var length = MemoryStreamAccessors._length(_ms);
        if (position > length)
            MemoryStreamAccessors._buffer(_ms).AsSpan(length, position - length).ZeroMemory();
        return base.GetMemory(sizeHint);
    }
    /// <inheritdoc/>
    /// <exception cref="NotSupportedException">Thrown if the stream cannot grow to satisfy <paramref name="sizeHint"/> because it was created over a caller-supplied array.</exception>
    protected override Memory<byte> Next(int sizeHint = 0)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentOutOfRangeException.ThrowIfNegative(sizeHint);

        var need = int.Max(sizeHint, 1);
        var position = MemoryStreamAccessors._position(_ms);
        var capacity = MemoryStreamAccessors._capacity(_ms);
        if (capacity - position >= need)
            return Buffer;

        if (!MemoryStreamAccessors._expandable(_ms))
            throw new NotSupportedException($"The underlying {nameof(MemoryStream)} is not expandable and its remaining capacity of {capacity - position} bytes cannot satisfy a request for {need} bytes.");

        // _origin is zero whenever the stream is expandable, so from here on absolute and stream-relative offsets coincide
        var required = (long)position + need;
        if (required > Array.MaxLength)
            throw new ArgumentOutOfRangeException(nameof(sizeHint), $"Satisfying the request would grow the stream to {required} bytes, past the maximum supported length of {Array.MaxLength} bytes.");

        // same schedule MemoryStream.EnsureCapacity uses, so a writer-driven stream reallocates no more often than a Write-driven one
        var grown = long.Max(required, long.Max(256, 2L * capacity));
        _ms.Capacity = (int)long.Min(grown, Array.MaxLength);
        return Buffer;
    }
    /// <inheritdoc/>
    /// <remarks>
    /// Truncates the stream to <paramref name="length"/> bytes and pulls the write position back if it fell beyond the new end. The capacity is left untouched.
    /// </remarks>
    public override void SetLength(long length)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentOutOfRangeException.ThrowIfNegative(length);

        var origin = MemoryStreamAccessors._origin(_ms);
        ref var streamLength = ref MemoryStreamAccessors._length(_ms);
        if (length > streamLength - origin)
            throw new ArgumentOutOfRangeException(nameof(length), $"Cannot set length beyond the current length of the stream. Use {nameof(Advance)}.");

        streamLength = origin + (int)length;
        ref var position = ref MemoryStreamAccessors._position(_ms);
        if (position > streamLength)
            position = streamLength;
    }
    /// <inheritdoc/>
    public override void Clear(bool zero = false)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (zero)
        {
            var origin = MemoryStreamAccessors._origin(_ms);
            MemoryStreamAccessors._buffer(_ms).AsSpan(origin, MemoryStreamAccessors._length(_ms) - origin).ZeroMemory();
        }
        SetLength(0);
    }

    /// <inheritdoc/>
    public override void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        if (!_leaveOpen)
            _ms.Dispose();
    }
}