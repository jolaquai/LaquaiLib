namespace LaquaiLib.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="Array"/> of <see cref="byte"/> Type.
/// </summary>
public static class ArrayExtensionsByte
{
    /// <summary>
    /// Creates a resizable <see cref="MemoryStream"/> from the given <see cref="byte"/> array. Its <see cref="Stream.Position"/> upon return is set to the <see cref="Array.Length"/> of <paramref name="bytes"/>, i.e. it is not sought to the beginning.
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
