namespace LaquaiLib.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="Stream"/> Type.
/// </summary>
public static class StreamExtensions
{
    /// <summary>
    /// Reads all bytes from the current position to the end of the <see cref="Stream"/> and advances the position within it to the end.
    /// </summary>
    /// <param name="stream">The <see cref="Stream"/> to read from.</param>
    /// <returns>The bytes of the rest of the <see cref="Stream"/>, from its current position to the end.</returns>
    public static byte[] ReadToEnd(this Stream stream)
    {
        var buffer = new byte[stream.Length - stream.Position];
        stream.Read(buffer);
        return buffer;
    }

    /// <summary>
    /// Reads all bytes from the current position to the end of the <see cref="Stream"/> into the specified <paramref name="span"/> and advances the position within it to the end.
    /// </summary>
    /// <param name="stream">The <see cref="Stream"/> to read from.</param>
    /// <param name="span">The <see cref="Span{T}"/> of <see cref="byte"/> to read into.</param>
    public static void ReadToEnd(this Stream stream, Span<byte> span)
    {
        var requiredSpace = stream.Length - stream.Position;
        if (span.Length < requiredSpace)
        {
            throw new ArgumentException($"The provided {nameof(Span<byte>)} is too small to hold the rest of the stream (can only accommodate {span.Length}/{requiredSpace} bytes).");
        }
        stream.Read(span);
    }

    /// <summary>
    /// Reads all bytes from the current position to the end of the <see cref="Stream"/> asynchronously, optionally monitoring a <paramref name="cancellationToken"/> for cancellation requests, and advances the position within it to the end.
    /// </summary>
    /// <param name="stream">The <see cref="Stream"/> to read from.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests.</param>
    /// <returns>A <see cref="Task{T}"/> that represents the asynchronous read operation and proxies for the read bytes.</returns>
    public static async Task<byte[]> ReadToEndAsync(this Stream stream, CancellationToken cancellationToken = default)
    {
        var buffer = new byte[stream.Length - stream.Position];
        await stream.ReadAsync(buffer, cancellationToken);
        return buffer;
    }
}
