
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
    /// Fills the specified <paramref name="destination"/> <see cref="Stream"/> with as many <see langword="byte"/>s as will fit.
    /// </summary>
    /// <param name="destination">The <see cref="Stream"/> to fill with random bytes.</param>
    /// <param name="bufferSize">The size of the buffer to use for copying.</param>
    public override void CopyTo(Stream destination, int bufferSize)
    {
        using (var buffer = new TempArray<byte>(int.Min(bufferSize, (int)(destination.Length - destination.Position))))
        {
            while (destination.Length - destination.Position >= bufferSize)
            {
                Read(buffer.Array);
                destination.Write(buffer.Array);
            }
            var remaining = (int)(destination.Length - destination.Position);
            if (remaining > 0)
            {
                var span = buffer.Array.AsSpan(0, remaining);
                Read(span);
                destination.Write(span);
            }
        }
    }
    /// <summary>
    /// Asynchronously fills the specified <paramref name="destination"/> <see cref="Stream"/> with as many <see langword="byte"/>s as will fit.
    /// </summary>
    /// <param name="destination">The <see cref="Stream"/> to fill with random bytes.</param>
    /// <param name="bufferSize">The size of the buffer to use for copying.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests.</param>
    /// <returns></returns>
    public override async Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
    {
        using (var buffer = new TempArray<byte>(int.Min(bufferSize, (int)(destination.Length - destination.Position))))
        {
            cancellationToken.ThrowIfCancellationRequested();
            while (destination.Length - destination.Position >= bufferSize)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await ReadAsync(buffer.Array, cancellationToken).ConfigureAwait(false);
                cancellationToken.ThrowIfCancellationRequested();
                await destination.WriteAsync(buffer.Array, cancellationToken).ConfigureAwait(false);
            }
            cancellationToken.ThrowIfCancellationRequested();
            var remaining = (int)(destination.Length - destination.Position);
            if (remaining > 0)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await ReadAsync(buffer.Array, 0, remaining, cancellationToken).ConfigureAwait(false);
                cancellationToken.ThrowIfCancellationRequested();
                await destination.WriteAsync(buffer.Array, 0, remaining, cancellationToken).ConfigureAwait(false);
            }
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