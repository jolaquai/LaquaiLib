namespace LaquaiLib.Collections.Enumeration;

/// <summary>
/// Implements the enumerator pattern to enumerate the elements in a <see cref="ReadOnlyMemory{T}"/>.
/// </summary>
/// <typeparam name="T">The type of the elements in the <see cref="ReadOnlyMemory{T}"/>.</typeparam>
/// <param name="memory">The <see cref="ReadOnlyMemory{T}"/> to enumerate.</param>
public struct MemoryEnumerator<T>(ReadOnlyMemory<T> memory)
{
    private readonly ReadOnlyMemory<T> _memory = memory;
    private int _index = -1;

    /// <summary>
    /// Returns the current instance. For use in <see langword="foreach"/> statements.
    /// </summary>
    /// <returns>The current instance.</returns>
    public readonly MemoryEnumerator<T> GetEnumerator() => this;

    /// <summary>
    /// Gets the element at the current position in the <see cref="Memory{T}"/>.
    /// </summary>
    public readonly T Current => _memory.Span[_index];
    /// <summary>
    /// Gets the element at the current position in the <see cref="Memory{T}"/>.
    /// </summary>
    /// <returns><see langword="true"/> if the enumerator was successfully advanced to the next element; <see langword="false"/> if the enumerator has passed the end of the collection.</returns>
    public bool MoveNext() => ++_index < _memory.Length;
    /// <summary>
    /// Resets the enumerator to immediately before the first element in the <see cref="Memory{T}"/>.
    /// </summary>
    public void Reset() => _index = -1;
}
