using System.Collections;

namespace LaquaiLib.Generators;

/// <summary>
/// Wraps an <see cref="ImmutableArray{T}"/> with order-sensitive structural equality.
/// </summary>
/// <typeparam name="T">The type of the elements.</typeparam>
/// <remarks>
/// Records do not get structural equality over an <see cref="ImmutableArray{T}"/> field; the compiler-generated
/// <c>Equals</c> falls back to the array's reference equality, which permanently defeats incremental pipeline caching.
/// Every model that carries a collection must use this type instead.
/// </remarks>
internal readonly struct EquatableArray<T> : IEquatable<EquatableArray<T>>, IEnumerable<T> where T : IEquatable<T>
{
    private readonly ImmutableArray<T> _array;

    public EquatableArray(ImmutableArray<T> array)
    {
        _array = array;
    }

    public EquatableArray(IEnumerable<T> items)
    {
        _array = items is null ? ImmutableArray<T>.Empty : ImmutableArray.CreateRange(items);
    }

    /// <summary>
    /// Gets the wrapped <see cref="ImmutableArray{T}"/>, normalizing the uninitialized <see langword="default"/> state to empty.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ImmutableArray<T> AsImmutableArray() => _array.IsDefault ? ImmutableArray<T>.Empty : _array;

    public int Length
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _array.IsDefault ? 0 : _array.Length;
    }
    public bool IsEmpty
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Length == 0;
    }
    public T this[int index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => AsImmutableArray()[index];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator EquatableArray<T>(ImmutableArray<T> array) => new EquatableArray<T>(array);

    public bool Equals(EquatableArray<T> other)
    {
        var mine = AsImmutableArray();
        var theirs = other.AsImmutableArray();
        if (mine.Length != theirs.Length)
            return false;
        for (var i = 0; i < mine.Length; i++)
        {
            var x = mine[i];
            var y = theirs[i];
            if (x is null)
            {
                if (y is not null)
                    return false;
            }
            else if (!x.Equals(y))
                return false;
        }
        return true;
    }

    public override bool Equals(object obj) => obj is EquatableArray<T> other && Equals(other);

    public override int GetHashCode()
    {
        var array = AsImmutableArray();
        // FNV-1a; System.HashCode is unavailable on netstandard2.0
        var hash = 2166136261u;
        for (var i = 0; i < array.Length; i++)
        {
            var item = array[i];
            hash = (hash ^ (uint)(item is null ? 0 : item.GetHashCode())) * 16777619u;
        }
        return unchecked((int)hash);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(EquatableArray<T> left, EquatableArray<T> right) => left.Equals(right);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(EquatableArray<T> left, EquatableArray<T> right) => !left.Equals(right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ImmutableArray<T>.Enumerator GetEnumerator() => AsImmutableArray().GetEnumerator();
    IEnumerator<T> IEnumerable<T>.GetEnumerator() => ((IEnumerable<T>)AsImmutableArray()).GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)AsImmutableArray()).GetEnumerator();
}
