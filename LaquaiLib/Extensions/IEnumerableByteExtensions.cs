using System.Reflection;
using System.Runtime.InteropServices;

using LaquaiLib.Wrappers;

namespace LaquaiLib.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="IEnumerable{T}"/> of <see cref="byte"/> Type.
/// </summary>
public static class IEnumerableByteExtensions
{
    /// <summary>
    /// Using the specified <see cref="byte"/> sequence, creates a new <typeparamref name="T"/> instance.
    /// </summary>
    /// <typeparam name="T">The <see cref="Type"/> of the <see langword="struct"/> to marshal the specified bytes into.</typeparam>
    /// <param name="enumerable">The <see cref="IEnumerable{T}"/> of <see cref="byte"/> to marshal into a new <typeparamref name="T"/> instance.</param>
    /// <param name="bigEndian">Whether the individual byte sequences the struct's fields are represented by are in big-endian order (which is the default for byte sequences returned from <see cref="BitConverter"/>).</param>
    /// <returns>The new <typeparamref name="T"/> instance.</returns>
    /// <exception cref="ArgumentException">Thrown if the specified struct type is not declared in a manner compatible with marshalling, or the specified byte sequence is not of the correct length.</exception>
    public static T IntoStruct<T>(this IEnumerable<byte> enumerable, bool bigEndian = true)
        where T : struct
    {
        if (typeof(T).StructLayoutAttribute is not StructLayoutAttribute sla)
        {
            throw new ArgumentException($"The struct type to marshal the specified bytes into must be decorated with a {typeof(StructLayoutAttribute).FullName}.");
        }
        if (sla.Value != LayoutKind.Sequential)
        {
            throw new ArgumentException("The struct type to marshal the specified bytes into must be laid-out sequentially.");
        }
        if (sla.Pack == 0)
        {
            //throw new ArgumentException("The struct type to marshal the specified bytes into must be packed explicitly to find memory boundaries.");
        }

        var enumerated = enumerable as byte[] ?? enumerable.ToArray();
        var fields = typeof(T).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        var structSize = Marshal.SizeOf<T>();

        if (enumerated.Length != structSize && sla.Pack == 0)
        {
            throw new ArgumentException($"With a struct that is packed, the specified byte sequence must be exactly {structSize} bytes long.");
        }
        var mem = TempAlloc.Create<T>(true);
        var span = mem.Data;

        var byteSpan = enumerated.AsSpan();

        if (!bigEndian)
        {
            // The data is little-endian, which is horrible
            // We now have to reverse the byte order of each field
            foreach (var field in fields)
            {
                var fieldSize = Marshal.SizeOf(field.FieldType);
                var fieldOffset = Marshal.OffsetOf<T>(field.Name).ToInt32();

                var fieldSpan = byteSpan.Slice(fieldOffset, fieldSize);
                fieldSpan.Reverse();
            }
        }

        foreach (var field in fields)
        {
            var fieldSize = Marshal.SizeOf(field.FieldType);
            var fieldOffset = Marshal.OffsetOf<T>(field.Name).ToInt32();

            var fieldSpan = byteSpan.Slice(fieldOffset, fieldSize);
            fieldSpan.CopyTo(span.Slice(fieldOffset, fieldSize));
        }
        return Marshal.PtrToStructure<T>(mem);
    }
}
