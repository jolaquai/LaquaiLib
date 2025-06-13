namespace LaquaiLib.Collections;

/// <summary>
/// Implements a concurrent (thread-safe) version of <see cref="TwoWayLookup{T1, T2}"/>.
/// </summary>
/// <remarks>
/// This type is thread-safe.
/// </remarks>
public class ConcurrentTwoWayLookup<T1, T2> : IEnumerable<KeyValuePair<T1, T2>>
    where T1 : notnull
    where T2 : notnull
{
    private readonly ConcurrentDictionary<T1, T2> _forward = [];
    private readonly ConcurrentDictionary<T2, T1> _reverse = [];

    #region Locking
    private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] internal ReadLock TakeReadLock() => new ReadLock(_lock);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] internal WriteLock TakeWriteLock() => new WriteLock(_lock);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] internal UpgradeableReadLock TakeUpgradeableReadLock() => new UpgradeableReadLock(this);
    internal readonly struct ReadLock : IDisposable
    {
        private readonly ReaderWriterLockSlim _lock;
        public ReadLock(ReaderWriterLockSlim rwLock)
        {
            _lock = rwLock;
            _lock.EnterReadLock();
        }
        public readonly void Dispose() => _lock.ExitReadLock();
    }
    internal readonly struct WriteLock : IDisposable
    {
        private readonly ReaderWriterLockSlim _lock;
        public WriteLock(ReaderWriterLockSlim rwLock)
        {
            _lock = rwLock;
            _lock.EnterWriteLock();
        }
        public readonly void Dispose() => _lock.ExitWriteLock();
    }
    internal readonly struct UpgradeableReadLock : IDisposable
    {
        private readonly ReaderWriterLockSlim _lock;
        private readonly ConcurrentTwoWayLookup<T1, T2> _lookup;

        public UpgradeableReadLock(ConcurrentTwoWayLookup<T1, T2> lookup)
        {
            _lookup = lookup;
            _lock = lookup._lock;

            _lock.EnterUpgradeableReadLock();
        }
        public readonly void Dispose() => _lock.ExitUpgradeableReadLock();
        public WriteLock Upgrade() => _lookup.TakeWriteLock();
    }
    #endregion

    /// <summary>
    /// Gets the number of key-value pairs in the lookup table.
    /// </summary>
    public int Count
    {
        get
        {
            using var rLock = TakeReadLock();
            return _forward.Count;
        }
    }

    /// <summary>
    /// Adds a new entry to the lookup table by the first type parameter <typeparamref name="T1"/>. An exception is thrown if either the key or the value already exists.
    /// </summary>
    /// <param name="key">The key of the entry.</param>
    /// <param name="value">The value of the entry.</param>
    public void AddForward(T1 key, T2 value)
    {
        using var wLock = TakeWriteLock();

        if (_forward.ContainsKey(key))
        {
            throw new ArgumentException($"The key '{key}' already exists in the lookup table.", nameof(key));
        }

        if (_reverse.ContainsKey(value))
        {
            throw new ArgumentException($"The value '{value}' already exists in the lookup table.", nameof(value));
        }

        _forward[key] = value;
        _reverse[value] = key;
    }
    /// <summary>
    /// Adds a new entry to the lookup table by the second type parameter <typeparamref name="T2"/>. An exception is thrown if either the key or the value already exists.
    /// </summary>
    /// <param name="key">The key of the entry.</param>
    /// <param name="value">The value of the entry.</param>
    public void AddReverse(T2 key, T1 value)
    {
        using var wLock = TakeWriteLock();

        if (_reverse.ContainsKey(key))
        {
            throw new ArgumentException($"The key '{key}' already exists in the lookup table.", nameof(key));
        }

        if (_forward.ContainsKey(value))
        {
            throw new ArgumentException($"The value '{value}' already exists in the lookup table.", nameof(value));
        }

        _reverse[key] = value;
        _forward[value] = key;
    }
    /// <summary>
    /// Attempts to add a new entry to the lookup table by the first type parameter <typeparamref name="T1"/>.
    /// </summary>
    /// <param name="key">The key of the entry.</param>
    /// <param name="value">The value of the entry.</param>
    /// <returns><see langword="true"/> if the key-value pair could be added, otherwise <see langword="false"/>.</returns>
    public bool TryAddForward(T1 key, T2 value)
    {
        using var uLock = TakeUpgradeableReadLock();
        if (!_forward.ContainsKey(key))
        {
            using var wLock = uLock.Upgrade();
            if (_forward.TryAdd(key, value))
            {
                _ = _reverse.TryAdd(value, key);
                return true;
            }
        }
        return false;
    }
    /// <summary>
    /// Attempts to add a new entry to the lookup table by the second type parameter <typeparamref name="T2"/>.
    /// </summary>
    /// <param name="key">The key of the entry.</param>
    /// <param name="value">The value of the entry.</param>
    /// <returns><see langword="true"/> if the key-value pair could be added, otherwise <see langword="false"/>.</returns>
    public bool TryAddReverse(T2 key, T1 value)
    {
        using var uLock = TakeUpgradeableReadLock();
        if (!_reverse.ContainsKey(key))
        {
            using var wLock = uLock.Upgrade();
            if (_reverse.TryAdd(key, value))
            {
                _ = _forward.TryAdd(value, key);
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Adds a new entry to the lookup table.
    /// </summary>
    /// <param name="key">The key of the entry.</param>
    /// <param name="value">The value of the entry.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(T1 key, T2 value) => AddForward(key, value);
    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(T2 key, T1 value) => AddReverse(key, value);
    /// <summary>
    /// Attempts to add a new entry to the lookup table.
    /// </summary>
    /// <param name="key">The key of the entry.</param>
    /// <param name="value">The value of the entry.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryAdd(T1 key, T2 value) => TryAddForward(key, value);
    /// <inheritdoc cref="TryAdd(T1, T2)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryAdd(T2 key, T1 value) => TryAddReverse(key, value);

    /// <summary>
    /// Retrieves an entry from the lookup table by its key. An exception is thrown if there is no entry with the given key.
    /// </summary>
    /// <param name="key">The key of the entry.</param>
    /// <returns>The value associated with the given key.</returns>
    public T2 GetForward(T1 key)
    {
        using var rLock = TakeReadLock();
        return _forward[key];
    }
    /// <summary>
    /// Retrieves an entry from the lookup table by its value. An exception is thrown if there is no entry with the given value.
    /// </summary>
    /// <param name="value">The value of the entry.</param>
    /// <returns>The key associated with the given value.</returns>
    public T1 GetReverse(T2 value)
    {
        using var rLock = TakeReadLock();
        return _reverse[value];
    }
    /// <summary>
    /// Sets a key-value pair in the lookup table and returns <paramref name="value"/>.
    /// </summary>
    /// <param name="key">The key of the entry.</param>
    /// <param name="value">The value of the entry.</param>
    /// <returns>A reference to <paramref name="value"/>.</returns>
    public T2 SetForward(T1 key, T2 value)
    {
        using var wLock = TakeWriteLock();

        // Any previous pairs in either dictionary associated with either of the inputs are removed
        if (_forward.TryRemove(key, out var oldValue))
        {
            _ = _reverse.TryRemove(oldValue, out _);
        }
        if (_reverse.TryRemove(value, out var oldKey))
        {
            _ = _forward.TryRemove(oldKey, out _);
        }

        _forward[key] = value;
        _reverse[value] = key;
        return value;
    }
    /// <summary>
    /// Sets a key-value pair in the lookup table and returns <paramref name="value"/>.
    /// </summary>
    /// <param name="key">The key of the entry.</param>
    /// <param name="value">The value of the entry.</param>
    /// <returns>A reference to <paramref name="value"/>.</returns>
    public T1 SetReverse(T2 key, T1 value)
    {
        using var wLock = TakeWriteLock();

        // Same as above
        if (_reverse.TryRemove(key, out var oldKey))
        {
            _ = _forward.TryRemove(oldKey, out _);
        }
        if (_forward.TryRemove(value, out var oldValue))
        {
            _ = _reverse.TryRemove(oldValue, out _);
        }

        _reverse[key] = value;
        _forward[value] = key;
        return value;
    }
    /// <summary>
    /// Gets or sets an entry in the lookup table.
    /// </summary>
    /// <param name="key">The key of the entry.</param>
    /// <returns>The value associated with the given key.</returns>
    public T2 this[T1 key]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => GetForward(key);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => SetForward(key, value);
    }
    /// <summary>
    /// Gets or sets an entry in the lookup table.
    /// </summary>
    /// <param name="key">The value of the entry.</param>
    /// <returns>The key associated with the given value.</returns>
    public T1 this[T2 key]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => GetReverse(key);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => SetReverse(key, value);
    }
    /// <summary>
    /// Attempts to retrieve an entry from the lookup table by its key.
    /// </summary>
    /// <param name="key">The key of the entry.</param>
    /// <param name="value">An <c>out</c> <typeparamref name="T2"/> variable that receives the retrieved value.</param>
    /// <returns><see langword="true"/> if there was a value associated with the key, otherwise <see langword="false"/>.</returns>
    public bool TryGetForward(T1 key, out T2 value)
    {
        using var rLock = TakeReadLock();
        return _forward.TryGetValue(key, out value);
    }
    /// <summary>
    /// Attempts to retrieve an entry from the lookup table by its value.
    /// </summary>
    /// <param name="value">The value of the entry.</param>
    /// <param name="key">An <c>out</c> <typeparamref name="T1"/> variable that receives the retrieved key.</param>
    /// <returns><see langword="true"/> if there was a key associated with the value, otherwise <see langword="false"/>.</returns>
    public bool TryGetReverse(T2 value, out T1 key)
    {
        using var rLock = TakeReadLock();
        return _reverse.TryGetValue(value, out key);
    }
    /// <summary>
    /// Removes an entry from the lookup table by its key. An exception is thrown if there is no entry with the given key.
    /// </summary>
    /// <param name="key">The key of the entry.</param>
    public bool RemoveForward(T1 key)
    {
        using var wLock = TakeWriteLock();
        var rev = _forward[key];
        return _reverse.TryRemove(rev, out _) && _forward.TryRemove(key, out _);
    }
    /// <summary>
    /// Removes an entry from the lookup table by its value. An exception is thrown if there is no entry with the given value.
    /// </summary>
    /// <param name="value">The value of the entry.</param>
    public bool RemoveReverse(T2 value)
    {
        using var wLock = TakeWriteLock();
        var forw = _reverse[value];
        return _forward.TryRemove(forw, out _) && _reverse.TryRemove(value, out _);
    }
    /// <summary>
    /// Attempts to remove an entry from the lookup table by its key.
    /// </summary>
    /// <param name="key">The key of the entry.</param>
    /// <returns><see langword="true"/> if there was a value associated with the key that could be removed, otherwise <see langword="false"/>.</returns>
    public bool TryRemoveForward(T1 key)
    {
        using var uLock = TakeUpgradeableReadLock();
        if (_forward.ContainsKey(key))
        {
            using var wLock = uLock.Upgrade();
            return RemoveForward(key);
        }
        return false;
    }
    /// <summary>
    /// Attempts to remove an entry from the lookup table by its value.
    /// </summary>
    /// <param name="value">The value of the entry.</param>
    /// <returns><see langword="true"/> if there was a key associated with the value that could be removed, otherwise <see langword="false"/>.</returns>
    public bool TryRemoveReverse(T2 value)
    {
        using var uLock = TakeUpgradeableReadLock();
        if (_reverse.ContainsKey(value))
        {
            using var wLock = uLock.Upgrade();
            return RemoveReverse(value);
        }
        return false;
    }

    /// <summary>
    /// Removes all entries from the lookup table.
    /// </summary>
    public void Clear()
    {
        using var wLock = TakeWriteLock();
        _forward.Clear();
        _reverse.Clear();
    }

    #region Thread-safe enumeration
    // Warning! For the duration the IEnumerator<> implementations remain undisposed, one read lock will be held
    // This shouldn't cause issues when using foreach, but manual usage of GetEnumerator() and Dispose() must be carefully managed
    /// <summary>
    /// Gets an enumerator that, by default, iterates through the forward collection as <see cref="KeyValuePair{TKey, TValue}"/>s.
    /// </summary>
    /// <returns>An enumerator that can be used to iterate through the forward collection.</returns>
    public IEnumerator<KeyValuePair<T1, T2>> GetEnumerator() => new ReadLockedEnumerator<T1, T2>(_lock, _forward);
    /// <summary>
    /// Returns an enumerator that iterates through the forward collection as <see cref="KeyValuePair{TKey, TValue}"/>s.
    /// </summary>
    /// <returns>An enumerator that can be used to iterate through the forward collection.</returns>
    IEnumerator<KeyValuePair<T1, T2>> IEnumerable<KeyValuePair<T1, T2>>.GetEnumerator() => new ReadLockedEnumerator<T1, T2>(_lock, _forward);
    /// <summary>
    /// Returns an enumerator that iterates through the reverse collection as <see cref="KeyValuePair{TKey, TValue}"/>s.
    /// </summary>
    /// <returns>An enumerator that can be used to iterate through the reverse collection.</returns>
    public IEnumerator<KeyValuePair<T2, T1>> GetReverseEnumerator() => new ReadLockedEnumerator<T2, T1>(_lock, _reverse);
    IEnumerator IEnumerable.GetEnumerator() => new ReadLockedEnumerator<object, object>(_lock, (IEnumerable<KeyValuePair<object, object>>)_forward);

    /// <summary>
    /// Implements an <see cref="IEnumerator{T}"/> that holds a reader lock for the entire duration it is alive.
    /// </summary>
    internal struct ReadLockedEnumerator<TKey, TValue>(ReaderWriterLockSlim rwLock, IEnumerable<KeyValuePair<TKey, TValue>> enumerable) : IEnumerator<KeyValuePair<TKey, TValue>>
        where TKey : notnull
        where TValue : notnull
    {
        private readonly IEnumerator<KeyValuePair<TKey, TValue>> _inner = enumerable.GetEnumerator();
        private readonly ReadLock _readLock = new ReadLock(rwLock);

        public readonly KeyValuePair<TKey, TValue> Current => _inner.Current;
        readonly object IEnumerator.Current => Current;

        public readonly void Dispose()
        {
            _inner.Dispose();
            _readLock.Dispose();
        }
        public readonly bool MoveNext() => _inner.MoveNext();
        public readonly void Reset() => _inner.Reset();
    }
    #endregion
}