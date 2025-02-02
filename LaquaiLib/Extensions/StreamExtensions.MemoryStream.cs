using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace LaquaiLib.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="Stream"/> Type.
/// </summary>
public static partial class StreamExtensions
{
    [UnsafeAccessor(UnsafeAccessorKind.Field)]
    private static extern ref byte[] _buffer(this MemoryStream ms);
    [UnsafeAccessor(UnsafeAccessorKind.Field)]
    private static extern ref int _capacity(this MemoryStream ms);
    [UnsafeAccessor(UnsafeAccessorKind.Field)]
    private static extern ref int _length(this MemoryStream ms);
    [UnsafeAccessor(UnsafeAccessorKind.Field)]
    private static extern ref int _position(this MemoryStream ms);

    /// <summary>
    /// Gets a <see cref="Span{T}"/> over the backing storage of the specified <see cref="MemoryStream"/>.
    /// </summary>
    /// <param name="stream">The <see cref="MemoryStream"/> to get the backing storage of.</param>
    /// <returns>A <see cref="Span{T}"/> over the backing storage of the specified <see cref="MemoryStream"/>.</returns>
    /// <remarks>
    /// This should be treated with the same care as a <see cref="Span{T}"/> returned from <see cref="CollectionsMarshal.AsSpan{T}(List{T}?)"/>.
    /// Do not use the <see cref="MemoryStream"/> while the <see cref="Span{T}"/> is in use.
    /// </remarks>
    // Obviously don't ever ref-return this
    public static Span<byte> AsSpan(this MemoryStream stream) => stream._buffer();
    /// <summary>
    /// Creates and returns an exact copy of this <see cref="MemoryStream"/>; its backing store references the same byte array as the original stream.
    /// It is, of course, capable of maintaining its own position and length within that array.
    /// Concurrent reads on the two streams are safe, but writing should be synchronized if it cannot be guaranteed that the segments being written do not overlap.
    /// Using a <see cref="ReaderWriterLockSlim"/> to manage this is recommended if writing is necessary.
    /// <para/>Note that the two streams will lose synchronization if write operations cause the backing array to be resized.
    /// </summary>
    /// <param name="stream">The <see cref="MemoryStream"/> to duplicate.</param>
    /// <returns>A new <see cref="MemoryStream"/> as described.</returns>
    private static MemoryStream Duplicate(this MemoryStream stream)
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
    public static void CopyBlock(this MemoryStream stream, int position, int length, Span<byte> destination)
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
    public static void CopyBlock(this MemoryStream stream, int position, int length, Span<byte> destination, int destPosition)
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
