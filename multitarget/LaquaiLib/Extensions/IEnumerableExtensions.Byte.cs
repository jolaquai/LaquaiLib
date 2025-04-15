using System.Runtime.InteropServices;

using LaquaiLib.Core;

namespace LaquaiLib.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="IEnumerable{T}"/> of <see cref="byte"/> Type.
/// </summary>
public static class IEnumerableExtensionsByte
{
    /// <summary>
    /// Using the specified <see cref="byte"/> sequence, creates a new <typeparamref name="T"/> instance.
    /// </summary>
    /// <typeparam name="T">The <see cref="Type"/> of the <see langword="struct"/> to marshal the specified bytes into.</typeparam>
    /// <param name="enumerable">The <see cref="IEnumerable{T}"/> of <see cref="byte"/> to marshal into a new <typeparamref name="T"/> instance.</param>
    /// <returns>The new <typeparamref name="T"/> instance.</returns>
    public static T IntoStruct<T>(this IEnumerable<byte> enumerable) where T : struct
    {
        switch (enumerable)
        {
            case byte[] array:
                return MemoryMarshal.Read<T>(array);
            case List<byte> list:
                return MemoryMarshal.Read<T>(list.AsSpan());
            default:
            {
                var size = System.Runtime.CompilerServices.Unsafe.SizeOf<T>();
                Span<byte> bytes = size <= Configuration.MaxStackallocSize ? stackalloc byte[size] : new byte[size];
                enumerable.Into(bytes);
                return MemoryMarshal.Read<T>(bytes);
            }
        }
    }
}
