using LaquaiLib.Interfaces;

namespace LaquaiLib.Collections.Enumeration;

#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type

/// <summary>
/// Implements the enumerator pattern to enumerate the elements of a (potentially) multidimensional array sequentially.
/// </summary>
/// <typeparam name="T">The type of the elements in the array. This must be exactly the same type as the array, otherwise users of this type will be faced with non-sensical exceptions.</typeparam>
public unsafe struct MultiDimArrayEnumerable<T> : IEnumerable<T>, IEnumerator<T>, ISpanProvider<T>, IMemoryProvider<T>
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
    /// <summary>
    /// Gets a <see cref="Memory{T}"/> over the entire array.
    /// </summary>
    // this is fine since the array is already pinned by the time callers can get in here
    public readonly Memory<T> Memory => new Memory<T>(Unsafe.As<T[]>(_array));

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
    /// Initializes a new <see cref="MultiDimArrayEnumerable{T}"/> with the specified <paramref name="array"/>.
    /// </summary>
    /// <param name="array">The array to enumerate.</param>
    public MultiDimArrayEnumerable(Array array)
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

    /// <summary>
    /// Implicitly creates an <see cref="MultiDimArrayEnumerable{T}"/> from an <see cref="Array"/>.
    /// </summary>
    /// <param name="array">The array to wrap.</param>
    public static implicit operator MultiDimArrayEnumerable<T>(Array array) => new MultiDimArrayEnumerable<T>(array);

    // there are no implicit conversions to Span<T> or Memory<T> to discourage "throwaway use"
    // not disposing instances of this type means serious resource leaks and performance impacts since the pinned array can't be moved
}
