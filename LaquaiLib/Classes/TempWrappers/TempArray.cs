using System;
using System.Buffers;
using System.Collections;
using System.IO;

namespace LaquaiLib.Util;

/// <summary>
/// Represents a temporary array of <typeparamref name="T"/> that is automatically cleared from memory when its wrapper object is disposed.
/// </summary>
public class TempArray<T> : ICloneable, IList, IStructuralComparable, IStructuralEquatable, IDisposable
{
    /// <summary>
    /// Instantiates a new <see cref="TempArray{T}"/> with the given size. It is automatically rented from and returned to <see cref="ArrayPool{T}.Shared"/> upon disposal of this <see cref="TempArray{T}"/>.
    /// </summary>
    public TempArray(int capacity)
    {
        _array = ArrayPool<T>.Shared.Rent(capacity);
        _pool = ArrayPool<T>.Shared;
        _isPooledInstance = _pool is not null;
    }

    /// <summary>
    /// Instantiates a new <see cref="TempArray{T}"/> as a wrapper around the specified array of <typeparamref name="T"/>.
    /// </summary>
    /// <param name="array">The array of <typeparamref name="T"/> to wrap with this <see cref="TempArray{T}"/>.</param>
    /// <param name="arrayPool">The <see cref="ArrayPool{T}"/> to return the array to when this <see cref="TempArray{T}"/> is disposed. May be <c>null</c> to indicate that the passed <paramref name="array"/> is not from any <see cref="ArrayPool{T}"/>.
    public TempArray(T[] array, ArrayPool<T>? arrayPool = null)
    {
        _array = array;
        _pool = arrayPool;
        _isPooledInstance = _pool is not null;
    }

    /// <summary>
    /// Instantiates a new <see cref="TempArray{T}"/> with the given <paramref name="size"/> and initializes all elements with the given <paramref name="value"/>.
    /// </summary>
    /// <param name="value">The value to initialize all elements with.</param>
    /// <param name="size">The size of the array to create.</param>
    public TempArray(T value, int size)
    {
        _array = new T[size];
        for (int i = 0; i < size; i++)
        {
            _array[i] = value;
        }
    }

    private T[] _array;
    private bool _isPooledInstance;
    private ArrayPool<T>? _pool;

    /// <summary>
    /// The array of <typeparamref name="T"/> this <see cref="TempArray{T}"/> wraps.
    /// </summary>
    public T[] Array {
        get {
            ObjectDisposedException.ThrowIf(_array is null, _array);
            return _array;
        }
        set {
            ArgumentNullException.ThrowIfNull(value);
            if (_isPooledInstance)
            {
                throw new NotSupportedException($"Pooled array instances may not be directly overwritten.");
            }
            _array = value;
        }
    }
    /// <summary>
    /// Whether this <see cref="TempArray{T}"/> has been disposed.
    /// </summary>
    public bool IsDisposed => _array is null;

    /// <inheritdoc/>
    public bool IsFixedSize => _array.IsFixedSize;
    /// <inheritdoc/>
    public bool IsReadOnly => _array.IsReadOnly;
    /// <inheritdoc/>
    public int Count => ((ICollection)_array).Count;
    /// <inheritdoc/>
    public bool IsSynchronized => _array.IsSynchronized;
    /// <inheritdoc/>
    public object SyncRoot => _array.SyncRoot;
    /// <inheritdoc/>
    public object this[int index] { get => ((IList)_array)[index]; set => ((IList)_array)[index] = value; }
    /// <inheritdoc/>
    public bool Equals(object? other, IEqualityComparer comparer) => ((IStructuralEquatable)_array).Equals(other, comparer);
    /// <inheritdoc/>
    public int GetHashCode(IEqualityComparer comparer) => ((IStructuralEquatable)_array).GetHashCode(comparer);
    /// <inheritdoc/>
    public int CompareTo(object? other, IComparer comparer) => ((IStructuralComparable)_array).CompareTo(other, comparer);
    /// <inheritdoc/>
    public int Add(object? value) => ((IList)_array).Add(value);
    /// <inheritdoc/>
    public void Clear() => ((IList)_array).Clear();
    /// <inheritdoc/>
    public bool Contains(object? value) => ((IList)_array).Contains(value);
    /// <inheritdoc/>
    public int IndexOf(object? value) => ((IList)_array).IndexOf(value);
    /// <inheritdoc/>
    public void Insert(int index, object? value) => ((IList)_array).Insert(index, value);
    /// <inheritdoc/>
    public void Remove(object? value) => ((IList)_array).Remove(value);
    /// <inheritdoc/>
    public void RemoveAt(int index) => ((IList)_array).RemoveAt(index);
    /// <inheritdoc/>
    public void CopyTo(Array array, int index) => _array.CopyTo(array, index);
    /// <inheritdoc/>
    public IEnumerator GetEnumerator() => _array.GetEnumerator();
    /// <inheritdoc/>
    public object Clone() => _array.Clone();

    #region Dispose pattern
    private void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (_array is not null)
            {
                for (int i = 0; i < _array.Length; i++)
                {
                    IDisposable disposable = _array[i] as IDisposable;
                    disposable?.Dispose();
                }
                _pool.Return(_array);
                GC.Collect();
            }
        }
    }

    ~TempArray()
    {
        Dispose(false);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        GC.SuppressFinalize(this);
        Dispose(true);
    }
    #endregion
}
