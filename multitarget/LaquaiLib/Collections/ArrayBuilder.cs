using LaquaiLib.Extensions;

namespace LaquaiLib.Collections;

/// <summary>
/// Effectively a wrapper around a <see cref="List{T}"/> that allows zero-copy array production when the list is at full capacity.
/// </summary>
/// <typeparam name="T"></typeparam>
public struct ArrayBuilder<T> : IList<T>
{
    private static class Accessors<T1>
    {
        [UnsafeAccessor(UnsafeAccessorKind.Field)] public static extern ref T1[] _items(List<T1> _);
    }

    private List<T> _items;
    private int _expectedCapacity;

    private readonly Span<T> Span
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => CollectionsMarshal.AsSpan(_items);
    }

    public ArrayBuilder(int expectedCapacity)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(expectedCapacity);
        Reseed(expectedCapacity);
    }
    public ArrayBuilder(int expectedCapacity, IEnumerable<T> items)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(expectedCapacity);
        Reseed(expectedCapacity);
        AddRange(items);
        // Overwrite the specified capacity if the collection ended up making the storage resize
        _expectedCapacity = _items.Capacity; 
    }
    public ArrayBuilder(int expectedCapacity, params ReadOnlySpan<T> items)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(expectedCapacity);
        ArgumentOutOfRangeException.ThrowIfLessThan(expectedCapacity, items.Length);
        Reseed(_expectedCapacity);
        AddRange(items);
    }

    /// <summary>
    /// Gets whether <see cref="MoveToArray(bool)"/> can be called without throwing an exception (that is, whether the list is currently at full capacity so that the internal array can be stolen without copying).
    /// </summary>
    public readonly bool CanMoveToArray => _items.Count == _items.Capacity;
    /// <summary>
    /// Drains the builder into an array. If at full capacity, the internal array will be stolen without copying; otherwise, a new array will be allocated and filled with the contents of the builder.
    /// </summary>
    /// <param name="reseed">Whether to reseed the builder with a new array of the expected capacity after draining. If <see langword="false"/>, the builder becomes unusable after draining.</param>
    /// <returns>The array containing the drained contents of the builder.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public T[] DrainToArray(bool reseed = false) => EvictImpl(reseed, false);
    /// <summary>
    /// Returns the internal array of the builder, stealing it from the builder without copying. Throws <see cref="InvalidOperationException"/> if the list is not currently at full capacity.
    /// </summary>
    /// <param name="reseed">Whether to reseed the builder with a new array of the expected capacity after draining. If <see langword="false"/>, the builder becomes unusable after draining (it does not, however, if the move couldn't be completed).</param>
    /// <returns>The array containing the contents of the builder.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public T[] MoveToArray(bool reseed = false) => EvictImpl(reseed, true);
    private T[] EvictImpl(bool reseed, bool throwIfCopyNeeded)
    {
        if (_items is null)
            throw new ObjectDisposedException(nameof(ArrayBuilder<>));

        T[] ret;
        if (_items.Count == 0)
            ret = [];
        else if (_items.Count == _items.Capacity)
            ret = Accessors<T>._items(_items);
        else if (throwIfCopyNeeded)
            throw new InvalidOperationException($"Cannot move to array when the list is not at full capacity (length: {_items.Count}, capacity: {_items.Capacity}).");
        else
            ret = ToArray();
        if (reseed)
            _items = [with(_expectedCapacity)];
        else
            _items = null;
        return ret;
    }
    /// <summary>
    /// Copies the contents of the builder into a new array. The builder remains valid and usable after this operation.
    /// </summary>
    /// <returns>The array containing the contents of the builder.</returns>
    public readonly T[] ToArray()
    {
        if (_items is null)
            throw new ObjectDisposedException(nameof(ArrayBuilder<>));
        var arr = new T[_items.Count];
        Span.CopyTo(arr);
        return arr;
    }
    /// <summary>
    /// Discards the current contents of the builder and assigns a new array of the specified expected capacity as the internal storage.
    /// </summary>
    public void Reseed(int expectedCapacity)
    {
        _expectedCapacity = expectedCapacity;
        _items = [with(expectedCapacity)];
    }

    #region IList<T>
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public int IndexOf(T item) => (_items ?? throw new ObjectDisposedException(nameof(ArrayBuilder<>))).IndexOf(item);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public void Insert(int index, T item) => (_items ?? throw new ObjectDisposedException(nameof(ArrayBuilder<>))).Insert(index, item);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public void RemoveAt(int index) => (_items ?? throw new ObjectDisposedException(nameof(ArrayBuilder<>))).RemoveAt(index);

    public readonly T this[int index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => (_items ?? throw new ObjectDisposedException(nameof(ArrayBuilder<>)))[index];
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => (_items ?? throw new ObjectDisposedException(nameof(ArrayBuilder<>)))[index] = value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)] public void Add(T item) => (_items ?? throw new ObjectDisposedException(nameof(ArrayBuilder<>))).Add(item);
    public void AddRange(IEnumerable<T> items)
    {
        if (_items is null)
            throw new ObjectDisposedException(nameof(ArrayBuilder<>));
        if (items is null)
            return;
        switch (items)
        {
            case List<T> list:
                AddRange(list.AsSpan());
                break;
            case T[] array:
                AddRange(array.AsSpan());
                break;
            case IReadOnlyCollection<T> coll:
                _items.EnsureCapacity(_items.Count + coll.Count);
                ref var slot = ref MemoryMarshal.GetReference(Span);
                foreach (var item in coll)
                {
                    slot = item;
                    slot = ref Unsafe.Add(ref slot, 1);
                }
                break;
            default:
                foreach (var item in items)
                    _items.Add(item);
                break;
        }
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddRange(ReadOnlySpan<T> items)
    {
        if (_items is null)
            throw new ObjectDisposedException(nameof(ArrayBuilder<>));
        if (items.IsEmpty)
            return;
        CollectionsMarshal.SetCount(_items, _items.Count + items.Length);
        items.CopyTo(Span[_items.Count..]);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public void Clear() => (_items ?? throw new ObjectDisposedException(nameof(ArrayBuilder<>))).Clear();
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public bool Contains(T item) => (_items ?? throw new ObjectDisposedException(nameof(ArrayBuilder<>))).Contains(item);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public void CopyTo(T[] array, int arrayIndex) => (_items ?? throw new ObjectDisposedException(nameof(ArrayBuilder<>))).CopyTo(array, arrayIndex);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public bool Remove(T item) => (_items ?? throw new ObjectDisposedException(nameof(ArrayBuilder<>))).Remove(item);

    public int Count
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => (_items ?? throw new ObjectDisposedException(nameof(ArrayBuilder<>))).Count;
    }
    public bool IsReadOnly
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _items is null;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public readonly IEnumerator<T> GetEnumerator() => (_items ?? throw new ObjectDisposedException(nameof(ArrayBuilder<>))).GetEnumerator();
    [MethodImpl(MethodImplOptions.AggressiveInlining)] readonly IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    #endregion
}
