using System.Buffers;

namespace LaquaiLib.Extensions;

/// <summary>
/// Provides extensions for <see cref="ArrayPool{T}"/>.
/// </summary>
public static class ArrayPoolExtensions
{
    extension<T>(ArrayPool<T> pool)
    {
        /// <summary>
        /// Returns <paramref name="array"/> to <paramref name="pool"/>, ensuring it is cleared if <typeparamref name="T"/> is a reference type or contains references.
        /// </summary>
        /// <param name="array">The array to return to the pool.</param>
        public void ReturnSafe(T[] array)
        {
            if (array is null)
                return;

            var refs = RuntimeHelpers.IsReferenceOrContainsReferences<T>();
            pool.Return(array, refs);
        }
    }

    extension(ArrayPool<byte> pool)
    {
        /// <summary>
        /// Rents a <see langword="byte"/> array from the <paramref name="pool"/> and hands out a <typeparamref name="TAs"/>-typed <see cref="Span{T}"/> over it.
        /// </summary>
        /// <typeparam name="TAs">The <see langword="unmanaged"/> type to cast views over the rented <see langword="byte"/> array to.</typeparam>
        /// <param name="minimumSize">The minimum number of instances of <typeparamref name="TAs"/> to request for the rented array.</param>
        /// <param name="span">The <typeparamref name="TAs"/>-typed <see cref="Span{T}"/> view over the rented <see langword="byte"/> array.</param>
        /// <returns>The rented <see langword="byte"/> array.</returns>
        public byte[] Rent<TAs>(int minimumSize, out Span<TAs> span) where TAs : unmanaged
        {
            var effectiveSize = sizeof(TAs) * minimumSize;
            if (effectiveSize > Array.MaxLength)
                throw new ArgumentOutOfRangeException(nameof(minimumSize), "The requested array size exceeds the maximum allowed length.");

            var arr = pool.Rent(effectiveSize);
            // fit as many TAs's as possible into the rented byte array
            var fit = arr.Length / sizeof(TAs);
            span = MemoryMarshal.Cast<byte, TAs>(arr.AsSpan())[..fit];
            return arr;
        }
    }
}
