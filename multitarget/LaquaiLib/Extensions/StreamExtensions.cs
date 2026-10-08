namespace LaquaiLib.Extensions;

/// <summary>
/// Provides extensions for the <see cref="Stream"/> type.
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
        /// Reads the entire contents of the <see cref="Stream"/> into a <see langword="byte"/> array, regardless of current position.
        /// The <see cref="Stream"/> remains sought to its end.
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
                throw new ArgumentException($"The provided {nameof(Span<>)} is too small to hold the rest of the stream (can only accommodate {span.Length}/{requiredSpace} bytes).");
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
                throw new ArgumentException($"The provided {nameof(Memory<>)} is too small to hold the rest of the stream (can only accommodate {memory.Length}/{requiredSpace} bytes).");
            await ms.ReadExactlyAsync(memory, cancellationToken).ConfigureAwait(false);
        }
    }
}
