namespace LaquaiLib.Collections.Enumeration;

/// <summary>
/// Implements the enumerable pattern to enumerate the elements in a <see cref="ReadOnlyMemory{T}"/>.
/// </summary>
/// <typeparam name="T">The type of the elements in the <see cref="ReadOnlyMemory{T}"/>.</typeparam>
/// <param name="memory">The <see cref="ReadOnlyMemory{T}"/> to enumerate.</param>
public struct MemoryEnumerable<T>(ReadOnlyMemory<T> memory) : IEnumerable<T>, IEnumerator<T>
{
    private readonly ReadOnlyMemory<T> _memory = memory;
    private int index = -1;

    /// <summary>
    /// Returns the current instance. For use in <see langword="foreach"/> statements.
    /// </summary>
    /// <returns>The current instance.</returns>
    public readonly IEnumerator<T> GetEnumerator() => this;
    readonly IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// Gets the element at the current position in the <see cref="Memory{T}"/>.
    /// </summary>
    public readonly T Current => _memory.Span[index];
    readonly object IEnumerator.Current => Current;

    /// <summary>
    /// Gets the element at the current position in the <see cref="Memory{T}"/>.
    /// </summary>
    /// <returns><see langword="true"/> if the enumerator was successfully advanced to the next element; <see langword="false"/> if the enumerator has passed the end of the collection.</returns>
    public bool MoveNext() => ++index < _memory.Length;
    /// <summary>
    /// Resets the enumerator to immediately before the first element in the <see cref="Memory{T}"/>.
    /// </summary>
    public void Reset() => index = -1;
    public readonly void Dispose() { }
}
