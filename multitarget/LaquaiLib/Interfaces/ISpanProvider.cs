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

/// <summary>
/// Defines a contract for types that provide a <see cref="Memory{T}"/> over an arbitrary data structure.
/// </summary>
/// <typeparam name="T">The type of elements in the memory.</typeparam>
public interface IMemoryProvider<T> : IReadOnlyMemoryProvider<T>, IDisposable
{
    /// <summary>
    /// Gets the <see cref="Memory{T}"/> provided by the implementing type.
    /// </summary>
    public Memory<T> Memory { get; }

    ReadOnlyMemory<T> IReadOnlyMemoryProvider<T>.ReadOnlyMemory => Memory;
}

/// <summary>
/// Defines a contract for types that provide a <see cref="ReadOnlyMemory{T}"/> over an arbitrary data structure.
/// </summary>
/// <typeparam name="T">The type of elements in the memory.</typeparam>
public interface IReadOnlyMemoryProvider<T> : IDisposable
{
    /// <summary>
    /// Gets the <see cref="ReadOnlyMemory{T}"/> provided by the implementing type.
    /// </summary>
    public ReadOnlyMemory<T> ReadOnlyMemory { get; }
}
