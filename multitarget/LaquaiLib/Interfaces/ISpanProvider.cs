using System.Buffers;

namespace LaquaiLib.Interfaces;

/// <summary>
/// Defines a contract for types that provide a <see cref="Span{T}"/> over an arbitrary data structure.
/// </summary>
/// <typeparam name="T">The type of elements in the span.</typeparam>
public interface ISpanProvider<T> : IReadOnlySpanProvider<T>
{
    /// <summary>
    /// Gets the <see cref="Span{T}"/> provided by the implementing type.
    /// </summary>
    public Span<T> Span { get; }

    ReadOnlySpan<T> IReadOnlySpanProvider<T>.ReadOnlySpan
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Span;
    }

    /// <summary>
    /// Gets a pinnable reference to the first element of the <see cref="ReadOnlySpan{T}"/> provided by the implementing type.
    /// If the span is empty, this method returns a <see langword="null"/> reference. Such a reference may be used for pinning, but must never be dereferenced.
    /// </summary>
    /// <returns>The pinnable reference.</returns>
    public new ref T GetPinnableReference()
    {
        var span = Span;
        return ref span.GetPinnableReference();
    }

    ref readonly T IReadOnlySpanProvider<T>.GetPinnableReference() => ref GetPinnableReference();
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
    /// <summary>
    /// Gets a pinnable reference to the first element of the <see cref="ReadOnlySpan{T}"/> provided by the implementing type.
    /// If the span is empty, this method returns a <see langword="null"/> reference. Such a reference may be used for pinning, but must never be dereferenced.
    /// </summary>
    /// <returns>The pinnable reference.</returns>
    public ref readonly T GetPinnableReference()
    {
        var span = ReadOnlySpan;
        return ref span.GetPinnableReference();
    }
}

/// <summary>
/// Defines a contract for types that provide a <see cref="Memory{T}"/> over an arbitrary data structure.
/// </summary>
/// <typeparam name="T">The type of elements in the memory.</typeparam>
public interface IMemoryProvider<T> : ISpanProvider<T>, IReadOnlyMemoryProvider<T>, IMemoryOwner<T>
{
    ReadOnlyMemory<T> IReadOnlyMemoryProvider<T>.ReadOnlyMemory
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Memory;
    }
    Span<T> ISpanProvider<T>.Span
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Memory.Span;
    }
    ReadOnlySpan<T> IReadOnlySpanProvider<T>.ReadOnlySpan
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Memory.Span;
    }
}

/// <summary>
/// Defines a contract for types that provide a <see cref="ReadOnlyMemory{T}"/> over an arbitrary data structure.
/// </summary>
/// <typeparam name="T">The type of elements in the memory.</typeparam>
public interface IReadOnlyMemoryProvider<T> : IReadOnlySpanProvider<T>
{
    /// <summary>
    /// Gets the <see cref="ReadOnlyMemory{T}"/> provided by the implementing type.
    /// </summary>
    public ReadOnlyMemory<T> ReadOnlyMemory { get; }

    ReadOnlySpan<T> IReadOnlySpanProvider<T>.ReadOnlySpan
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ReadOnlyMemory.Span;
    }
}
