using System;
using System.Collections;
using System.IO;

namespace LaquaiLib.Util;

/// <summary>
/// Represents a temporary array of <typeparamref name="T"/> that is automatically cleared from memory when its wrapper object is disposed.
/// </summary>
public class TempArray<T> : ICloneable, IList, IStructuralComparable, IStructuralEquatable, IDisposable
{
    /// <summary>
    /// Instantiates a new <see cref="TempArray{T}"/> with the given size.
    /// </summary>
    public TempArray(int capacity)
    {
        _array = new T[capacity];
    }

    /// <summary>
    /// Instantiates a new <see cref="TempArray{T}"/> as a wrapper around the specified array of <typeparamref name="T"/>.
    /// </summary>
    /// <param name="array">The array of <typeparamref name="T"/> to wrap with this <see cref="TempArray{T}"/>.</param>
    public TempArray(T[] array)
    {
        _array = array;
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

    /// <summary>
    /// The array of <typeparamref name="T"/> this <see cref="TempArray{T}"/> wraps.
    /// </summary>
    public T[] Array {
        get {
            ObjectDisposedException.ThrowIf(_array is null, _array);
            return _array;
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
                _array = null;
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
