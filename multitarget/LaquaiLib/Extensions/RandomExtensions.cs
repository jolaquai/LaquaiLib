using System.Buffers;

namespace LaquaiLib.Extensions;

#pragma warning disable CA5394 // Do not use insecure randomness

/// <summary>
/// Provides extension methods for the <see cref="Random"/> type.
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
            {
                throw new ArgumentException("The stream must be writable.", nameof(destination));
            }

            scoped Span<byte> span;

            if (destination is MemoryStream ms)
            {
                // Since a MemoryStream is resizable, calculate the next-greater power of 2 and resize it to that
                var newSize = ms.Length + count;
                if (newSize > ms.Capacity)
                {
                    // Start at a minimum of 1: a default MemoryStream has Capacity 0, and 0 << 1 == 0
                    // would loop forever.
                    var newCapacity = Math.Max(ms.Capacity, 1);
                    while (newCapacity < newSize)
                    {
                        newCapacity <<= 1;
                    }
                    ms.Capacity = newCapacity;
                }
                ms.SetLength(newSize);

                span = ms.AsSpan((int)ms.Position, count);
                random.NextBytes(span);
                ms.Position += count;
                return;
            }

            // Otherwise we have little choice but to read into a buffer and write it to the stream
            byte[] buffer = null;
            if (count <= Config.MaxStackallocSize)
                span = stackalloc byte[count];
            else
            {
                buffer = ArrayPool<byte>.Shared.Rent(count);
                span = buffer.AsSpan(0, count);
            }
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

            // This branch actually remains synchronous
            if (destination is MemoryStream ms)
            {
                var newSize = ms.Length + count;
                if (newSize > Array.MaxLength)
                    throw new ArgumentOutOfRangeException(nameof(count), "The resulting size of the MemoryStream exceeds the maximum allowed size.");

                if (newSize > ms.Capacity)
                    ms.Capacity = (int)newSize;
                ms.SetLength(newSize);
                var span = ms.AsSpan((int)ms.Position, count);
                random.NextBytes(span);
                ms.Position += count;
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
    }
}
