namespace LaquaiLib.Wrappers;

[Union]
public readonly struct FlexEnumeration<T> : IEnumerable<T>
{
    private readonly IEnumerable<T> _enumerable;
    private readonly T _item;
    private readonly bool _empty;

    public FlexEnumeration() => _empty = true;
    public FlexEnumeration(T item) => _item = item;
    public FlexEnumeration(IEnumerable<T> enumerable) => _enumerable = enumerable;

    // canonical discriminant: _empty -> none; _enumerable!=null -> many; else single
    public object Value => _empty ? null : _enumerable ?? (object)_item;

    public bool HasValue => !_empty && (_enumerable is not null || _item is not null);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetValue(out T value)
    {
        if (!_empty && _enumerable is null)
        { value = _item; return true; }
        value = default;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetValue(out IEnumerable<T> value)
    {
        value = _enumerable;
        return _enumerable is not null;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Enumerator GetEnumerator() => new(_empty, _enumerable, _item); // concrete, no box
    IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public struct Enumerator : IEnumerator<T>
    {
        private readonly bool _empty;
        private readonly IEnumerable<T> _enumerable;
        private readonly T _item;
        private IEnumerator<T> _enumerator;
        private int _state; // 0 = not started, 1 = single emitted

        internal Enumerator(bool empty, IEnumerable<T> enumerable, T item)
        {
            _empty = empty;
            _enumerable = enumerable;
            _item = item;
            _enumerator = null;
            _state = 0;
            Current = default;
        }

        public T Current { get; private set; }
        readonly object IEnumerator.Current => Current;
        public readonly void Dispose() => _enumerator?.Dispose();

        public bool MoveNext()
        {
            if (_empty)
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

        public void Reset() { _enumerator?.Dispose(); _enumerator = null; _state = 0; Current = default; }
    }
}
