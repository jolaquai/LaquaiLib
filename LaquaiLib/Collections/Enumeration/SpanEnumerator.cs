namespace LaquaiLib.Collections.Enumeration;

/// <summary>
/// Implements the enumerator pattern to enumerate the elements in a <see cref="ReadOnlySpan{T}"/>.
/// </summary>
/// <typeparam name="T">The type of the elements in the <see cref="ReadOnlySpan{T}"/>.</typeparam>
/// <param name="span">The <see cref="ReadOnlySpan{T}"/> to enumerate.</param>
public ref struct SpanEnumerator<T>(ReadOnlySpan<T> span)
{
    private readonly ReadOnlySpan<T> _span = span;
    private int _index = -1;

    /// <summary>
    /// Returns the current instance. For use in <see langword="foreach"/> statements.
    /// </summary>
    /// <returns>The current instance.</returns>
    public readonly SpanEnumerator<T> GetEnumerator() => this;

    /// <summary>
    /// Gets the element at the current position in the <see cref="Span{T}"/>.
    /// </summary>
    public readonly T Current => _span[_index];
    /// <summary>
    /// Gets the element at the current position in the <see cref="Span{T}"/>.
    /// </summary>
    /// <returns><see langword="true"/> if the enumerator was successfully advanced to the next element; <see langword="false"/> if the enumerator has passed the end of the collection.</returns>
    public bool MoveNext() => ++_index < _span.Length;
    /// <summary>
    /// Resets the enumerator to immediately before the first element in the <see cref="Span{T}"/>.
    /// </summary>
    public void Reset() => _index = -1;
}
