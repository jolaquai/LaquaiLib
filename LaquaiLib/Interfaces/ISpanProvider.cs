namespace LaquaiLib.Interfaces;

/// <summary>
/// Defines a contract for types that provide a <see cref="Span{T}"/> over an arbitrary data structure.
/// </summary>
/// <typeparam name="T">The type of elements in the span.</typeparam>
public interface ISpanProvider<T> : IReadOnlySpanProvider<T>, IDisposable
{
    /// <summary>
    /// Gets the <see cref="Span{T}"/> provided by the implementing type.
    /// </summary>
    public Span<T> Span { get; }

    ReadOnlySpan<T> IReadOnlySpanProvider<T>.ReadOnlySpan => Span;
}

/// <summary>
/// Defines a contract for types that provide a <see cref="ReadOnlySpan{T}"/> over an arbitrary data structure.
/// </summary>
/// <typeparam name="T">The type of elements in the span.</typeparam>
public interface IReadOnlySpanProvider<T> : IDisposable
{
    /// <summary>
    /// Gets the <see cref="ReadOnlySpan{T}"/> provided by the implementing type.
    /// </summary>
    public ReadOnlySpan<T> ReadOnlySpan { get; }
}
