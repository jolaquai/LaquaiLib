using LaquaiLib.Interfaces;

namespace LaquaiLib.Collections.Enumeration;

#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type

/// <summary>
/// Implements the enumerator pattern to enumerate the elements of a (potentially) multidimensional array sequentially.
/// </summary>
/// <typeparam name="T">The type of the elements in the array. This must be exactly the same type as the array, otherwise users of this type will be faced with non-sensical exceptions.</typeparam>
public unsafe class MultiDimArrayEnumerable<T> : IEnumerable<T>, ISpanProvider<T>
{
    private readonly Array _array;
    private readonly int _length;
    private readonly GCHandle _handle;
    private readonly T* _start;

    /// <summary>
    /// Gets a <see cref="Span{T}"/> over the entire array.
    /// </summary>
    public Span<T> Span => new Span<T>(_start, _length);

    /// <summary>
    /// Enables enumeration of the items of the <see cref="Array"/> a <see cref="MultiDimArrayEnumerable{T}"/> pins.
    /// </summary>
    public struct Enumerator : IEnumerator<T>
    {
        private readonly MultiDimArrayEnumerable<T> _parent;
        private readonly T* _start, _end;
        private T* cur;
        internal Enumerator(MultiDimArrayEnumerable<T> parent)
        {
            _start = parent._start;
            _end = parent._start + parent._length;
            cur = _start - 1; // Start before the first element
            _parent = parent;
        }

        /// <inheritdoc/>
        public bool MoveNext() => ++cur < _end;
        /// <inheritdoc/>
        public void Reset() => cur = _start - 1;
        /// <inheritdoc/>
        public T Current => _parent.disposed == 0 ? *cur : throw new ObjectDisposedException(nameof(MultiDimArrayEnumerable<>));
        object IEnumerator.Current => Current;
        /// <inheritdoc/>
        public void Dispose() { }
    }

    /// <summary>
    /// Initializes a new <see cref="MultiDimArrayEnumerable{T}"/> with the specified <paramref name="array"/>.
    /// </summary>
    /// <param name="array">The array to enumerate.</param>
    public MultiDimArrayEnumerable(Array array)
    {
        _array = array;
        _handle = GCHandle.Alloc(array, GCHandleType.Pinned);
        // The array is pinned here already so this is safe
        _start = (T*)Unsafe.AsPointer(ref Unsafe.As<byte, T>(ref MemoryMarshal.GetArrayDataReference(array)));
        _length = array.Length;
    }

    /// <inheritdoc/>
    public IEnumerator<T> GetEnumerator() => new Enumerator(this);
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private volatile byte disposed;
    ~MultiDimArrayEnumerable() => Dispose();
    /// <summary>
    /// Frees the <see cref="GCHandle"/> used to pin the target array.
    /// </summary>
    public void Dispose()
    {
        if (Interlocked.Exchange(ref disposed, 1) == 0)
        {
            _handle.Free();
            GC.SuppressFinalize(this);
        }
    }
}
