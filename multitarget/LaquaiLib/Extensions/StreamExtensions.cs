namespace LaquaiLib.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="Stream"/> Type.
/// </summary>
public static partial class StreamExtensions
{
    extension(Stream ms)
    {
        /// <summary>
        /// Reads all bytes from the current position to the end of the <see cref="Stream"/> and advances the position within it to the end.
        /// </summary>
        /// <returns>The bytes of the rest of the <see cref="Stream"/>, from its current position to the end.</returns>
        public byte[] ReadToEnd()
        {
            var buffer = new byte[ms.Length - ms.Position];
            ms.ReadExactly(buffer);
            return buffer;
        }
        /// <summary>
        /// Reads the entire contents of the <paramref name="stream"/> into a <see langword="byte"/> array, regardless of current position.
        /// The <paramref name="stream"/> remains sought to its end.
        /// </summary>
        /// <returns>The created <see langword="byte"/> array.</returns>
        public byte[] ToArray()
        {
            var buffer = new byte[ms.Length];
            ms.Position = 0;
            ms.ReadExactly(buffer);
            return buffer;
        }
        /// <summary>
        /// Reads all bytes from the current position to the end of the <see cref="Stream"/> into the specified <paramref name="span"/> and advances the position within it to the end.
        /// </summary>
        /// <param name="span">A <see cref="Span{T}"/> of <see cref="byte"/> to read into.</param>
        public void ReadToEnd(Span<byte> span)
        {
            var requiredSpace = ms.Length - ms.Position;
            if (span.Length < requiredSpace)
            {
                throw new ArgumentException($"The provided {nameof(Span<>)} is too small to hold the rest of the stream (can only accommodate {span.Length}/{requiredSpace} bytes).");
            }
            ms.ReadExactly(span);
        }
        /// <summary>
        /// Asynchronously reads all bytes from the current position to the end of the <see cref="Stream"/>, optionally monitoring a <paramref name="cancellationToken"/> for cancellation requests, and advances the position within it to the end.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to monitor for cancellation requests.</param>
        /// <returns>A <see cref="Task{TResult}"/> that represents the asynchronous read operation and resolves to the bytes read.</returns>
        public async Task<byte[]> ReadToEndAsync(CancellationToken cancellationToken = default)
        {
            var buffer = new byte[ms.Length - ms.Position];
            await ms.ReadExactlyAsync(buffer, cancellationToken).ConfigureAwait(false);
            return buffer;
        }
        /// <summary>
        /// Asynchronously reads all bytes from the current position to the end of the <see cref="Stream"/> into the specified <paramref name="memory"/> and advances the position within it to the end.
        /// </summary>
        /// <param name="memory">A <see cref="Memory{T}"/> of <see cref="byte"/> to read into.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to monitor for cancellation requests.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous read operation.</returns>
        public async Task ReadToEndAsync(Memory<byte> memory, CancellationToken cancellationToken = default)
        {
            var requiredSpace = ms.Length - ms.Position;
            if (memory.Length < requiredSpace)
            {
                throw new ArgumentException($"The provided {nameof(Memory<>)} is too small to hold the rest of the stream (can only accommodate {memory.Length}/{requiredSpace} bytes).");
            }
            await ms.ReadExactlyAsync(memory, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Reads as many <see langword="byte"/>s from the specified <paramref name="stream"/> as will fit into <paramref name="span"/>, or less than that if the <paramref name="stream"/> has fewer bytes left before the end.
        /// </summary>
        /// <param name="span">The <see cref="Span{T}"/> to read into.</param>
        public void ReadFill(Span<byte> span)
        {
            var bytesLeft = Math.Min(span.Length, ms.Length - ms.Position);
            if (bytesLeft < span.Length)
            {
                span = span[..(int)bytesLeft];
            }
            ms.ReadExactly(span);
        }
        /// <summary>
        /// Asynchronously reads data from a stream into a specified memory buffer.
        /// </summary>
        /// <param name="memory">The buffer that receives the read bytes from the stream.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to monitor for cancellation requests.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous read operation.</returns>
        public async Task ReadFillAsync(Memory<byte> memory, CancellationToken cancellationToken = default)
        {
            var bytesLeft = Math.Min(memory.Length, ms.Length - ms.Position);
            if (bytesLeft < memory.Length)
            {
                memory = memory[..(int)bytesLeft];
            }
            await ms.ReadExactlyAsync(memory, cancellationToken).ConfigureAwait(false);
        }
    }
}
