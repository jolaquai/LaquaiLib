namespace LaquaiLib.IO.Streams;

/// <summary>
/// Contains factory methods that produce either <see cref="MemoryStream"/> or <see cref="FileStream"/> instances, depending on the size of the data expected to be written to it.
/// </summary>
public static class MemoryOrFileStream
{
    /// <summary>
    /// The number of bytes at which the stream will switch from a <see cref="MemoryStream"/> to a <see cref="FileStream"/>.
    /// </summary>
    /// <remarks>
    /// You may freely change this value at runtime. Its initial value is 64 MB.
    /// </remarks>
    public static int Cutoff { get; set; } = ResetCutoff();
    /// <summary>
    /// Resets the <see cref="Cutoff"/> to the initial value.
    /// </summary>
    /// <returns>The new value of <see cref="Cutoff"/>.</returns>
    public static int ResetCutoff() => Cutoff = 64 * 1024 * 1024; // 64 MB

    /// <summary>
    /// Creates a new <see cref="Stream"/> with the given expected payload size.
    /// </summary>
    /// <param name="payloadSize">The expected size of the payload to be written to this stream. If it exceeds <see cref="Cutoff"/>, a <see cref="FileStream"/> wrapping a temporary file is created (which is deleted on call of <see cref="Stream.Dispose()"/>. Otherwise, a <see cref="MemoryStream"/> is created.</param>
    /// <returns>The created <see cref="Stream"/>.</returns>
    public static Stream Create(int payloadSize)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(payloadSize);
        return Create(payloadSize, Cutoff);
    }

    /// <summary>
    /// Creates a new <see cref="Stream"/> with the given expected payload size and cutoff. <see cref="Cutoff"/> is ignored.
    /// </summary>
    /// <param name="payloadSize">The expected size of the payload to be written to this stream. If it exceeds <paramref name="cutoff"/>, a <see cref="FileStream"/> wrapping a temporary file is created (which is deleted on call of <see cref="Stream.Dispose()"/>. Otherwise, a <see cref="MemoryStream"/> is created.</param>
    /// <returns>The created <see cref="Stream"/>.</returns>
    /// <param name="cutoff">The cutoff at which to switch from <see cref="MemoryStream"/> to <see cref="FileStream"/>.</param>
    public static Stream Create(int payloadSize, int cutoff)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(payloadSize);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(cutoff);

        return payloadSize >= cutoff
            ? new FileStream(Path.GetTempFileName(), FileMode.Create, FileAccess.ReadWrite, FileShare.Read, 4096, FileOptions.Asynchronous | FileOptions.DeleteOnClose)
            : new MemoryStream(payloadSize);
    }
}
