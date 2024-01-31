namespace LaquaiLib.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="Span{T}"/> Type.
/// </summary>
public static class SpanExtensions
{
    /// <summary>
    /// Returns  a <see cref="ReadOnlySpan{T}"/> that represents a read-only view of the current <see cref="Span{T}"/> instance.
    /// </summary>
    /// <typeparam name="T">The type of the elements in <paramref name="span"/>.</typeparam>
    /// <param name="span">The <see cref="Span{T}"/> to wrap.</param>
    /// <returns>The <see cref="ReadOnlySpan{T}"/> that acts as a read-only view of the current <see cref="Span{T}"/>.</returns>
    public static ReadOnlySpan<T> AsReadOnly<T>(this Span<T> span) => span;
}
