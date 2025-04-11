using System.Drawing;

using LaquaiLib.Core;
using LaquaiLib.Unsafe;

namespace LaquaiLib.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="Random"/> type.
/// </summary>
public static class RandomExtensions
{
    /// <summary>
    /// Writes <paramref name="count"/> random <see langword="byte"/>s to the specified <paramref name="destination"/> <see cref="Stream"/>.
    /// </summary>
    /// <param name="random">The <see cref="Random"/> instance to use.</param>
    /// <param name="destination">The <see cref="Stream"/> to write to.</param>
    /// <param name="count">The number of <see langword="byte"/>s to write.</param>
    /// <exception cref="ArgumentException">Thrown when the <paramref name="destination"/> <see cref="Stream"/> is not writable.</exception>
    public static void NextBytes(this Random random, Stream destination, int count)
    {
        if (!destination.CanWrite)
        {
            throw new ArgumentException("The stream must be writable.", nameof(destination));
        }

        if (destination is MemoryStream ms)
        {
            // Since a MemoryStream is resizable, calculate the next-greater power of 2 and resize it to that
            var newSize = ms.Length + count;
            if (newSize > ms.Capacity)
            {
                var newCapacity = ms.Capacity;
                while (newCapacity < newSize)
                {
                    newCapacity <<= 1;
                }
                ms.Capacity = newCapacity;
            }
            ms.SetLength(newSize);

            var span = ms.AsSpan((int)ms.Position, count);
            random.NextBytes(span);
            ms.Position += count;
            return;
        }

        // Otherwise we have little choice but to read into a buffer and write it to the stream
        Span<byte> buffer = count <= Configuration.MaxStackallocSize ? stackalloc byte[count] : new byte[count];
        random.NextBytes(buffer);
        destination.Write(buffer);
    }
    /// <summary>
    /// Asynchronously writes <paramref name="count"/> random <see langword="byte"/>s to the specified <paramref name="destination"/> <see cref="Stream"/>.
    /// </summary>
    /// <param name="random">The <see cref="Random"/> instance to use.</param>
    /// <param name="destination">The <see cref="Stream"/> to write to.</param>
    /// <param name="count">The number of <see langword="byte"/>s to write.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public static Task NextBytesAsync(this Random random, Stream destination, int count)
    {
        if (!destination.CanWrite)
        {
            throw new ArgumentException("The stream must be writable.", nameof(destination));
        }

        // This branch actually remains synchronous
        if (destination is MemoryStream ms)
        {
            var newSize = ms.Length + count;
            if (newSize > ms.Capacity)
            {
                var newCapacity = ms.Capacity;
                while (newCapacity < newSize)
                {
                    newCapacity <<= 1;
                }
                ms.Capacity = newCapacity;
            }
            ms.SetLength(newSize);
            var span = ms.AsSpan((int)ms.Position, count);
            random.NextBytes(span);
            ms.Position += count;
            return Task.CompletedTask;
        }

        var buffer = new byte[count];
        random.NextBytes(buffer);
        return destination.WriteAsync(buffer).AsTask();
    }
}
