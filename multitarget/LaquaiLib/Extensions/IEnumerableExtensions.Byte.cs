using System.Buffers;

namespace LaquaiLib.Extensions;

public static partial class IEnumerableExtensions
{
    extension(IEnumerable<byte> enumerable)
    {
        /// <summary>
        /// Using the specified <see cref="byte"/> sequence, creates a new <typeparamref name="T"/> instance.
        /// </summary>
        /// <typeparam name="T">The <see cref="Type"/> of the <see langword="struct"/> to marshal the specified bytes into.</typeparam>
        /// <returns>The new <typeparamref name="T"/> instance.</returns>
        public T IntoStruct<T>() where T : unmanaged
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
                    byte[] bytesBuffer = null;
                    scoped var bytes = size <= Config.MaxStackallocSize ? stackalloc byte[size] : (bytesBuffer = ArrayPool<byte>.Shared.Rent(size)).AsSpan(0, size);
                    try
                    {
                        var written = enumerable.Into(bytes);
                        if (written < size)
                            throw new InvalidOperationException($"Not enough bytes to read a {typeof(T)}.");
                        return MemoryMarshal.Read<T>(bytes[..sizeof(T)]);
                    }
                    finally
                    {
                        if (bytesBuffer != null)
                            ArrayPool<byte>.Shared.Return(bytesBuffer);
                    }
                }
            }
        }
    }
}
