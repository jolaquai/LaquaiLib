using System.Security.Cryptography;

using LaquaiLib.Unsafe;

namespace LaquaiLib.Streams.RandomStreams;

/// <summary>
/// Implements <see cref="RandomStream"/> with a cryptographic random number generator.
/// </summary>
public class CryptographicRandomStream() : RandomStream(null)
{
    private readonly RandomNumberGenerator rng = RandomNumberGenerator.Create();

    /// <inheritdoc/>
    public override int Read(Span<byte> buffer)
    {
        rng.GetBytes(buffer);
        return buffer.Length;
    }
    /// <inheritdoc/>
    public override int ReadByte()
    {
        var buffer = MemoryManager.CreateBuffer(1);
        rng.GetBytes(buffer);
        return buffer[0];
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            rng?.Dispose();
        }

        base.Dispose(disposing);
    }
}