using LaquaiLib.Core;

namespace LaquaiLib.Extensions;

public static partial class IEnumerableExtensions
{
    extension(IEnumerable<byte> enumerable)
    {
        /// <summary>
        /// Using the specified <see cref="byte"/> sequence, creates a new <typeparamref name="T"/> instance.
        /// </summary>
        /// <typeparam name="T">The <see cref="Type"/> of the <see langword="struct"/> to marshal the specified bytes into.</typeparam>
        /// <param name="enumerable">The <see cref="IEnumerable{T}"/> of <see cref="byte"/> to marshal into a new <typeparamref name="T"/> instance.</param>
        /// <returns>The new <typeparamref name="T"/> instance.</returns>
        public T IntoStruct<T>() where T : struct
        {
            switch (enumerable)
            {
                case byte[] array:
                    return MemoryMarshal.Read<T>(array);
                case List<byte> list:
                    return MemoryMarshal.Read<T>(list.AsSpan());
                default:
                {
                    var size = Unsafe.SizeOf<T>();
                    Span<byte> bytes = size <= Configuration.MaxStackallocSize ? stackalloc byte[size] : new byte[size];
                    _ = enumerable.Into(bytes);
                    return MemoryMarshal.Read<T>(bytes);
                }
            }
        }
    }
}
