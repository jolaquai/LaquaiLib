namespace LaquaiLib.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="Stream"/> Type.
/// </summary>
public static partial class StreamExtensions
{
    /// <summary>
    /// Reads all bytes from the current position to the end of the <see cref="Stream"/> and advances the position within it to the end.
    /// </summary>
    /// <param name="stream">The <see cref="Stream"/> to read from.</param>
    /// <returns>The bytes of the rest of the <see cref="Stream"/>, from its current position to the end.</returns>
    public static byte[] ReadToEnd(this Stream stream)
    {
        var buffer = new byte[stream.Length - stream.Position];
        stream.ReadExactly(buffer);
        return buffer;
    }
    /// <summary>
    /// Reads the entire contents of the <paramref name="stream"/> into a <see langword="byte"/> array, regardless of current position.
    /// The <paramref name="stream"/> remains sought to its end.
    /// </summary>
    /// <param name="stream">The <see cref="Stream"/> to read from.</param>
    /// <returns>The created <see langword="byte"/> array.</returns>
    public static byte[] ToArray(this Stream stream)
    {
        var buffer = new byte[stream.Length];
        stream.Position = 0;
        stream.ReadExactly(buffer);
        return buffer;
    }
    /// <summary>
    /// Reads all bytes from the current position to the end of the <see cref="Stream"/> into the specified <paramref name="span"/> and advances the position within it to the end.
    /// </summary>
    /// <param name="stream">The <see cref="Stream"/> to read from.</param>
    /// <param name="span">A <see cref="Span{T}"/> of <see cref="byte"/> to read into.</param>
    public static void ReadToEnd(this Stream stream, Span<byte> span)
    {
        var requiredSpace = stream.Length - stream.Position;
        if (span.Length < requiredSpace)
        {
            throw new ArgumentException($"The provided {nameof(Span<>)} is too small to hold the rest of the stream (can only accommodate {span.Length}/{requiredSpace} bytes).");
        }
        stream.ReadExactly(span);
    }
    /// <summary>
    /// Reads as many <see langword="byte"/>s from the specified <paramref name="stream"/> as will fit into <paramref name="span"/>, or less than that if the <paramref name="stream"/> has fewer bytes left before the end.
    /// </summary>
    /// <param name="stream">The <see cref="Stream"/> to read from.</param>
    /// <param name="span">The <see cref="Span{T}"/> to read into.</param>
    public static void ReadFill(this Stream stream, Span<byte> span)
    {
        var bytesLeft = Math.Min(span.Length, stream.Length - stream.Position);
        if (bytesLeft < span.Length)
        {
            span = span[..(int)bytesLeft];
        }
        stream.ReadExactly(span);
    }
    /// <summary>
    /// Reads all bytes from the current position to the end of the <see cref="Stream"/> asynchronously, optionally monitoring a <paramref name="cancellationToken"/> for cancellation requests, and advances the position within it to the end.
    /// </summary>
    /// <param name="stream">The <see cref="Stream"/> to read from.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to monitor for cancellation requests.</param>
    /// <returns>A <see cref="Task{TResult}"/> that represents the asynchronous read operation and resolves to the bytes read.</returns>
    public static async Task<byte[]> ReadToEndAsync(this Stream stream, CancellationToken cancellationToken = default)
    {
        var buffer = new byte[stream.Length - stream.Position];
        await stream.ReadExactlyAsync(buffer, cancellationToken).ConfigureAwait(false);
        return buffer;
    }
    /// <summary>
    /// Reads all bytes from the current position to the end of the <see cref="Stream"/> into the specified <paramref name="memory"/> and advances the position within it to the end.
    /// </summary>
    /// <param name="stream">The <see cref="Stream"/> to read from.</param>
    /// <param name="memory">A <see cref="Memory{T}"/> of <see cref="byte"/> to read into.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to monitor for cancellation requests.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous read operation.</returns>
    public static async Task ReadToEndAsync(this Stream stream, Memory<byte> memory, CancellationToken cancellationToken = default)
    {
        var requiredSpace = stream.Length - stream.Position;
        if (memory.Length < requiredSpace)
        {
            throw new ArgumentException($"The provided {nameof(Memory<>)} is too small to hold the rest of the stream (can only accommodate {memory.Length}/{requiredSpace} bytes).");
        }
        await stream.ReadExactlyAsync(memory, cancellationToken).ConfigureAwait(false);
    }
}
