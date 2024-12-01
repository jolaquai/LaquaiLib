using LaquaiLib.Wrappers;

// TODO: Make this generic and able to return arbitrary types, not just bytes

namespace LaquaiLib.Streams.RandomStreams;

/// <summary>
/// Represents a <see cref="Stream"/> that generates random bytes upon reading from it.
/// It comes in two variants: <see cref="RandomStream"/> for applications that do not require cryptographic security and <see cref="CryptographicRandomStream"/>.
/// </summary>
public class RandomStream : Stream
{
    private readonly Random random;

    /// <summary>
    /// Initializes a new <see cref="RandomStream"/>.
    /// </summary>
    public RandomStream()
    {
        random = new Random();
    }
    /// <summary>
    /// Initializes a new <see cref="RandomStream"/> with the specified seed.
    /// </summary>
    public RandomStream(int seed)
    {
        random = new Random(seed);
    }
    /// <summary>
    /// Initializes a new <see cref="RandomStream"/> with the specified <see cref="Random"/> instance.
    /// </summary>
    public RandomStream(Random random)
    {
        this.random = random;
    }

    /// <summary>
    /// Whether the <see cref="Stream"/> can be read from. This is always <see langword="true"/> for <see cref="RandomStream"/>.
    /// </summary>
    public override bool CanRead => true;
    /// <summary>
    /// Whether the <see cref="Stream"/> can be seeked. This is always <see langword="false"/> for <see cref="RandomStream"/> since seeking is ignored.
    /// </summary>
    public override bool CanSeek => false;
    /// <summary>
    /// Whether the <see cref="Stream"/> can be written to. This is irrelevant for <see cref="RandomStream"/> since writes are ignored.
    /// </summary>
    public override bool CanWrite => false;
    /// <summary>
    /// The length of the <see cref="Stream"/>. This is irrelevant for <see cref="RandomStream"/>. Its length will never change.
    /// </summary>
    public override long Length => long.MaxValue;
    /// <summary>
    /// The position in the <see cref="Stream"/>. This is irrelevant for <see cref="RandomStream"/>. Its position will never change.
    /// </summary>
    public override long Position { get; set; }

    /// <summary>
    /// Flushing is ignored for <see cref="RandomStream"/>.
    /// </summary>
    public override void Flush()
    {
    }
    /// <summary>
    /// Fills the specified buffer with random bytes.
    /// </summary>
    /// <param name="buffer">The buffer to fill with random bytes.</param>
    /// <param name="offset">The offset in the buffer at which to start writing.</param>
    /// <param name="count">The number of bytes to write.</param>
    /// <returns>The number of bytes written, which is always equal to <paramref name="count"/>.</returns>
    public override int Read(byte[] buffer, int offset, int count) => Read(buffer.AsSpan(offset, count));
    /// <summary>
    /// Fills the specified buffer with random bytes.
    /// </summary>
    /// <param name="buffer">The buffer to fill with random bytes.</param>
    /// <returns>The number of bytes written, which is always equal to the length of the buffer.</returns>
    public override int Read(Span<byte> buffer)
    {
        random.NextBytes(buffer);
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
        await Task.Run(() => random.NextBytes(buffer.Span), cancellationToken).ConfigureAwait(false);
        return buffer.Length;
    }
    /// <summary>
    /// Returns a random <see langword="byte"/> value.
    /// </summary>
    /// <returns>The random <see langword="byte"/> value.</returns>
    public override int ReadByte() => random.Next(byte.MinValue, byte.MaxValue + 1);
    /// <summary>
    /// Seek operations are ignored for <see cref="RandomStream"/>.
    /// </summary>
    /// <param name="offset">Ignored.</param>
    /// <param name="origin">Ignored.</param>
    /// <returns><c>0</c>.</returns>
    public override long Seek(long offset, SeekOrigin origin = SeekOrigin.Begin) => 0;
    /// <summary>
    /// The length of a <see cref="RandomStream"/> is irrelevant and operations controlling it are ignored.
    /// </summary>
    /// <param name="value">Ignored.</param>
    public override void SetLength(long value)
    {
    }
    /// <summary>
    /// Writes are ignored for <see cref="RandomStream"/>.
    /// </summary>
    /// <param name="buffer">Ignored.</param>
    /// <param name="offset">Ignored.</param>
    /// <param name="count">Ignored.</param>
    public override void Write(byte[] buffer, int offset, int count)
    {
    }

    /// <summary>
    /// Fills the specified <paramref name="destination"/> <see cref="Stream"/> with as many <see langword="byte"/>s as will fit.
    /// </summary>
    /// <param name="destination">The <see cref="Stream"/> to fill with random bytes.</param>
    /// <param name="bufferSize">The size of the buffer to use for copying.</param>
    public override void CopyTo(Stream destination, int bufferSize = 4069)
    {
        using (var buffer = new TempArray<byte>(int.Min(bufferSize, (int)(destination.Length - destination.Position))))
        {
            while (destination.Length - destination.Position >= bufferSize)
            {
                ReadExactly(buffer.Span);
                destination.Write(buffer.Span);
            }
            var remaining = (int)(destination.Length - destination.Position);
            if (remaining > 0)
            {
                var span = buffer.Span[..remaining];
                ReadExactly(span);
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
    public override async Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken) => await Task.Run(() => CopyTo(destination, bufferSize), cancellationToken).ConfigureAwait(false);
}
