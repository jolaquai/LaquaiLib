namespace LaquaiLib.Extensions;

/// <summary>
/// Provides extensions for the <see cref="Stream"/> type.
/// </summary>
public static partial class StreamExtensions
{
    extension(MemoryStream stream)
    {
        /// <summary>
        /// Gets a <see cref="Span{T}"/> over the entire backing storage of the specified <see cref="MemoryStream"/>.
        /// </summary>
        /// <returns>A <see cref="Span{T}"/> over the entire backing storage of the specified <see cref="MemoryStream"/>.</returns>
        /// <remarks>
        /// Offsets are relative to the start of the backing array, not the stream: the stream's origin and length are ignored, so the span includes unused capacity and bytes before the origin.
        /// This should be treated with the same care as a <see cref="Span{T}"/> returned from <see cref="CollectionsMarshal.AsSpan{T}(List{T}?)"/>.
        /// Do not use the <see cref="MemoryStream"/> while the <see cref="Span{T}"/> is in use.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<byte> AsSpanUnsafe() => MemoryStreamAccessors._buffer(stream);
        /// <summary>
        /// Gets a <see cref="Span{T}"/> over a section of the entire backing storage of the specified <see cref="MemoryStream"/>.
        /// </summary>
        /// <param name="start">The starting index of the slice.</param>
        /// <returns>A <see cref="Span{T}"/> over the entire backing storage of the specified <see cref="MemoryStream"/>.</returns>
        /// <remarks>
        /// Offsets are relative to the start of the backing array, not the stream: the stream's origin and length are ignored, so the span includes unused capacity and bytes before the origin.
        /// This should be treated with the same care as a <see cref="Span{T}"/> returned from <see cref="CollectionsMarshal.AsSpan{T}(List{T}?)"/>.
        /// Do not use the <see cref="MemoryStream"/> while the <see cref="Span{T}"/> is in use.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<byte> AsSpanUnsafe(Index start) => MemoryStreamAccessors._buffer(stream).AsSpan(start);
        /// <summary>
        /// Gets a <see cref="Span{T}"/> over a section of the entire backing storage of the specified <see cref="MemoryStream"/>.
        /// </summary>
        /// <param name="start">The starting index of the slice.</param>
        /// <returns>A <see cref="Span{T}"/> over the entire backing storage of the specified <see cref="MemoryStream"/>.</returns>
        /// <remarks>
        /// Offsets are relative to the start of the backing array, not the stream: the stream's origin and length are ignored, so the span includes unused capacity and bytes before the origin.
        /// This should be treated with the same care as a <see cref="Span{T}"/> returned from <see cref="CollectionsMarshal.AsSpan{T}(List{T}?)"/>.
        /// Do not use the <see cref="MemoryStream"/> while the <see cref="Span{T}"/> is in use.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<byte> AsSpanUnsafe(int start) => MemoryStreamAccessors._buffer(stream).AsSpan(start);
        /// <summary>
        /// Gets a <see cref="Span{T}"/> over a section of the entire backing storage of the specified <see cref="MemoryStream"/>.
        /// </summary>
        /// <param name="range">The range in the backing storage to get a <see cref="Span{T}"/> over.</param>
        /// <returns>A <see cref="Span{T}"/> over the entire backing storage of the specified <see cref="MemoryStream"/>.</returns>
        /// <remarks>
        /// Offsets are relative to the start of the backing array, not the stream: the stream's origin and length are ignored, so the span includes unused capacity and bytes before the origin.
        /// This should be treated with the same care as a <see cref="Span{T}"/> returned from <see cref="CollectionsMarshal.AsSpan{T}(List{T}?)"/>.
        /// Do not use the <see cref="MemoryStream"/> while the <see cref="Span{T}"/> is in use.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<byte> AsSpanUnsafe(Range range) => MemoryStreamAccessors._buffer(stream).AsSpan(range);
        /// <summary>
        /// Gets a <see cref="Span{T}"/> over a section of the entire backing storage of the specified <see cref="MemoryStream"/>.
        /// </summary>
        /// <param name="start">The starting index of the slice.</param>
        /// <param name="length">The length of the slice.</param>
        /// <returns>A <see cref="Span{T}"/> over the entire backing storage of the specified <see cref="MemoryStream"/>.</returns>
        /// <remarks>
        /// Offsets are relative to the start of the backing array, not the stream: the stream's origin and length are ignored, so the span includes unused capacity and bytes before the origin.
        /// This should be treated with the same care as a <see cref="Span{T}"/> returned from <see cref="CollectionsMarshal.AsSpan{T}(List{T}?)"/>.
        /// Do not use the <see cref="MemoryStream"/> while the <see cref="Span{T}"/> is in use.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<byte> AsSpanUnsafe(int start, int length) => MemoryStreamAccessors._buffer(stream).AsSpan(start, length);
        /// <summary>
        /// Gets a <see cref="Span{T}"/> over the visible contents of the specified <see cref="MemoryStream"/>, from its origin up to its <see cref="Stream.Length"/>.
        /// </summary>
        /// <returns>A <see cref="Span{T}"/> over the contents of the specified <see cref="MemoryStream"/>.</returns>
        /// <remarks>
        /// Offsets are relative to the start of the stream, like <see cref="Stream.Position"/>.
        /// Do not resize or otherwise use the <see cref="MemoryStream"/> while the <see cref="Span{T}"/> is in use.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<byte> AsSpan() => MemoryStreamAccessors._buffer(stream).AsSpan(MemoryStreamAccessors._origin(stream), MemoryStreamAccessors._length(stream) - MemoryStreamAccessors._origin(stream));
        /// <summary>
        /// Gets a <see cref="Span{T}"/> over a section of the visible contents of the specified <see cref="MemoryStream"/>.
        /// </summary>
        /// <param name="start">The starting index of the slice, relative to the start of the stream.</param>
        /// <returns>A <see cref="Span{T}"/> over the section.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="start"/> is outside the stream's contents.</exception>
        /// <inheritdoc cref="AsSpan(MemoryStream)" path="/remarks"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<byte> AsSpan(Index start) => stream.AsSpan()[start..];
        /// <inheritdoc cref="AsSpan(MemoryStream, Index)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<byte> AsSpan(int start) => stream.AsSpan()[start..];
        /// <summary>
        /// Gets a <see cref="Span{T}"/> over a section of the visible contents of the specified <see cref="MemoryStream"/>.
        /// </summary>
        /// <param name="range">The range in the stream's contents to get a <see cref="Span{T}"/> over.</param>
        /// <returns>A <see cref="Span{T}"/> over the section.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="range"/> is outside the stream's contents.</exception>
        /// <inheritdoc cref="AsSpan(MemoryStream)" path="/remarks"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<byte> AsSpan(Range range) => stream.AsSpan()[range];
        /// <summary>
        /// Gets a <see cref="Span{T}"/> over a section of the visible contents of the specified <see cref="MemoryStream"/>.
        /// </summary>
        /// <param name="start">The starting index of the slice, relative to the start of the stream.</param>
        /// <param name="length">The length of the slice.</param>
        /// <returns>A <see cref="Span{T}"/> over the section.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the section is outside the stream's contents.</exception>
        /// <inheritdoc cref="AsSpan(MemoryStream)" path="/remarks"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<byte> AsSpan(int start, int length) => stream.AsSpan().Slice(start, length);
        /// <summary>
        /// Gets a <see cref="Memory{T}"/> over the backing storage of the specified <see cref="MemoryStream"/>.
        /// </summary>
        /// <returns>A <see cref="Memory{T}"/> over the backing storage of the specified <see cref="MemoryStream"/>.</returns>
        /// <remarks>
        /// This should be treated with the same care as a <see cref="Span{T}"/> returned from <see cref="CollectionsMarshal.AsSpan{T}(List{T}?)"/>.
        /// Do not use the <see cref="MemoryStream"/> while the <see cref="Memory{T}"/> is in use.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Memory<byte> AsMemory() => MemoryStreamAccessors._buffer(stream);
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
        public Memory<byte> AsMemory(Index start) => MemoryStreamAccessors._buffer(stream).AsMemory(start);
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
        public Memory<byte> AsMemory(int start) => MemoryStreamAccessors._buffer(stream).AsMemory(start);
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
        public Memory<byte> AsMemory(Range range) => MemoryStreamAccessors._buffer(stream).AsMemory(range);
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
        public Memory<byte> AsMemory(int start, int length) => MemoryStreamAccessors._buffer(stream).AsMemory(start, length);

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
                throw new ArgumentOutOfRangeException(nameof(position));
            stream.AsSpan().Slice(position, length).CopyTo(destination);
        }

        /// <summary>
        /// Creates a <see cref="MemoryStream"/> that directly uses <paramref name="bytes"/> as if it were created empty and then written to.
        /// This method is unsafe because 1. the returned <see cref="MemoryStream"/> will drop <paramref name="bytes"/> upon resizing and 2. <paramref name="bytes"/> is aliased by the stream.
        /// </summary>
        /// <param name="bytes">The byte array to use as the backing store for the <see cref="MemoryStream"/>.</param>
        /// <param name="origin">The index in <paramref name="bytes"/> at which the <see cref="MemoryStream"/> starts.</param>
        /// <param name="length">The initial length to assign to the <see cref="MemoryStream"/>. If <see langword="null"/>, defaults to the remaining length of <paramref name="bytes"/> after <paramref name="origin"/>.</param>
        /// <param name="position">The initial position to assign to the <see cref="MemoryStream"/>.</param>
        /// <param name="exposable">Whether the <see cref="MemoryStream"/> should expose its internal buffer through <see cref="MemoryStream.GetBuffer"/> and <see cref="MemoryStream.TryGetBuffer(out ArraySegment{byte})"/>.</param>
        /// <returns>The created <see cref="MemoryStream"/>.</returns>
        public static MemoryStream UnsafeFromByteArray(byte[] bytes, int origin = 0, int? length = null, int position = 0, bool exposable = true)
        {
            var len = length ?? bytes.Length - origin;

            if ((uint)origin > (uint)bytes.Length)
                throw new ArgumentOutOfRangeException(nameof(origin), "Origin must be greater than or equal to zero and less than or equal to the length of the specified byte array.");
            if ((uint)position > (uint)(bytes.Length - origin))
                throw new ArgumentOutOfRangeException(nameof(position), "Initial position must be greater than or equal to zero and less than or equal to the length of the specified byte array.");
            if ((uint)len > (uint)(bytes.Length - origin))
                throw new ArgumentOutOfRangeException(nameof(length), "Initial length must be greater than or equal to zero and less than or equal to the length of the specified byte array.");

            var ms = RuntimeHelpers.GetUninitializedObject<MemoryStream>();
            MemoryStreamAccessors._buffer(ms) = bytes;
            MemoryStreamAccessors._origin(ms) = origin;
            MemoryStreamAccessors._length(ms) = origin + len;
            MemoryStreamAccessors._position(ms) = origin + position;
            MemoryStreamAccessors._capacity(ms) = bytes.Length;
            MemoryStreamAccessors._expandable(ms) = true;
            MemoryStreamAccessors._writable(ms) = true;
            MemoryStreamAccessors._exposable(ms) = exposable;
            MemoryStreamAccessors._isOpen(ms) = true;
            return ms;
        }
    }
}
