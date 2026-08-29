using System.Buffers;

namespace LaquaiLib.Wrappers;

/// <summary>
/// Wraps an array of <typeparamref name="T"/>, the <see cref="ArrayPool{T}"/> it was rented from, an optional <paramref name="offset"/> and <paramref name="length"/> delimiting the span of the array to be used, and a <paramref name="clear"/> flag indicating whether the array should be cleared when returned to the pool.
/// </summary>
/// <typeparam name="T">The type of the elements in the array.</typeparam>
/// <param name="array">The array to wrap.</param>
/// <param name="offset">The offset in the array where the span starts. Defaults to 0.</param>
/// <param name="length">The length of the span. Defaults to -1, which means the span will extend to the end of the array.</param>
/// <param name="arrayPool">The <see cref="ArrayPool{T}"/> from which the array was rented. Defaults to <see cref="ArrayPool{T}.Shared"/>.</param>
/// <param name="clear">Whether to clear the array when returned to the pool. Defaults to <see langword="false"/>.</param>
public struct RentedArray<T>(T[] array, int offset = 0, int length = -1, ArrayPool<T> arrayPool = null, bool clear = false) : IDisposable
{
    /// <summary>
    /// Gets the underlying array.
    /// </summary>
    public T[] Array { get; private set; } = array;
    /// <summary>
    /// The offset in the array where the span starts.
    /// </summary>
    public readonly int Offset = offset;
    /// <summary>
    /// The length of the span.
    /// </summary>
    public readonly int Length = length == -1 ? array.Length - offset : length;
    /// <summary>
    /// The <see cref="ArrayPool{T}"/> from which the array was rented.
    /// </summary>
    public readonly ArrayPool<T> ArrayPool = arrayPool ?? ArrayPool<T>.Shared;
    /// <summary>
    /// Whether to clear the array when returned to the pool.
    /// </summary>
    public readonly bool Clear = clear;

    /// <summary>
    /// Gets a <see cref="Span{T}"/> representing the portion of the array defined by the <see cref="Offset"/> and <see cref="Length"/>.
    /// </summary>
    public readonly Span<T> Span => Array.AsSpan(Offset, Length);

    /// <summary>
    /// Disposes the <see cref="RentedArray{T}"/> by returning the underlying array to the <see cref="ArrayPool{T}"/>.
    /// </summary>
    public void Dispose()
    {
        if (Array is T[] array)
        {
            ArrayPool.Return(array, Clear);
            Array = null;
        }
    }
}
