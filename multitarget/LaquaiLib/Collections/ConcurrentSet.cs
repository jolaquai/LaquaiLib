#pragma warning disable IDE0058 // Expression value is never used

namespace LaquaiLib.Collections;

/// <summary>
/// Implements a thread-safe <see cref="ISet{T}"/>.
/// </summary>
public sealed class ConcurrentSet<T> : ISet<T> where T : notnull
{
    private const int DefaultCapacity = 16;

    private struct Bucket
    {
        public T Item;
        public volatile int State; // 0=empty, 1=occupied, -1=deleted
        public int Hash;
    }

    private volatile Bucket[] _buckets;
    private volatile int _count;
    private volatile int _version;
    private readonly IEqualityComparer<T> _comparer;

    /// <summary>
    /// Initializes a new <see cref="ConcurrentSet{T}"/> with the default initial capacity and comparer.
    /// </summary>
    public ConcurrentSet() : this(DefaultCapacity, EqualityComparer<T>.Default) { }
    /// <summary>
    /// Initializes a new <see cref="ConcurrentSet{T}"/> with the specified initial capacity and the default comparer.
    /// </summary>
    /// <param name="capacity">The initial capacity of the set.</param>
    public ConcurrentSet(int capacity) : this(capacity, EqualityComparer<T>.Default) { }
    /// <summary>
    /// Initializes a new <see cref="ConcurrentSet{T}"/> with the specified comparer and the default initial capacity.
    /// </summary>
    /// <param name="comparer">The equality comparer to use for the set.</param>
    public ConcurrentSet(IEqualityComparer<T> comparer) : this(DefaultCapacity, comparer) { }
    /// <summary>
    /// Initializes a new <see cref="ConcurrentSet{T}"/> with the specified initial capacity and comparer.
    /// </summary>
    /// <param name="capacity">The initial capacity of the set.</param>
    /// <param name="comparer">The equality comparer to use for the set.</param>
    public ConcurrentSet(int capacity, IEqualityComparer<T> comparer)
    {
        var size = GetNextPowerOfTwo(Math.Max(capacity, DefaultCapacity));
        _buckets = new Bucket[size];
        _comparer = comparer ?? EqualityComparer<T>.Default;
    }

    /// <summary>
    /// Gets the number of elements in the set.
    /// </summary>
    public int Count => _count;
    /// <summary>
    /// Gets whether the set is read-only.
    /// </summary>
    public bool IsReadOnly => false;

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Add(T item)
    {
        var hash = _comparer.GetHashCode(item);
        var buckets = _buckets;
        var mask = buckets.Length - 1;
        var index = hash & mask;

        while (true)
        {
            ref var bucket = ref buckets[index];
            var state = bucket.State;

            if (state is 0 or (-1))
            {
                if (Interlocked.CompareExchange(ref bucket.State, 1, state) == state)
                {
                    bucket.Item = item;
                    bucket.Hash = hash;
                    Interlocked.Increment(ref _count);
                    Interlocked.Increment(ref _version);
                    return true;
                }
            }
            else if (bucket.Hash == hash && _comparer.Equals(bucket.Item, item))
            {
                return false;
            }

            index = (index + 1) & mask;
        }
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(T item)
    {
        var hash = _comparer.GetHashCode(item);
        var buckets = _buckets;
        var mask = buckets.Length - 1;
        var index = hash & mask;

        while (true)
        {
            ref var bucket = ref buckets[index];
            var state = bucket.State;

            if (state == 0)
            {
                return false;
            }

            if (state == 1 && bucket.Hash == hash && _comparer.Equals(bucket.Item, item))
            {
                return true;
            }

            index = (index + 1) & mask;
        }
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Remove(T item)
    {
        var hash = _comparer.GetHashCode(item);
        var buckets = _buckets;
        var mask = buckets.Length - 1;
        var index = hash & mask;

        while (true)
        {
            ref var bucket = ref buckets[index];
            var state = bucket.State;

            if (state == 0)
            {
                return false;
            }

            if (state == 1 && bucket.Hash == hash && _comparer.Equals(bucket.Item, item))
            {
                if (Interlocked.CompareExchange(ref bucket.State, -1, 1) == 1)
                {
                    bucket.Item = default!;
                    Interlocked.Decrement(ref _count);
                    Interlocked.Increment(ref _version);
                    return true;
                }
            }

            index = (index + 1) & mask;
        }
    }

    /// <inheritdoc/>
    public void Clear()
    {
        _buckets = new Bucket[_buckets.Length];
        _count = 0;
        Interlocked.Increment(ref _version);
    }

    /// <inheritdoc/>
    public void CopyTo(T[] array, int arrayIndex)
    {
        var buckets = _buckets;
        var copied = 0;

        for (var i = 0; i < buckets.Length && copied < _count; i++)
        {
            if (buckets[i].State == 1)
            {
                array[arrayIndex + copied++] = buckets[i].Item;
            }
        }
    }

    /// <inheritdoc/>
    public void ExceptWith(IEnumerable<T> other)
    {
        foreach (var item in other)
        {
            Remove(item);
        }
    }

    /// <inheritdoc/>
    public void IntersectWith(IEnumerable<T> other)
    {
        var otherSet = new HashSet<T>(other, _comparer);
        var buckets = _buckets;

        for (var i = 0; i < buckets.Length; i++)
        {
            if (buckets[i].State == 1 && !otherSet.Contains(buckets[i].Item))
            {
                Remove(buckets[i].Item);
            }
        }
    }

    /// <inheritdoc/>
    public void UnionWith(IEnumerable<T> other)
    {
        foreach (var item in other)
        {
            Add(item);
        }
    }

    /// <inheritdoc/>
    public void SymmetricExceptWith(IEnumerable<T> other)
    {
        foreach (var item in other)
        {
            if (!Remove(item))
            {
                Add(item);
            }
        }
    }

    /// <inheritdoc/>
    public bool IsSubsetOf(IEnumerable<T> other)
    {
        var otherSet = new HashSet<T>(other, _comparer);
        var buckets = _buckets;

        for (var i = 0; i < buckets.Length; i++)
        {
            if (buckets[i].State == 1 && !otherSet.Contains(buckets[i].Item))
            {
                return false;
            }
        }
        return true;
    }

    /// <inheritdoc/>
    public bool IsSupersetOf(IEnumerable<T> other)
    {
        foreach (var item in other)
        {
            if (!Contains(item))
            {
                return false;
            }
        }
        return true;
    }

    /// <inheritdoc/>
    public bool IsProperSubsetOf(IEnumerable<T> other)
    {
        var otherSet = new HashSet<T>(other, _comparer);
        return Count < otherSet.Count && IsSubsetOf(otherSet);
    }

    /// <inheritdoc/>
    public bool IsProperSupersetOf(IEnumerable<T> other)
    {
        var otherSet = new HashSet<T>(other, _comparer);
        return Count > otherSet.Count && IsSupersetOf(otherSet);
    }

    /// <inheritdoc/>
    public bool Overlaps(IEnumerable<T> other)
    {
        foreach (var item in other)
        {
            if (Contains(item))
            {
                return true;
            }
        }
        return false;
    }

    /// <inheritdoc/>
    public bool SetEquals(IEnumerable<T> other)
    {
        var otherSet = new HashSet<T>(other, _comparer);
        return Count == otherSet.Count && IsSubsetOf(otherSet);
    }

    /// <inheritdoc/>
    public IEnumerator<T> GetEnumerator()
    {
        var buckets = _buckets;
        var version = _version;

        for (var i = 0; i < buckets.Length; i++)
        {
            if (_version != version)
            {
                throw new InvalidOperationException("Collection was modified during enumeration.");
            }

            if (buckets[i].State == 1)
            {
                yield return buckets[i].Item;
            }
        }
    }

    void ICollection<T>.Add(T item) => Add(item);
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int GetNextPowerOfTwo(int value)
    {
        value--;
        value |= value >> 1;
        value |= value >> 2;
        value |= value >> 4;
        value |= value >> 8;
        value |= value >> 16;
        return value + 1;
    }
}
