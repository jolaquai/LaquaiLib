
using LaquaiLib.Wrappers;

using System.Security.Cryptography;

namespace LaquaiLib.Streams.RandomStreams;

/// <summary>
/// Implements <see cref="RandomStream"/> with a cryptographic random number generator.
/// </summary>
public class CryptographicRandomStream : RandomStream
{
    private readonly RandomNumberGenerator rng;
    /// <summary>
    /// Initializes a new <see cref="CryptographicRandomStream"/>.
    /// </summary>
    public CryptographicRandomStream()
    {
        random = null!;
        rng = RandomNumberGenerator.Create();
    }

    public override int Read(byte[] buffer, int offset, int count) => Read(buffer.AsSpan(offset, count));
    /// <summary>
    /// Fills the specified buffer with random bytes.
    /// </summary>
    /// <param name="buffer">The buffer to fill with random bytes.</param>
    /// <returns>The number of bytes written, which is always equal to the length of the buffer.</returns>
    public override int Read(Span<byte> buffer)
    {
        rng.GetBytes(buffer);
        return buffer.Length;
    }
    /// <summary>
    /// Asynchronously fills the specified buffer with random bytes.
    /// </summary>
    /// <param name="buffer">The buffer to fill with random bytes.</param>
    /// <param name="offset">The offset in the buffer at which to start writing.</param>
    /// <param name="count">The number of bytes to write.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests.</param>
    /// <returns>The number of bytes written, which is always equal to <paramref name="count"/>.</returns>
    public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) => await ReadAsync(buffer.AsMemory(offset, count), cancellationToken).ConfigureAwait(false);
    /// <summary>
    /// Asynchronously fills the specified buffer with random bytes.
    /// </summary>
    /// <param name="buffer">The buffer to fill with random bytes.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests.</param>
    /// <returns>The number of bytes written, which is always equal to the length of the buffer.</returns>
    public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        await Task.Run(() => rng.GetBytes(buffer.Span), cancellationToken).ConfigureAwait(false);
        return buffer.Length;
    }

    /// <summary>
    /// Fills the specified <paramref name="destination"/> <see cref="Stream"/> with as many <see langword="byte"/>s as will fit.
    /// </summary>
    /// <param name="destination">The <see cref="Stream"/> to fill with random bytes.</param>
    public new void CopyTo(Stream destination)
    {
        var exactly = (int)(destination.Length - destination.Position);
        using (var buffer = new TempArray<byte>(exactly))
        {
            Read(buffer.Array, 0, exactly);
            destination.Write(buffer.Array, 0, exactly);
        }
    }
    /// <summary>
    /// Writes <paramref name="byteCount"/> random bytes to the specified <paramref name="destination"/> <see cref="Stream"/>.
    /// </summary>
    /// <param name="destination">The <see cref="Stream"/> to write to.</param>
    /// <param name="byteCount">How many random bytes to write to <paramref name="destination"/>.</param>
    public override void CopyTo(Stream destination, int byteCount)
    {
        using (var buffer = new TempArray<byte>(byteCount))
        {
            Read(buffer.Array);
            destination.Write(buffer.Array, 0, byteCount);
        }
    }
    /// <summary>
    /// Asynchronously writes <paramref name="byteCount"/> random bytes to the specified <paramref name="destination"/> <see cref="Stream"/>.
    /// </summary>
    /// <param name="destination">The <see cref="Stream"/> to write to.</param>
    /// <param name="byteCount">How many random bytes to write to <paramref name="destination"/>.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests.</param>
    public override async Task CopyToAsync(Stream destination, int byteCount, CancellationToken cancellationToken)
    {
        using (var buffer = new TempArray<byte>(int.Min(byteCount, (int)(destination.Length - destination.Position))))
        {
            cancellationToken.ThrowIfCancellationRequested();
            await ReadAsync(buffer.Array, cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            await destination.WriteAsync(buffer.Array, cancellationToken).ConfigureAwait(false);
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            rng?.Dispose();
        }

        base.Dispose(disposing);
    }
}