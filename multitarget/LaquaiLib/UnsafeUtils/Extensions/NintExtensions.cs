namespace LaquaiLib.UnsafeUtils.Extensions;

/// <summary>
/// Provides Extension Methods for the <see cref="nint"/> and <see cref="nuint"/> types, which implicitly includes pointers.
/// </summary>
public static unsafe class NintExtensions
{
    extension(nint address)
    {
        /// <summary>
        /// Constructs a <see cref="byte"/> array from a region of memory starting at <paramref name="address"/> with the specified <paramref name="length"/>.
        /// </summary>
        /// <param name="address">The address of the first <see cref="byte"/> to be included in the array.</param>
        /// <param name="length">The length of the array.</param>
        /// <returns>The constructed <see cref="byte"/> array.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte[] ToArray(int length)
        {
            var arr = GC.AllocateUninitializedArray<byte>(length);
            Marshal.Copy(address, arr, 0, length);
            return arr;
        }
        /// <summary>
        /// Copies the contents of the memory region starting at <paramref name="address"/> to the specified <paramref name="span"/>.
        /// </summary>
        /// <param name="address">The address of the first <see cref="byte"/> to be copied.</param>
        /// <param name="span">The <see cref="byte"/> array to copy the memory region to. Its <see cref="Span{T}.Length"/> dictates how many bytes will be copied.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(Span<byte> span) => address.AsSpan<byte>(span.Length).CopyTo(span);
        /// <summary>
        /// Copies the contents of the memory region starting at <paramref name="address"/> to the specified <paramref name="span"/>.
        /// </summary>
        /// <param name="address">The address of the first <typeparamref name="T"/> to be copied.</param>
        /// <param name="span">The <paramref name="T"/> array to copy the memory region to. Its <see cref="Span{T}.Length"/> dictates how many bytes will be copied.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo<T>(Span<T> span) where T : unmanaged => address.AsSpan<T>(span.Length).CopyTo(span);
        /// <summary>
        /// Wraps the specified <paramref name="address"/> in a <see cref="Span{T}"/> of <typeparamref name="T"/> with the specified <paramref name="length"/>.
        /// </summary>
        /// <param name="address">The address of the first <see cref="byte"/> to be included in the <see cref="Span{T}"/>.</param>
        /// <param name="length">The length of the <see cref="Span{T}"/>.</param>
        /// <returns>The constructed <see cref="Span{T}"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> AsSpan<T>(int length) where T : unmanaged => new Span<T>((void*)address, length);
    }
}
