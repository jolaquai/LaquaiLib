using System.Buffers;

using DocumentFormat.OpenXml.Bibliography;

namespace LaquaiLib.Extensions;

#pragma warning disable CA5394 // Do not use insecure randomness

/// <summary>
/// Provides extensions for the <see cref="Random"/> type.
/// </summary>
public static class RandomExtensions
{
    extension(Random random)
    {
        /// <summary>
        /// Writes <paramref name="count"/> random <see langword="byte"/>s to the specified <paramref name="destination"/> <see cref="Stream"/>.
        /// </summary>
        /// <param name="destination">The <see cref="Stream"/> to write to.</param>
        /// <param name="count">The number of <see langword="byte"/>s to write.</param>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="destination"/> <see cref="Stream"/> is not writable.</exception>
        public void NextBytes(Stream destination, int count)
        {
            if (!destination.CanWrite)
                throw new ArgumentException("The stream must be writable.", nameof(destination));

            scoped Span<byte> span;

            if (destination is MemoryStream ms)
            {
                WriteMemoryStreamCore(ms, count, random);
                return;
            }

            // Otherwise we have little choice but to read into a buffer and write it to the stream
            byte[] buffer = null;
            span = count <= Config.MaxStackallocSize ? span = stackalloc byte[count] : (buffer = ArrayPool<byte>.Shared.Rent(count)).AsSpan(0, count);
            try
            {
                random.NextBytes(span);
                destination.Write(span);
            }
            finally
            {
                if (buffer != null)
                    ArrayPool<byte>.Shared.Return(buffer);
            }
        }
        /// <summary>
        /// Asynchronously writes <paramref name="count"/> random <see langword="byte"/>s to the specified <paramref name="destination"/> <see cref="Stream"/>.
        /// </summary>
        /// <param name="destination">The <see cref="Stream"/> to write to.</param>
        /// <param name="count">The number of <see langword="byte"/>s to write.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async ValueTask NextBytesAsync(Stream destination, int count)
        {
            if (!destination.CanWrite)
                throw new ArgumentException("The stream must be writable.", nameof(destination));

            if (destination is MemoryStream ms)
            {
                WriteMemoryStreamCore(ms, count, random);
                return;
            }

            var buffer = ArrayPool<byte>.Shared.Rent(count);
            var mem = buffer.AsMemory(0, count);
            try
            {
                random.NextBytes(mem.Span);
                await destination.WriteAsync(mem).ConfigureAwait(false);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }
        private static void WriteMemoryStreamCore(MemoryStream ms, int count, Random rand)
        {
            var newSize = ms.Position + count;
            if (newSize > Array.MaxLength)
                throw new ArgumentOutOfRangeException(nameof(count), "The resulting size of the MemoryStream exceeds the maximum allowed size.");

            if (newSize > ms.Capacity)
            {
                if (!MemoryStreamAccessors._expandable(ms))
                    throw new OutOfMemoryException("The MemoryStream is not expandable and cannot accommodate the requested number of bytes.");
                else
                    ms.Capacity = (int)newSize;
            }

            ms.SetLength(newSize);

            var span = ms.AsSpan((int)ms.Position, count);
            rand.NextBytes(span);
            ms.Position += count;
        }
    }
}
