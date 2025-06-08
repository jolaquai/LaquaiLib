namespace LaquaiLib.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="Stream"/> Type.
/// </summary>
public static partial class StreamExtensions
{
    [UnsafeAccessor(UnsafeAccessorKind.Field)] private static extern ref byte[] _buffer(this MemoryStream _);
    [UnsafeAccessor(UnsafeAccessorKind.Field)] private static extern ref int _capacity(this MemoryStream _);
    [UnsafeAccessor(UnsafeAccessorKind.Field)] private static extern ref int _length(this MemoryStream _);
    [UnsafeAccessor(UnsafeAccessorKind.Field)] private static extern ref int _position(this MemoryStream _);

    extension(MemoryStream stream)
    {
        /// <summary>
        /// Gets a <see cref="Span{T}"/> over the backing storage of the specified <see cref="MemoryStream"/>.
        /// </summary>
        /// <param name="stream">The <see cref="MemoryStream"/> to get the backing storage of.</param>
        /// <returns>A <see cref="Span{T}"/> over the backing storage of the specified <see cref="MemoryStream"/>.</returns>
        /// <remarks>
        /// This should be treated with the same care as a <see cref="Span{T}"/> returned from <see cref="CollectionsMarshal.AsSpan{T}(List{T}?)"/>.
        /// Do not use the <see cref="MemoryStream"/> while the <see cref="Span{T}"/> is in use.
        /// </remarks>
        public Span<byte> AsSpan() => stream._buffer();
        /// <summary>
        /// Gets a <see cref="Span{T}"/> over a section of the backing storage of the specified <see cref="MemoryStream"/>.
        /// </summary>
        /// <param name="stream">The <see cref="MemoryStream"/> to get the backing storage of.</param>
        /// <param name="start">The starting index of the slice.</param>
        /// <returns>A <see cref="Span{T}"/> over the backing storage of the specified <see cref="MemoryStream"/>.</returns>
        /// <remarks>
        /// This should be treated with the same care as a <see cref="Span{T}"/> returned from <see cref="CollectionsMarshal.AsSpan{T}(List{T}?)"/>.
        /// Do not use the <see cref="MemoryStream"/> while the <see cref="Span{T}"/> is in use.
        /// </remarks>
        public Span<byte> AsSpan(Index start) => stream._buffer().AsSpan(start);
        /// <summary>
        /// Gets a <see cref="Span{T}"/> over a section of the backing storage of the specified <see cref="MemoryStream"/>.
        /// </summary>
        /// <param name="stream">The <see cref="MemoryStream"/> to get the backing storage of.</param>
        /// <param name="start">The starting index of the slice.</param>
        /// <returns>A <see cref="Span{T}"/> over the backing storage of the specified <see cref="MemoryStream"/>.</returns>
        /// <remarks>
        /// This should be treated with the same care as a <see cref="Span{T}"/> returned from <see cref="CollectionsMarshal.AsSpan{T}(List{T}?)"/>.
        /// Do not use the <see cref="MemoryStream"/> while the <see cref="Span{T}"/> is in use.
        /// </remarks>
        public Span<byte> AsSpan(int start) => stream._buffer().AsSpan(start);
        /// <summary>
        /// Gets a <see cref="Span{T}"/> over a section of the backing storage of the specified <see cref="MemoryStream"/>.
        /// </summary>
        /// <param name="stream">The <see cref="MemoryStream"/> to get the backing storage of.</param>
        /// <param name="range">The range in the backing storage to get a <see cref="Span{T}"/> over.</param>
        /// <returns>A <see cref="Span{T}"/> over the backing storage of the specified <see cref="MemoryStream"/>.</returns>
        /// <remarks>
        /// This should be treated with the same care as a <see cref="Span{T}"/> returned from <see cref="CollectionsMarshal.AsSpan{T}(List{T}?)"/>.
        /// Do not use the <see cref="MemoryStream"/> while the <see cref="Span{T}"/> is in use.
        /// </remarks>
        public Span<byte> AsSpan(Range range) => stream._buffer().AsSpan(range);
        /// <summary>
        /// Gets a <see cref="Span{T}"/> over a section of the backing storage of the specified <see cref="MemoryStream"/>.
        /// </summary>
        /// <param name="stream">The <see cref="MemoryStream"/> to get the backing storage of.</param>
        /// <param name="start">The starting index of the slice.</param>
        /// <param name="length">The length of the slice.</param>
        /// <returns>A <see cref="Span{T}"/> over the backing storage of the specified <see cref="MemoryStream"/>.</returns>
        /// <remarks>
        /// This should be treated with the same care as a <see cref="Span{T}"/> returned from <see cref="CollectionsMarshal.AsSpan{T}(List{T}?)"/>.
        /// Do not use the <see cref="MemoryStream"/> while the <see cref="Span{T}"/> is in use.
        /// </remarks>
        public Span<byte> AsSpan(int start, int length) => stream._buffer().AsSpan(start, length);
        /// <summary>
        /// Gets a <see cref="Memory{T}"/> over the backing storage of the specified <see cref="MemoryStream"/>.
        /// </summary>
        /// <param name="stream">The <see cref="MemoryStream"/> to get the backing storage of.</param>
        /// <returns>A <see cref="Memory{T}"/> over the backing storage of the specified <see cref="MemoryStream"/>.</returns>
        /// <remarks>
        /// This should be treated with the same care as a <see cref="Span{T}"/> returned from <see cref="CollectionsMarshal.AsSpan{T}(List{T}?)"/>.
        /// Do not use the <see cref="MemoryStream"/> while the <see cref="Memory{T}"/> is in use.
        /// </remarks>
        public Memory<byte> AsMemory() => stream._buffer();
        /// <summary>
        /// Gets a <see cref="Memory{T}"/> over a section of the backing storage of the specified <see cref="MemoryStream"/>.
        /// </summary>
        /// <param name="stream">The <see cref="MemoryStream"/> to get the backing storage of.</param>
        /// <param name="start">The starting index of the slice.</param>
        /// <returns>A <see cref="Memory{T}"/> over the backing storage of the specified <see cref="MemoryStream"/>.</returns>
        /// <remarks>
        /// This should be treated with the same care as a <see cref="Span{T}"/> returned from <see cref="CollectionsMarshal.AsSpan{T}(List{T}?)"/>.
        /// Do not use the <see cref="MemoryStream"/> while the <see cref="Memory{T}"/> is in use.
        /// </remarks>
        public Memory<byte> AsMemory(Index start) => stream._buffer().AsMemory(start);
        /// <summary>
        /// Gets a <see cref="Memory{T}"/> over a section of the backing storage of the specified <see cref="MemoryStream"/>.
        /// </summary>
        /// <param name="stream">The <see cref="MemoryStream"/> to get the backing storage of.</param>
        /// <param name="start">The starting index of the slice.</param>
        /// <returns>A <see cref="Memory{T}"/> over the backing storage of the specified <see cref="MemoryStream"/>.</returns>
        /// <remarks>
        /// This should be treated with the same care as a <see cref="Span{T}"/> returned from <see cref="CollectionsMarshal.AsSpan{T}(List{T}?)"/>.
        /// Do not use the <see cref="MemoryStream"/> while the <see cref="Memory{T}"/> is in use.
        /// </remarks>
        public Memory<byte> AsMemory(int start) => stream._buffer().AsMemory(start);
        /// <summary>
        /// Gets a <see cref="Memory{T}"/> over a section of the backing storage of the specified <see cref="MemoryStream"/>.
        /// </summary>
        /// <param name="stream">The <see cref="MemoryStream"/> to get the backing storage of.</param>
        /// <param name="range">The range in the backing storage to get a <see cref="Memory{T}"/> over.</param>
        /// <returns>A <see cref="Memory{T}"/> over the backing storage of the specified <see cref="MemoryStream"/>.</returns>
        /// <remarks>
        /// This should be treated with the same care as a <see cref="Span{T}"/> returned from <see cref="CollectionsMarshal.AsSpan{T}(List{T}?)"/>.
        /// Do not use the <see cref="MemoryStream"/> while the <see cref="Memory{T}"/> is in use.
        /// </remarks>
        public Memory<byte> AsMemory(Range range) => stream._buffer().AsMemory(range);
        /// <summary>
        /// Gets a <see cref="Memory{T}"/> over a section of the backing storage of the specified <see cref="MemoryStream"/>.
        /// </summary>
        /// <param name="stream">The <see cref="MemoryStream"/> to get the backing storage of.</param>
        /// <param name="start">The starting index of the slice.</param>
        /// <param name="length">The length of the slice.</param>
        /// <returns>A <see cref="Memory{T}"/> over the backing storage of the specified <see cref="MemoryStream"/>.</returns>
        /// <remarks>
        /// This should be treated with the same care as a <see cref="Span{T}"/> returned from <see cref="CollectionsMarshal.AsSpan{T}(List{T}?)"/>.
        /// Do not use the <see cref="MemoryStream"/> while the <see cref="Memory{T}"/> is in use.
        /// </remarks>
        public Memory<byte> AsMemory(int start, int length) => stream._buffer().AsMemory(start, length);

        /// <summary>
        /// Creates and returns an exact copy of this <see cref="MemoryStream"/>; its backing store references the same byte array as the original stream.
        /// It is, of course, capable of maintaining its own position and length within that array.
        /// Concurrent reads on the two streams are safe, but writing should be synchronized if it cannot be guaranteed that the segments being written do not overlap.
        /// Using a <see cref="ReaderWriterLockSlim"/> to manage this is recommended if writing is necessary.
        /// <para/>Note that the two streams will lose synchronization if write operations cause the backing array to be resized.
        /// </summary>
        /// <param name="stream">The <see cref="MemoryStream"/> to duplicate.</param>
        /// <returns>A new <see cref="MemoryStream"/> as described.</returns>
        private MemoryStream Duplicate()
        {
            var newMs = new MemoryStream();
            newMs._buffer() = stream._buffer();
            newMs._capacity() = stream._capacity();
            newMs._length() = stream._length();
            newMs._position() = stream._position();
            return newMs;
        }
        /// <summary>
        /// Reads a block of bytes from the current stream and writes the data to a given span.
        /// </summary>
        /// <param name="stream">The <see cref="MemoryStream"/> to read from.</param>
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
        /// <summary>
        /// Reads a block of bytes from the current stream and writes the data to a given span.
        /// </summary>
        /// <param name="stream">The <see cref="MemoryStream"/> to read from.</param>
        /// <param name="position">The byte offset in the stream at which to begin reading.</param>
        /// <param name="length">The number of bytes to read.</param>
        /// <param name="destination">The <see cref="Span{T}"/> to write the data to.</param>
        /// <param name="destPosition">The byte offset in the destination span at which to begin writing.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="position"/> is less than zero or <paramref name="position"/> + <paramref name="length"/> is greater than the length of the stream.</exception>
        /// <exception cref="ArgumentException">Thrown if the destination span is too short to contain the requested number of bytes.</exception>
        public void CopyBlock(int position, int length, Span<byte> destination, int destPosition)
        {
            if (position < 0 || position + length > stream.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(position));
            }
            if (destPosition < 0 || destPosition + length > destination.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(destPosition));
            }
            if (destination[destPosition..].Length < length)
            {
                throw new ArgumentException("The destination span is too short to contain the requested number of bytes.", nameof(destination));
            }
            stream.AsSpan().Slice(position, length).CopyTo(destination[destPosition..]);
        }
    }
}
