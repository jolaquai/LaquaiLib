namespace LaquaiLib.Wrappers;

/// <summary>
/// Holds either no value, a single instance of <typeparamref name="T"/> or an <see cref="IEnumerable{T}"/>. Allows enumeration of the contained value(s) without boxing.
/// </summary>
/// <typeparam name="T">The type of the value(s) to hold.</typeparam>
/// <remarks>
/// The empty case is identified by <see cref="Value"/> being <see langword="null"/>. Constructing an instance from a <see langword="null"/> item or a <see langword="null"/> sequence consequently yields an empty container.
/// </remarks>
[Union]
public readonly struct FlexEnumerableContainer<T> : IEnumerable<T>
{
    private readonly IEnumerable<T> _enumerable;
    private readonly T _item;
    private readonly bool _isSingle; // required discriminant; default(T) is not null for value types

    /// <summary>
    /// Creates an empty <see cref="FlexEnumerableContainer{T}"/>.
    /// </summary>
    public FlexEnumerableContainer() { }
    /// <summary>
    /// Creates a <see cref="FlexEnumerableContainer{T}"/> that wraps the specified single instance of <typeparamref name="T"/>.
    /// </summary>
    /// <param name="item">The single instance of <typeparamref name="T"/> to wrap. If it is <see langword="null"/>, the container is empty.</param>
    public FlexEnumerableContainer(T item)
    {
        if (item is null)
            return;

        _item = item;
        _isSingle = true;
    }
    /// <summary>
    /// Creates a <see cref="FlexEnumerableContainer{T}"/> that wraps the specified <see cref="IEnumerable{T}"/>.
    /// </summary>
    /// <param name="enumerable">The <see cref="IEnumerable{T}"/> to wrap. If it is <see langword="null"/>, the container is empty.</param>
    public FlexEnumerableContainer(IEnumerable<T> enumerable)
    {
        _enumerable = enumerable;
    }

    // discriminant: _isSingle -> single (boxes through Value), _enumerable != null -> many, otherwise empty
    /// <summary>
    /// Gets the wrapped value, boxed if this instance wraps a single instance of <typeparamref name="T"/>, or <see langword="null"/> if it is empty.
    /// </summary>
    public object Value => _isSingle ? _item : _enumerable;
    /// <summary>
    /// Gets whether this instance wraps a value, that is, whether <see cref="Value"/> is not <see langword="null"/>.
    /// </summary>
    public bool HasValue => _isSingle || _enumerable is not null;

    /// <summary>
    /// Attempts to unwrap a single instance of <typeparamref name="T"/> from the <see cref="FlexEnumerableContainer{T}"/>.
    /// </summary>
    /// <param name="value">An output parameter that will contain the unwrapped value.</param>
    /// <returns><see langword="true"/> if the <see cref="FlexEnumerableContainer{T}"/> wraps a single instance of <typeparamref name="T"/>; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetValue(out T value)
    {
        value = _item;
        return _isSingle;
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
    public Enumerator GetEnumerator() => new(in this); // concrete, no box
    IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// Implements a strongly-typed enumerator for <see cref="FlexEnumerableContainer{T}"/> that can avoid boxing when enumerating.
    /// </summary>
    public struct Enumerator : IEnumerator<T>
    {
        private readonly IEnumerable<T> _enumerable;
        private readonly T _item;
        private readonly bool _isSingle;
        private IEnumerator<T> _enumerator;
        private bool _consumed;

        internal Enumerator(in FlexEnumerableContainer<T> container)
        {
            _enumerable = container._enumerable;
            _item = container._item;
            _isSingle = container._isSingle;
            _enumerator = null;
            _consumed = false;
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
            if (_enumerable is not null)
            {
                _enumerator ??= _enumerable.GetEnumerator(); // lazy
                var moved = _enumerator.MoveNext();
                Current = moved ? _enumerator.Current : default;
                return moved;
            }

            if (_isSingle && !_consumed)
            {
                Current = _item;
                _consumed = true;
                return true;
            }

            Current = default;
            return false;
        }
        /// <summary>
        /// Resets the enumerator to its initial position, which is before the first element.
        /// </summary>
        public void Reset()
        {
            _enumerator?.Dispose();
            _enumerator = null;
            _consumed = false;
            Current = default;
        }
    }
}
