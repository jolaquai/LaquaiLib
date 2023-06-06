using System.IO;

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
