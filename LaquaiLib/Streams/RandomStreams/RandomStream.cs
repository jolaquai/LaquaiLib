namespace LaquaiLib.Streams.RandomStreams;

/// <summary>
/// Represents a <see cref="Stream"/> that generates random bytes upon reading from it.
/// It comes in two variants: <see cref="RandomStream"/> for applications that do not require cryptographic security and <see cref="CryptographicRandomStream"/>.
/// </summary>
public class RandomStream : ExceptStream
{
    /// <inheritdoc/>
    public override bool CanRead => base.CanRead;

    /// <summary>
    /// Exposes a <see cref="System.Random"/> instance derived types may use.
    /// If unused, should be set to <see langword="null"/>.
    /// </summary>
    protected Random Random { get; set; }

    /// <summary>
    /// Initializes a new <see cref="RandomStream"/>.
    /// </summary>
    public RandomStream() : this(Random.Shared)
    {
    }
    /// <summary>
    /// Initializes a new <see cref="RandomStream"/> with the specified seed.
    /// </summary>
    public RandomStream(int seed)
    {
        Random = new Random(seed);
    }
    /// <summary>
    /// Initializes a new <see cref="RandomStream"/> with the specified <see cref="System.Random"/> instance.
    /// </summary>
    public RandomStream(Random random)
    {
        Random = random;
    }

    /// <summary>
    /// The length of the <see cref="Stream"/>. This is irrelevant for <see cref="RandomStream"/>. Its length will never change.
    /// </summary>
    public override long Length { get; } = long.MaxValue;
    /// <summary>
    /// The position in the <see cref="Stream"/>. This is irrelevant for <see cref="RandomStream"/>. Its position will never change.
    /// </summary>
    public override long Position
    {
        get => 0;
        set { }
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
        Random.NextBytes(buffer);
        return buffer.Length;
    }
    /// <summary>
    /// Returns a random <see langword="byte"/> value.
    /// </summary>
    /// <returns>The random <see langword="byte"/> value.</returns>
    public override int ReadByte() => Random.Next(byte.MinValue, byte.MaxValue + 1);
}
