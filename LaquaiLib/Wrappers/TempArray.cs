using System.Buffers;
using System.Collections;

namespace LaquaiLib.Wrappers;

/// <summary>
/// Represents a temporary array of <typeparamref name="T"/> that is automatically cleared from memory when its wrapper object is disposed.
/// </summary>
public ref struct TempArray<T> : ICloneable, IStructuralComparable, IStructuralEquatable, IDisposable
{
    /// <summary>
    /// Initializes a new <see cref="TempArray{T}"/> with the given size. It is automatically rented from and returned to <see cref="ArrayPool{T}.Shared"/> upon disposal of this <see cref="TempArray{T}"/>, unless <see langword="false"/> is explicitly passed for <paramref name="allowPooledArray"/>.
    /// </summary>
    /// <param name="capacity">The capacity of the array to create.</param>
    /// <param name="allowPooledArray">Whether to allow the array to be rented from <see cref="ArrayPool{T}.Shared"/>. If <see langword="false"/>, a new array will be created instead.</param>
    public TempArray(int capacity, bool allowPooledArray = true)
    {
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
        _array = new T[size];
        for (var i = 0; i < size; i++)
        {
            _array[i] = value;
        }
    }

    private T[] _array;
    private readonly bool _isPooledInstance;
    private readonly ArrayPool<T> _pool;

    /// <summary>
    /// The array of <typeparamref name="T"/> this <see cref="TempArray{T}"/> wraps.
    /// </summary>
    public T[] Array
    {
        get
        {
            ObjectDisposedException.ThrowIf(IsDisposed, _array!);
            return _array!;
        }
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            if (_isPooledInstance)
            {
                throw new NotSupportedException("Pooled array instances may not be directly overwritten.");
            }
            _array = value;
        }
    }

    /// <summary>
    /// Whether this <see cref="TempArray{T}"/> has been disposed.
    /// </summary>
    public bool IsDisposed => _array is null;

    /// <inheritdoc/>
    public bool IsFixedSize => _array!.IsFixedSize;
    /// <inheritdoc/>
    public bool IsReadOnly => _array!.IsReadOnly;
    /// <inheritdoc/>
    public int Count => _array!.Length;
    /// <inheritdoc/>
    public bool IsSynchronized => _array!.IsSynchronized;
    /// <inheritdoc/>
    public object SyncRoot => _array!.SyncRoot;
    /// <inheritdoc/>
    public T this[int index] { get => (T)((IList)_array!)[index]; set => ((IList)_array!)[index] = value; }
    /// <inheritdoc/>
    public bool Equals(object other, IEqualityComparer comparer) => ((IStructuralEquatable)_array!).Equals(other, comparer);
    /// <inheritdoc/>
    public int GetHashCode(IEqualityComparer comparer) => ((IStructuralEquatable)_array!).GetHashCode(comparer);
    /// <inheritdoc/>
    public int CompareTo(object other, IComparer comparer) => ((IStructuralComparable)_array!).CompareTo(other, comparer);
    /// <inheritdoc/>
    public int Add(object value) => ((IList)_array!).Add(value);
    /// <inheritdoc/>
    public void Clear() => ((IList)_array!).Clear();
    /// <inheritdoc/>
    public bool Contains(object value) => ((IList)_array!).Contains(value);
    /// <inheritdoc/>
    public int IndexOf(object value) => ((IList)_array!).IndexOf(value);
    /// <inheritdoc/>
    public void Insert(int index, object value) => ((IList)_array!).Insert(index, value);
    /// <inheritdoc/>
    public void Remove(object value) => ((IList)_array!).Remove(value);
    /// <inheritdoc/>
    public void RemoveAt(int index) => ((IList)_array!).RemoveAt(index);
    /// <inheritdoc/>
    public void CopyTo(Array array, int index) => _array!.CopyTo(array, index);
    /// <summary>
    /// Copies the elements of this <see cref="TempArray{T}"/> to another <see cref="TempArray{T}"/>, starting at a particular index.
    /// </summary>
    /// <param name="tempArray">The <see cref="TempArray{T}"/> that is the destination of the elements copied from this <see cref="TempArray{T}"/>.</param>
    /// <param name="index">The zero-based index in array at which copying begins.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="tempArray"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="index"/> is less than zero.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="tempArray"/> is multi-dimensional, the number of elements in this <see cref="TempArray{T}"/> is greater than the available space from <paramref name="index"/> to the end of the destination <see cref="TempArray{T}"/> or the type of the source <see cref="TempArray{T}"/> cannot be cast automatically to the type of the destination <see cref="TempArray{T}"/>.</exception>
    public void CopyTo(TempArray<T> tempArray, int index) => _array!.CopyTo(tempArray._array!, index);
    /// <inheritdoc/>
    public IEnumerator GetEnumerator() => _array!.GetEnumerator();
    /// <inheritdoc/>
    public object Clone() => _array!.Clone();

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
}
