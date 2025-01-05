using System.Collections;
using System.Runtime.InteropServices;

namespace LaquaiLib.Collections.Enumeration;

/// <summary>
/// Implements the enumerator pattern to enumerate the elements of a multidimensional array sequentially.
/// </summary>
/// <typeparam name="T">The type of the elements in the array.</typeparam>
public unsafe struct MultiDimArrayEnumerator<T> : IEnumerable<T>, IEnumerator<T>, IDisposable
{
    private readonly Array _array;
    private readonly int _length;
    private readonly GCHandle _handle;
    private readonly T* _start;
    private T* ptr;

    public T Current { get; private set; }

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

    public readonly IEnumerator<T> GetEnumerator() => this;
    readonly IEnumerator IEnumerable.GetEnumerator() => this;

    public void Dispose()
    {
        _handle.Free();
        GC.SuppressFinalize(this);
    }
}
