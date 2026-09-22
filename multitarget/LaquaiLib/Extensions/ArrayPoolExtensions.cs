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

    extension<TSource>(ArrayPool<TSource> pool) where TSource : unmanaged
    {
        /// <summary>
        /// Requests a <typeparamref name="TSource"/> array from the <paramref name="pool"/> and hands out a <typeparamref name="TAs"/>-typed <see cref="Span{T}"/> over it.
        /// <paramref name="minimumSize"/> is used as a base for calculating the minimum number of <typeparamref name="TSource"/> instances to request from the pool, based on the size of <typeparamref name="TAs"/>.
        /// </summary>
        /// <typeparam name="TAs">The <see langword="unmanaged"/> type to cast views over the rented <typeparamref name="TSource"/> array to.</typeparam>
        /// <param name="minimumSize">The minimum number of instances of <typeparamref name="TAs"/> to request for the rented array.</param>
        /// <param name="span">The <typeparamref name="TAs"/>-typed <see cref="Span{T}"/> view over the rented <typeparamref name="TSource"/> array.</param>
        /// <returns>The rented <typeparamref name="TSource"/> array.</returns>
        /// <remarks>
        /// Easiest way to use this method is to call it by explicitly typing the <paramref name="span"/> parameter:
        /// <code lang="csharp">
        /// var array = ArrayPool&lt;byte&gt;.Shared.Rent(minimumSize, out Span&lt;long&gt; span);
        /// // array.GetType() == typeof(byte[])
        /// </code>
        /// </remarks>
        public TSource[] Rent<TAs>(int minimumSize, out Span<TAs> span) where TAs : unmanaged
        {
            if (minimumSize < 0)
                throw new ArgumentOutOfRangeException(nameof(minimumSize), "The requested size must be non-negative.");

            var effectiveSizeBytes = sizeof(TAs) * minimumSize;
            var effectiveSize = (effectiveSizeBytes + sizeof(TSource) - 1) / sizeof(TSource);
            if (effectiveSize > Array.MaxLength)
                throw new ArgumentOutOfRangeException(nameof(minimumSize), "The requested array size exceeds the maximum allowed length.");

            var arr = pool.Rent(effectiveSize);
            // fit as many TAs's as possible into the rented TSource array
            var fit = arr.Length * sizeof(TSource) / sizeof(TAs);
            span = MemoryMarshal.Cast<TSource, TAs>(arr.AsSpan())[..fit];
            return arr;
        }
    }
}
