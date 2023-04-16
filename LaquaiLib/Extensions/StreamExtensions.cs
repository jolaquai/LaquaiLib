using System.IO;

namespace LaquaiLib.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="Stream"/> Type.
/// </summary>
public static class StreamExtensions
{
    /// <summary>
    /// Reads all characters from the current position to the end of the stream.
    /// </summary>
    /// <returns>The rest of the stream as a String, from the current position to the end.</returns>
    public static string ReadToEnd(this Stream stream)
    {
        using StreamReader sr = new(stream);
        return sr.ReadToEnd();
    }

    /// <summary>
    /// Reads all characters from the current position to the end of the stream asynchronously and returns them as one string.
    /// </summary>
    /// <returns>A task that represents the asynchronous read operation.</returns>
    public static async Task<string> ReadToEndAsync(this Stream stream)
    {
        using StreamReader sr = new(stream);
        return await sr.ReadToEndAsync();
    }

    /// <summary>
    /// Reads all characters from the current position to the end of the stream asynchronously and returns them as one string.
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous read operation.</returns>
    public static async Task<string> ReadToEndAsync(this Stream stream, CancellationToken cancellationToken)
    {
        using StreamReader sr = new(stream);
        return await sr.ReadToEndAsync(cancellationToken);
    }
}
