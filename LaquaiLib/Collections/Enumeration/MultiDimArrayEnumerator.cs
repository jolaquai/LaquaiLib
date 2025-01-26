using System.Collections;
using System.Runtime.InteropServices;

using LaquaiLib.Interfaces;

namespace LaquaiLib.Collections.Enumeration;

#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type

/// <summary>
/// Implements the enumerator pattern to enumerate the elements of a multidimensional array sequentially.
/// </summary>
/// <typeparam name="T">The type of the elements in the array.</typeparam>
public unsafe struct MultiDimArrayEnumerator<T> : IEnumerable<T>, IEnumerator<T>, ISpanProvider<T>
{
    private readonly Array _array;
    private readonly int _length;
    private readonly GCHandle _handle;
    private readonly T* _start;
    private T* ptr;

    /// <inheritdoc/>
    public T Current { get; private set; }
    /// <summary>
    /// Gets a <see cref="Span{T}"/> over the entire array.
    /// </summary>
    public readonly Span<T> Span => new Span<T>(_start, _length);

    /// <inheritdoc/>
    public bool MoveNext()
    {
        if (ptr - _start < _length)
        {
            Current = *ptr;
            ptr++;
            return true;
        }
        return false;
    }
    /// <inheritdoc/>
    public void Reset() => ptr = _start;

    readonly object IEnumerator.Current => Current;

    /// <summary>
    /// Initializes a new <see cref="MultiDimArrayEnumerator{T}"/> with the specified <paramref name="array"/>.
    /// </summary>
    /// <param name="array">The array to enumerate.</param>
    public MultiDimArrayEnumerator(Array array)
    {
        _array = array;
        _handle = GCHandle.Alloc(array, GCHandleType.Pinned);
        _start = (T*)_handle.AddrOfPinnedObject();
        _length = array.Length;
        ptr = _start;
    }

    /// <inheritdoc/>
    public readonly IEnumerator<T> GetEnumerator() => this;
    readonly IEnumerator IEnumerable.GetEnumerator() => this;

    /// <summary>
    /// Frees the <see cref="GCHandle"/> used to pin the target array.
    /// </summary>
    public readonly void Dispose()
    {
        _handle.Free();
        GC.SuppressFinalize(this);
    }
}
