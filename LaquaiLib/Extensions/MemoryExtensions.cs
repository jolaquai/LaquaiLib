using System.Runtime.CompilerServices;

namespace LaquaiLib.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="Span{T}"/>, <see cref="ReadOnlySpan{T}"/>, <see cref="Memory{T}"/> and <see cref="ReadOnlyMemory{T}"/> types.
/// </summary>
public static class MemoryExtensions
{
    /// <summary>
    /// Converts the elements of a <see cref="ReadOnlySpan{T}"/> using a <paramref name="selector"/> function.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements of the input span.</typeparam>
    /// <typeparam name="TResult">The type of the elements of the output array.</typeparam>
    /// <param name="span">The input span.</param>
    /// <param name="selector">A <see cref="Func{T, TResult}"/> that is passed each element of the input span and returns the result.</param>
    /// <returns>The array of the results.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TResult[] ToArray<TSource, TResult>(this ReadOnlySpan<TSource> span, Func<TSource, TResult> selector)
    {
        var ret = new TResult[span.Length];
        for (var i = 0; i < span.Length; i++)
        {
            ret[i] = selector(span[i]);
        }
        return ret;
    }
    /// <summary>
    /// Converts the elements of a <see cref="ReadOnlyMemory{T}"/> using a <paramref name="selector"/> function.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements of the input memory.</typeparam>
    /// <typeparam name="TResult">The type of the elements of the output array.</typeparam>
    /// <param name="memory">The input memory.</param>
    /// <param name="selector">A <see cref="Func{T, TResult}"/> that is passed each element of the input memory and returns the result.</param>
    /// <returns>The array of the results.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TResult[] ToArray<TSource, TResult>(this ReadOnlyMemory<TSource> memory, Func<TSource, TResult> selector) => memory.Span.ToArray(selector);
}
