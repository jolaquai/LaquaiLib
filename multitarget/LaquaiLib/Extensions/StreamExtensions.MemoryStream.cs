namespace LaquaiLib.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="Stream"/> Type.
/// </summary>
public static partial class StreamExtensions
{
    extension(MemoryStream stream)
    {
        /// <summary>
        /// Gets a <see cref="Span{T}"/> over the backing storage of the specified <see cref="MemoryStream"/>.
        /// </summary>
        /// <returns>A <see cref="Span{T}"/> over the backing storage of the specified <see cref="MemoryStream"/>.</returns>
        /// <remarks>
        /// This should be treated with the same care as a <see cref="Span{T}"/> returned from <see cref="CollectionsMarshal.AsSpan{T}(List{T}?)"/>.
        /// Do not use the <see cref="MemoryStream"/> while the <see cref="Span{T}"/> is in use.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<byte> AsSpan() => UnsafeUtils.Accessors.MemoryStreamAccessors._buffer(stream);
        /// <summary>
        /// Gets a <see cref="Span{T}"/> over a section of the backing storage of the specified <see cref="MemoryStream"/>.
        /// </summary>
        /// <param name="start">The starting index of the slice.</param>
        /// <returns>A <see cref="Span{T}"/> over the backing storage of the specified <see cref="MemoryStream"/>.</returns>
        /// <remarks>
        /// This should be treated with the same care as a <see cref="Span{T}"/> returned from <see cref="CollectionsMarshal.AsSpan{T}(List{T}?)"/>.
        /// Do not use the <see cref="MemoryStream"/> while the <see cref="Span{T}"/> is in use.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<byte> AsSpan(Index start) => UnsafeUtils.Accessors.MemoryStreamAccessors._buffer(stream).AsSpan(start);
        /// <summary>
        /// Gets a <see cref="Span{T}"/> over a section of the backing storage of the specified <see cref="MemoryStream"/>.
        /// </summary>
        /// <param name="start">The starting index of the slice.</param>
        /// <returns>A <see cref="Span{T}"/> over the backing storage of the specified <see cref="MemoryStream"/>.</returns>
        /// <remarks>
        /// This should be treated with the same care as a <see cref="Span{T}"/> returned from <see cref="CollectionsMarshal.AsSpan{T}(List{T}?)"/>.
        /// Do not use the <see cref="MemoryStream"/> while the <see cref="Span{T}"/> is in use.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<byte> AsSpan(int start) => UnsafeUtils.Accessors.MemoryStreamAccessors._buffer(stream).AsSpan(start);
        /// <summary>
        /// Gets a <see cref="Span{T}"/> over a section of the backing storage of the specified <see cref="MemoryStream"/>.
        /// </summary>
        /// <param name="range">The range in the backing storage to get a <see cref="Span{T}"/> over.</param>
        /// <returns>A <see cref="Span{T}"/> over the backing storage of the specified <see cref="MemoryStream"/>.</returns>
        /// <remarks>
        /// This should be treated with the same care as a <see cref="Span{T}"/> returned from <see cref="CollectionsMarshal.AsSpan{T}(List{T}?)"/>.
        /// Do not use the <see cref="MemoryStream"/> while the <see cref="Span{T}"/> is in use.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<byte> AsSpan(Range range) => UnsafeUtils.Accessors.MemoryStreamAccessors._buffer(stream).AsSpan(range);
        /// <summary>
        /// Gets a <see cref="Span{T}"/> over a section of the backing storage of the specified <see cref="MemoryStream"/>.
        /// </summary>
        /// <param name="start">The starting index of the slice.</param>
        /// <param name="length">The length of the slice.</param>
        /// <returns>A <see cref="Span{T}"/> over the backing storage of the specified <see cref="MemoryStream"/>.</returns>
        /// <remarks>
        /// This should be treated with the same care as a <see cref="Span{T}"/> returned from <see cref="CollectionsMarshal.AsSpan{T}(List{T}?)"/>.
        /// Do not use the <see cref="MemoryStream"/> while the <see cref="Span{T}"/> is in use.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<byte> AsSpan(int start, int length) => UnsafeUtils.Accessors.MemoryStreamAccessors._buffer(stream).AsSpan(start, length);
        /// <summary>
        /// Gets a <see cref="Memory{T}"/> over the backing storage of the specified <see cref="MemoryStream"/>.
        /// </summary>
        /// <returns>A <see cref="Memory{T}"/> over the backing storage of the specified <see cref="MemoryStream"/>.</returns>
        /// <remarks>
        /// This should be treated with the same care as a <see cref="Span{T}"/> returned from <see cref="CollectionsMarshal.AsSpan{T}(List{T}?)"/>.
        /// Do not use the <see cref="MemoryStream"/> while the <see cref="Memory{T}"/> is in use.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Memory<byte> AsMemory() => UnsafeUtils.Accessors.MemoryStreamAccessors._buffer(stream);
        /// <summary>
        /// Gets a <see cref="Memory{T}"/> over a section of the backing storage of the specified <see cref="MemoryStream"/>.
        /// </summary>
        /// <param name="start">The starting index of the slice.</param>
        /// <returns>A <see cref="Memory{T}"/> over the backing storage of the specified <see cref="MemoryStream"/>.</returns>
        /// <remarks>
        /// This should be treated with the same care as a <see cref="Span{T}"/> returned from <see cref="CollectionsMarshal.AsSpan{T}(List{T}?)"/>.
        /// Do not use the <see cref="MemoryStream"/> while the <see cref="Memory{T}"/> is in use.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Memory<byte> AsMemory(Index start) => UnsafeUtils.Accessors.MemoryStreamAccessors._buffer(stream).AsMemory(start);
        /// <summary>
        /// Gets a <see cref="Memory{T}"/> over a section of the backing storage of the specified <see cref="MemoryStream"/>.
        /// </summary>
        /// <param name="start">The starting index of the slice.</param>
        /// <returns>A <see cref="Memory{T}"/> over the backing storage of the specified <see cref="MemoryStream"/>.</returns>
        /// <remarks>
        /// This should be treated with the same care as a <see cref="Span{T}"/> returned from <see cref="CollectionsMarshal.AsSpan{T}(List{T}?)"/>.
        /// Do not use the <see cref="MemoryStream"/> while the <see cref="Memory{T}"/> is in use.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Memory<byte> AsMemory(int start) => UnsafeUtils.Accessors.MemoryStreamAccessors._buffer(stream).AsMemory(start);
        /// <summary>
        /// Gets a <see cref="Memory{T}"/> over a section of the backing storage of the specified <see cref="MemoryStream"/>.
        /// </summary>
        /// <param name="range">The range in the backing storage to get a <see cref="Memory{T}"/> over.</param>
        /// <returns>A <see cref="Memory{T}"/> over the backing storage of the specified <see cref="MemoryStream"/>.</returns>
        /// <remarks>
        /// This should be treated with the same care as a <see cref="Span{T}"/> returned from <see cref="CollectionsMarshal.AsSpan{T}(List{T}?)"/>.
        /// Do not use the <see cref="MemoryStream"/> while the <see cref="Memory{T}"/> is in use.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Memory<byte> AsMemory(Range range) => UnsafeUtils.Accessors.MemoryStreamAccessors._buffer(stream).AsMemory(range);
        /// <summary>
        /// Gets a <see cref="Memory{T}"/> over a section of the backing storage of the specified <see cref="MemoryStream"/>.
        /// </summary>
        /// <param name="start">The starting index of the slice.</param>
        /// <param name="length">The length of the slice.</param>
        /// <returns>A <see cref="Memory{T}"/> over the backing storage of the specified <see cref="MemoryStream"/>.</returns>
        /// <remarks>
        /// This should be treated with the same care as a <see cref="Span{T}"/> returned from <see cref="CollectionsMarshal.AsSpan{T}(List{T}?)"/>.
        /// Do not use the <see cref="MemoryStream"/> while the <see cref="Memory{T}"/> is in use.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Memory<byte> AsMemory(int start, int length) => UnsafeUtils.Accessors.MemoryStreamAccessors._buffer(stream).AsMemory(start, length);

        /// <summary>
        /// Creates and returns an exact copy of this <see cref="MemoryStream"/>; its backing store references the same byte array as the original stream.
        /// It is, of course, capable of maintaining its own position and length within that array.
        /// Concurrent reads on the two streams are safe, but writing should be synchronized if it cannot be guaranteed that the segments being written do not overlap.
        /// Using a <see cref="ReaderWriterLockSlim"/> to manage this is recommended if writing is necessary.
        /// <para/>Note that the two streams will lose synchronization if write operations cause the backing array to be resized. Specify <paramref name="writable"/> as <see langword="false"/> to prevent this.
        /// </summary>
        /// <returns>A new <see cref="MemoryStream"/> as described.</returns>
        private MemoryStream Duplicate(bool writable, bool expandable)
        {
            var newMs = new MemoryStream();
            UnsafeUtils.Accessors.MemoryStreamAccessors._buffer(newMs) = UnsafeUtils.Accessors.MemoryStreamAccessors._buffer(stream);
            UnsafeUtils.Accessors.MemoryStreamAccessors._capacity(newMs) = UnsafeUtils.Accessors.MemoryStreamAccessors._capacity(stream);
            UnsafeUtils.Accessors.MemoryStreamAccessors._length(newMs) = UnsafeUtils.Accessors.MemoryStreamAccessors._length(stream);
            UnsafeUtils.Accessors.MemoryStreamAccessors._position(newMs) = UnsafeUtils.Accessors.MemoryStreamAccessors._position(stream);

            UnsafeUtils.Accessors.MemoryStreamAccessors._writable(newMs) = writable;
            UnsafeUtils.Accessors.MemoryStreamAccessors._expandable(newMs) = expandable;

            return newMs;
        }
        /// <summary>
        /// Reads a block of bytes from the current stream and writes the data to a given span.
        /// </summary>
        /// <param name="position">The byte offset in the stream at which to begin reading.</param>
        /// <param name="length">The number of bytes to read.</param>
        /// <param name="destination">The <see cref="Span{T}"/> to write the data to.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="position"/> is less than zero or <paramref name="position"/> + <paramref name="length"/> is greater than the length of the stream.</exception>
        public void CopyBlock(int position, int length, Span<byte> destination)
        {
            if (position < 0 || position + length > stream.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(position));
            }
            stream.AsSpan().Slice(position, length).CopyTo(destination);
        }
    }
}
