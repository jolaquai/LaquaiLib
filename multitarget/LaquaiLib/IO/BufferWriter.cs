using System.Buffers;

using LaquaiLib.Extensions;

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
    /// Returns either the current segment of the buffer if it is not full, or the next segment of the buffer.
    /// </summary>
    /// <param name="sizeHint">The desired size of the next segment.</param>
    /// <returns>The segment to write into next.</returns>
    protected abstract Memory<T> Next(int sizeHint = 0);

    /// <summary>
    /// Truncates the contents of the buffer to the specified length.
    /// </summary>
    /// <param name="length">The new length of the buffer.</param>
    public abstract void SetLength(long length);
    /// <summary>
    /// Clears the contents of the buffer, optionally zeroing out the memory.
    /// </summary>
    /// <param name="zero">Whether to zero out the memory of the buffer.</param>
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
    protected Memory<T> buffer;

    /// <summary>
    /// Gets the position in the buffer where the next write will occur.
    /// </summary>
    public int Length { get; private set; }

    /// <inheritdoc/>
    public override void Advance(int count)
    {
        if (Length == -1 || buffer.IsEmpty)
            throw new InvalidOperationException("BufferWriter is not initialized.");
        if (Length + count > buffer.Length)
            throw new ArgumentOutOfRangeException(nameof(count), "Cannot advance past the end of the buffer.");

        Length += count;
    }
    /// <inheritdoc/>
    public override Memory<T> GetMemory(int sizeHint = 0) => buffer[Length..];
    /// <inheritdoc/>
    public override void Clear(bool zero = false)
    {
        if (zero)
            buffer[..Length].Span.Clear();
        SetLength(0);
    }
}

/// <summary>
/// Implements <see cref="BufferWriterBase{T}"/> that grows by renting segments from an <see cref="ArrayPool{T}"/>.
/// </summary>
/// <typeparam name="T">The type of elements written to the buffer.</typeparam>
public sealed class PooledBufferWriter<T>(ArrayPool<T> pool = null, bool zeroOnDispose = false) : SegmentedBufferWriterBase<T>
{
    /// <summary>
    /// Enumerates the segments of the <see cref="PooledBufferWriter{T}"/> as <see cref="Memory{T}"/> instances.
    /// </summary>
    public struct SegmentEnumerator : IEnumerator<Memory<T>>
    {
        private readonly List<T[]> _segments;
        private int _state;
        private List<T[]>.Enumerator _inner;

        internal SegmentEnumerator(List<T[]> segments)
        {
            _segments = segments;
        }

        /// <summary>
        /// Gets the current segment of the buffer as a <see cref="Memory{T}"/> instance.
        /// </summary>
        public readonly Memory<T> Current => _inner.Current;

        /// <summary>
        /// Advances the enumerator to the next segment of the buffer.
        /// </summary>
        /// <returns><see langword="true"/> if the enumerator was successfully advanced to the next segment; <see langword="false"/> if the enumerator has passed the end of the buffer.</returns>
        public bool MoveNext()
        {
            if (_state == 2)
                return false;
            if (_state == 0)
                Reset();
            if (_inner.MoveNext())
                return true;
            _state = 2;
            return false;
        }
        /// <summary>
        /// Resets the enumerator to its initial position, which is before the first segment of the buffer.
        /// </summary>
        public void Reset()
        {
            _inner.Dispose();
            _inner = _segments.GetEnumerator();
            _state = 1;
        }

        readonly object IEnumerator.Current => Current;

        /// <summary>
        /// Disposes the enumerator and releases any resources associated with it.
        /// </summary>
        public readonly void Dispose() => _inner.Dispose();
    }

    private const int DefaultSegmentSize = 2048;
    /// <summary>
    /// The maximum number of elements that can be stored in a single segment, calculated based on the size of the element type <typeparamref name="T"/> and a fixed segment size of <see cref="DefaultSegmentSize"/> bytes.
    /// </summary>
    private static readonly int _maxElems = DefaultSegmentSize / Unsafe.SizeOf<T>();
    private readonly ArrayPool<T> _pool = pool ?? ArrayPool<T>.Shared;
    private readonly bool _zeroOnDispose = zeroOnDispose;
    private readonly List<T[]> _segments = [];

    /// <inheritdoc/>
    public override long AbsoluteLength => SegmentedBufferHelpers.RelativeToAbsolute(CollectionsMarshal.AsSpan(_segments), segment, index);

    /// <inheritdoc/>
    public override Memory<T> GetMemory(int sizeHint = 0) => Next()[index..];
    /// <inheritdoc/>
    protected sealed override Memory<T> Next(int sizeHint = 0)
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

        var arr = _pool.Rent(sizeHint < _maxElems && sizeHint > 0 ? sizeHint : _maxElems);
        _segments.Add(arr);
        return currentSegment = arr;
    }
    /// <inheritdoc/>
    public override void SetLength(long length)
    {
        if (length > AbsoluteLength)
            throw new ArgumentOutOfRangeException(nameof(length), $"Cannot set length beyond the current absolute length of the buffer. Use {nameof(Advance)}.");

        var (idx, off) = SegmentedBufferHelpers.AbsoluteToRelative(CollectionsMarshal.AsSpan(_segments), length);
        segment = idx;
        index = off;
    }
    /// <inheritdoc/>
    public override void Clear(bool zero = false)
    {
        if (zero)
            foreach (var segment in _segments)
                segment.AsSpan().ZeroMemory();
        SetLength(0);
    }

    /// <summary>
    /// Gets an enumerator that iterates through the segments of the buffer as <see cref="Memory{T}"/> instances.
    /// </summary>
    /// <returns>The enumerator for the segments of the buffer.</returns>
    public SegmentEnumerator GetSegments() => new(_segments);
    /// <summary>
    /// Copies the data written so far to a new array and returns it.
    /// </summary>
    /// <returns>The created array.</returns>
    public T[] ToArray()
    {
        var ret = new T[AbsoluteLength];
        var offset = 0;
        for (var i = 0; i <= segment; i++)
        {
            var seg = _segments[i];
            var len = i == segment ? index : seg.Length;
            seg.AsSpan(0, len).CopyTo(ret.AsSpan(offset));
            offset += len;
        }
        return ret;
    }

    /// <inheritdoc/>
    public override void Dispose()
    {
        var zod = _zeroOnDispose;
        foreach (var segment in _segments)
            _pool.Return(segment, zod);
        _segments.Clear();
    }
}