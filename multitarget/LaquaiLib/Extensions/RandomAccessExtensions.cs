namespace LaquaiLib.Extensions;

public static class RandomAccessExtensions
{
    extension(RandomAccess)
    {
        /// <summary>
        /// Reads a sequence of bytes from the specified <see cref="MemoryStream"/> into the provided buffer, starting at the given offset.
        /// </summary>
        /// <param name="ms">The <see cref="MemoryStream"/> to read from.</param>
        /// <param name="buffer">The <see cref="Span{T}"/> of <see langword="byte"/> to read the bytes into.</param>
        /// <param name="offset">The zero-based byte offset in the <see cref="MemoryStream"/> at which to begin reading.</param>
        /// <returns>The total number of bytes read into the buffer.</returns>
        public static int Read(MemoryStream ms, Span<byte> buffer, int offset)
        {
            var len = buffer.Length;
            var src = ms.AsSpan(offset);
            var read = Math.Min(len, src.Length);
            src[..read].CopyTo(buffer);
            return read;
        }
        /// <summary>
        /// Reads a sequence of bytes from the specified <see cref="MemoryStream"/> into the provided list of buffers, starting at the given offset.
        /// </summary>
        /// <param name="ms">The <see cref="MemoryStream"/> to read from.</param>
        /// <param name="buffers">An <see cref="IReadOnlyList{T}"/> of <see cref="Memory{T}"/> of <see langword="byte"/> to read the bytes into.</param>
        /// <param name="offset">The zero-based byte offset in the <see cref="MemoryStream"/> at which to begin reading.</param>
        /// <returns>The total number of bytes read into the buffers.</returns>
        public static int Read(MemoryStream ms, IReadOnlyList<Memory<byte>> buffers, int offset)
        {
            var src = ms.AsSpan(offset);
            var totalRead = 0;
            foreach (var buffer in buffers)
            {
                var read = Math.Min(buffer.Length, src.Length - totalRead);
                if (read <= 0)
                    break;
                src.Slice(totalRead, read).CopyTo(buffer.Span);
                totalRead += read;
            }
            return totalRead;
        }
        /// <summary>
        /// Writes a sequence of bytes from the provided buffer into the specified <see cref="MemoryStream"/>, starting at the given offset.
        /// </summary>
        /// <param name="ms">The <see cref="MemoryStream"/> to write to.</param>
        /// <param name="buffer">The <see cref="ReadOnlySpan{T}"/> of <see langword="byte"/> containing the bytes to write.</param>
        /// <param name="offset">The zero-based byte offset in the <see cref="MemoryStream"/> at which to begin writing.</param>
        public static void Write(MemoryStream ms, ReadOnlySpan<byte> buffer, int offset)
        {
            var len = buffer.Length;
            var dest = ms.AsSpan(offset);
            var write = Math.Min(len, dest.Length);
            buffer[..write].CopyTo(dest);
        }
        /// <summary>
        /// Writes a sequence of bytes from the provided list of buffers into the specified <see cref="MemoryStream"/>, starting at the given offset.
        /// </summary>
        /// <param name="ms">The <see cref="MemoryStream"/> to write to.</param>
        /// <param name="buffers">An <see cref="IReadOnlyList{T}"/> of <see cref="ReadOnlyMemory{T}"/> of <see langword="byte"/> containing the bytes to write.</param>
        /// <param name="offset">The zero-based byte offset in the <see cref="MemoryStream"/> at which to begin writing.</param>
        public static void Write(MemoryStream ms, IReadOnlyList<ReadOnlyMemory<byte>> buffers, int offset)
        {
            var dest = ms.AsSpan(offset);
            var totalWritten = 0;
            foreach (var buffer in buffers)
            {
                var write = Math.Min(buffer.Length, dest.Length - totalWritten);
                if (write <= 0)
                    break;
                buffer.Span[..write].CopyTo(dest.Slice(totalWritten, write));
                totalWritten += write;
            }
        }
    }
}
