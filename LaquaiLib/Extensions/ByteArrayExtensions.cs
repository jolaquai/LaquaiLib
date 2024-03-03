using System.Text;

namespace LaquaiLib.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="Array"/> of <see cref="char"/> Type.
/// </summary>
public static class ByteArrayExtensions
{
    /// <summary>
    /// Converts an array of <see cref="byte"/>s to its equivalent string representation that is encoded with uppercase hex characters.
    /// </summary>
    /// <param name="bytes">The <see cref="byte"/> array to convert.</param>
    /// <returns>The string as described.</returns>
    /// <remarks>This method uses the internal <see cref="Convert.ToHexString(byte[])"/> method for the conversion, but its output is reversed appropriately to account for endianness differences.</remarks>
    public static string ToHexString(this byte[] bytes)
    {
        var sb = new StringBuilder();
        System.Convert.ToHexString(bytes).Reverse().Chunk(2).ForEach(chars => sb.Append(chars.Reverse().ToArray()));
        return sb.ToString().Trim();
    }
    /// <summary>
    /// Converts a <see cref="ReadOnlySpan{T}"/> of <see cref="byte"/> to its equivalent string representation that is encoded with uppercase hex characters.
    /// </summary>
    /// <param name="bytes">The <see cref="byte"/> span to convert.</param>
    /// <returns>The string as described.</returns>
    /// <remarks>This method uses the internal <see cref="Convert.ToHexString(byte[])"/> method for the conversion, but its output is reversed appropriately to account for endianness differences.</remarks>
    public static string ToHexString(this ReadOnlySpan<byte> bytes)
    {
        var sb = new StringBuilder();
        System.Convert.ToHexString(bytes).Reverse().Chunk(2).ForEach(chars => sb.Append(chars.Reverse().ToArray()));
        return sb.ToString().Trim();
    }

    /// <summary>
    /// Creates a (non-resizable!) <see cref="MemoryStream"/> from the given <see cref="byte"/> array.
    /// </summary>
    /// <param name="bytes">The <see cref="byte"/> array to create the <see cref="MemoryStream"/> from.</param>
    /// <returns>The created <see cref="MemoryStream"/>.</returns>
    public static MemoryStream AsMemoryStream(this byte[] bytes) => new MemoryStream(bytes);
    /// <summary>
    /// Creates a <see cref="MemoryStream"/> from the given <see cref="byte"/> array. Its <see cref="Stream.Position"/> upon return is set to the <see cref="Array.Length"/> of <paramref name="bytes"/>, i.e. it is not sought to the beginning.
    /// </summary>
    /// <param name="bytes">The <see cref="byte"/> array to write into the new <see cref="MemoryStream"/>.</param>
    /// <returns>The created <see cref="MemoryStream"/>.</returns>
    public static MemoryStream ToMemoryStream(this byte[] bytes)
    {
        var ms = new MemoryStream(bytes.Length + 1);
        ms.Write(bytes, 0, bytes.Length);
        return ms;
    }
}
