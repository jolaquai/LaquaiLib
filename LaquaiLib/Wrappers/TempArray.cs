using System.Buffers;

namespace LaquaiLib.Wrappers;

/// <summary>
/// Represents a temporary array of <typeparamref name="T"/> that is automatically cleared from memory when its wrapper object is disposed.
/// </summary>
public ref struct TempArray<T> : IDisposable
{
    /// <summary>
    /// Initializes a new <see cref="TempArray{T}"/> with the given size. It is automatically rented from and returned to <see cref="ArrayPool{T}.Shared"/> upon disposal of this <see cref="TempArray{T}"/>, unless <see langword="false"/> is explicitly passed for <paramref name="allowPooledArray"/>.
    /// </summary>
    /// <param name="capacity">The capacity of the array to create.</param>
    /// <param name="allowPooledArray">Whether to allow the array to be rented from <see cref="ArrayPool{T}.Shared"/>. If <see langword="false"/>, a new array of exactly the specified size is allocated instead.</param>
    public TempArray(int capacity, bool allowPooledArray = true)
    {
        _requestedSize = capacity;
        if (allowPooledArray)
        {
            _array = ArrayPool<T>.Shared.Rent(capacity);
            _pool = ArrayPool<T>.Shared;
        }
        else
        {
            _array = new T[capacity];
            _pool = null;
        }
        _isPooledInstance = _pool is not null;
    }
    /// <summary>
    /// Initializes a new <see cref="TempArray{T}"/> as a wrapper around the specified array of <typeparamref name="T"/>.
    /// </summary>
    /// <param name="array">The array of <typeparamref name="T"/> to wrap with this <see cref="TempArray{T}"/>.</param>
    /// <param name="arrayPool">The <see cref="ArrayPool{T}"/> to return the array to when this <see cref="TempArray{T}"/> is disposed. May be <see langword="null"/> to indicate that the passed <paramref name="array"/> is not from any <see cref="ArrayPool{T}"/>.</param>
    public TempArray(T[] array, ArrayPool<T> arrayPool = null)
    {
        _requestedSize = array.Length;
        _array = array;
        _pool = arrayPool;
        _isPooledInstance = _pool is not null;
    }
    /// <summary>
    /// Initializes a new <see cref="TempArray{T}"/> with the given <paramref name="size"/> and initializes all elements with the given <paramref name="value"/>.
    /// </summary>
    /// <param name="value">The value to initialize all elements with. The assignment is shallow; if <typeparamref name="T"/> is a reference type, all elements will reference the same object.</param>
    /// <param name="size">The size of the array to create.</param>
    public TempArray(T value, int size)
    {
        _requestedSize = size;
        _array = new T[size];
        for (var i = 0; i < size; i++)
        {
            _array[i] = value;
        }
    }
    /// <summary>
    /// Initializes a new <see cref="TempArray{T}"/> with the elements of the specified <paramref name="items"/> collection. The array is rented from the specified <paramref name="arrayPool"/> or <see cref="ArrayPool{T}.Shared"/> if <see langword="null"/>.
    /// </summary>
    /// <param name="items">The collection of elements to initialize the array with.</param>
    /// <param name="arrayPool">The <see cref="ArrayPool{T}"/> to rent the array from. May be <see langword="null"/> to rent from <see cref="ArrayPool{T}.Shared"/>.</param>
    public TempArray(IEnumerable<T> items, ArrayPool<T> arrayPool = null)
    {
        var renter = new ArrayRenter(items, arrayPool ?? ArrayPool<T>.Shared);
        _requestedSize = renter.Count;
        _array = renter.Array;
        _pool = arrayPool;
        _isPooledInstance = _pool is not null;
    }

    private readonly int _requestedSize;
    private T[] _array;
    private readonly bool _isPooledInstance;
    private readonly ArrayPool<T> _pool;

    /// <summary>
    /// Gets a <see cref="Span{T}"/> around the backing array of this <see cref="TempArray{T}"/>.
    /// </summary>
    public readonly Span<T> Span
    {
        get
        {
            ObjectDisposedException.ThrowIf(IsDisposed, _array);
            return _array.AsSpan(0, _requestedSize);
        }
    }

    /// <summary>
    /// Whether this <see cref="TempArray{T}"/> has been disposed.
    /// </summary>
    public readonly bool IsDisposed => _array is null;

    #region Dispose pattern
    /// <inheritdoc/>
    public void Dispose()
    {
        if (_array is not null)
        {
            for (var i = 0; i < _array.Length; i++)
            {
                var disposable = _array[i] as IDisposable;
                disposable?.Dispose();
            }
            if (_isPooledInstance)
            {
                _pool!.Return(_array, true);
            }
            _array = null;
        }
    }
    #endregion

    // Helper struct that rents its backing store(s) from ArrayPool<T>.Shared, growing like a normal List<T> until it has exhaused the input IEnumerable<T>
    // The current array is then returned; the caller is then responsible for returning it to the pool
    internal readonly struct ArrayRenter
    {
        public int Count { get; }
        public T[] Array { get; }

        public ArrayRenter(IEnumerable<T> items, ArrayPool<T> pool)
        {
            // Cheap path for known collections
            if (items is ICollection<T> coll)
            {
                Array = pool.Rent(coll.Count);
                Count = coll.Count;
                coll.CopyTo(Array, 0);
                return;
            }

            Array = pool.Rent(2);
            Count = 0;
            foreach (var item in items)
            {
                if (Count == Array.Length)
                {
                    var newArray = pool.Rent((Array.Length - 1) << 1);
                    Array.CopyTo(newArray);
                    pool.Return(Array);
                    Array = newArray;
                }
                Array[Count++] = item;
            }
        }
    }
}
