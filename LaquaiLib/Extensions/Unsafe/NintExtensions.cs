using System.Runtime.InteropServices;

namespace LaquaiLib.Extensions.Unsafe;

/// <summary>
/// Provides Extension Methods for the <see cref="nint"/> and <see cref="nuint"/> types, which implicitly includes pointers.
/// </summary>
public static unsafe class NintExtensions
{
    /// <summary>
    /// Constructs a <see cref="byte"/> array from a region of memory starting at <paramref name="address"/> with the specified <paramref name="length"/>.
    /// </summary>
    /// <param name="address">The address of the first <see cref="byte"/> to be included in the array.</param>
    /// <param name="length">The length of the array.</param>
    /// <returns>The constructed <see cref="byte"/> array.</returns>
    public static byte[] ToArray(this nint address, int length)
    {
        var arr = new byte[length];
        Marshal.Copy(address, arr, 0, length);
        return arr;
    }
    /// <summary>
    /// Copies the contents of the memory region starting at <paramref name="address"/> to the specified <paramref name="arr"/>.
    /// </summary>
    /// <param name="address">The address of the first <see cref="byte"/> to be copied.</param>
    /// <param name="arr">The <see cref="byte"/> array to copy the memory region to. Its <see cref="Array.Length"/> dictates how many bytes will be copied.</param>
    public static void ToArray(this nint address, byte[] arr) => Marshal.Copy(address, arr, 0, arr.Length);
}
