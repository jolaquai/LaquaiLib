namespace LaquaiLib.Wrappers;

/// <summary>
/// Holds either no value, a single instance of <typeparamref name="T"/> or an <see cref="IEnumerable{T}"/>. Allows enumeration of the contained value(s) without boxing.
/// </summary>
/// <typeparam name="T">The type of the value(s) to hold.</typeparam>
[Union]
public readonly struct FlexEnumerableContainer<T> : IEnumerable<T>
{
    private readonly IEnumerable<T> _enumerable;
    private readonly T _item;
    private readonly bool _hasItems;

    /// <summary>
    /// Creates an empty <see cref="FlexEnumerableContainer{T}"/>.
    /// </summary>
    public FlexEnumerableContainer() { }
    /// <summary>
    /// Creates an empty <see cref="FlexEnumerableContainer{T}"/>.
    /// </summary>
    /// <param name="_"></param>
    public FlexEnumerableContainer(Empty _) { }
    /// <summary>
    /// Creates a <see cref="FlexEnumerableContainer{T}"/> that wraps the specified single instance of <typeparamref name="T"/>.
    /// </summary>
    /// <param name="item">The single instance of <typeparamref name="T"/> to wrap.</param>
    public FlexEnumerableContainer(T item)
    {
        _item = item;
        _hasItems = true;
    }
    /// <summary>
    /// Creates a <see cref="FlexEnumerableContainer{T}"/> that wraps the specified <see cref="IEnumerable{T}"/>.
    /// </summary>
    /// <param name="enumerable">The <see cref="IEnumerable{T}"/> to wrap.</param>
    public FlexEnumerableContainer(IEnumerable<T> enumerable)
    {
        _enumerable = enumerable;
        _hasItems = true;
    }

    // canonical discriminant: _empty -> Empty case; _enumerable!=null -> many; else single
    /// <summary>
    /// Gets the (potentially boxed) wrapped value.
    /// </summary>
    public object Value => !_hasItems ? default(Empty) : _enumerable ?? (object)_item;
    /// <summary>
    /// Gets whether the union contains a value.
    /// </summary>
    public bool HasValue => !_hasItems || _enumerable is not null || _item is not null;

    /// <summary>
    /// Attempts to unwrap an empty <see cref="FlexEnumerableContainer{T}"/>. Only succeeds if it was created empty.
    /// The <see cref="Empty"/> value itself is meaningless.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetValue(out Empty value)
    {
        value = default;
        return !_hasItems;
    }
    /// <summary>
    /// Attempts to unwrap a single instance of <typeparamref name="T"/> from the <see cref="FlexEnumerableContainer{T}"/>.
    /// </summary>
    /// <param name="value">An output parameter that will contain the unwrapped value.</param>
    /// <returns><see langword="true"/> if the <see cref="FlexEnumerableContainer{T}"/> wraps a single instance of <typeparamref name="T"/>; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetValue(out T value)
    {
        if (_hasItems && _enumerable is null)
        { value = _item; return true; }
        value = default;
        return false;
    }
    /// <summary>
    /// Attempts to unwrap an <see cref="IEnumerable{T}"/> from the <see cref="FlexEnumerableContainer{T}"/>.
    /// </summary>
    /// <param name="value">An output parameter that will contain the unwrapped value.</param>
    /// <returns><see langword="true"/> if the <see cref="FlexEnumerableContainer{T}"/> wraps an <see cref="IEnumerable{T}"/>; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetValue(out IEnumerable<T> value)
    {
        value = _enumerable;
        return _enumerable is not null;
    }
    /// <summary>
    /// Gets an enumerator that iterates through the contained value(s).
    /// </summary>
    /// <returns>The enumerator that can be used to iterate through the contained value(s).</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Enumerator GetEnumerator() => new(_hasItems, _enumerable, _item); // concrete, no box
    IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// Implements a strongly-typed enumerator for <see cref="FlexEnumerableContainer{T}"/> that can avoid boxing when enumerating.
    /// </summary>
    public struct Enumerator : IEnumerator<T>
    {
        private readonly bool _hasItems;
        private readonly IEnumerable<T> _enumerable;
        private readonly T _item;
        private IEnumerator<T> _enumerator;
        private int _state; // 0 = not started, 1 = single emitted

        internal Enumerator(bool hasItems, IEnumerable<T> enumerable, T item)
        {
            _hasItems = hasItems;
            _enumerable = enumerable;
            _item = item;
            _enumerator = null;
            _state = 0;
            Current = default;
        }

        /// <summary>
        /// Gets the element in the collection at the current position of the enumerator.
        /// </summary>
        public T Current { get; private set; }
        readonly object IEnumerator.Current => Current;
        /// <summary>
        /// Disposes the enumerator.
        /// </summary>
        public readonly void Dispose() => _enumerator?.Dispose();
        /// <summary>
        /// Advances the enumerator to the next element of the collection.
        /// </summary>
        /// <returns><see langword="true"/> if the enumerator could be advanced; otherwise (that is, when enumeration has ended), <see langword="false"/>.</returns>
        public bool MoveNext()
        {
            if (!_hasItems)
                return false;

            if (_enumerable is null) // single
            {
                if (_state == 0)
                { Current = _item; _state = 1; return true; }
                Current = default;
                return false;
            }

            _enumerator ??= _enumerable.GetEnumerator(); // many (lazy)
            var moved = _enumerator.MoveNext();
            Current = moved ? _enumerator.Current : default;
            return moved;
        }
        /// <summary>
        /// Resets the enumerator to its initial position, which is before the first element.
        /// </summary>
        public void Reset() { _enumerator?.Dispose(); _enumerator = null; _state = 0; Current = default; }
    }
}
