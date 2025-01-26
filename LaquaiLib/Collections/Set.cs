using System.Collections;

using LaquaiLib.Extensions;

namespace LaquaiLib.Collections;

/// <summary>
/// Implements <see cref="ISet{T}"/> as a generic, unordered collection of elements.
/// </summary>
/// <typeparam name="T">The type of elements in the set.</typeparam>
internal class Set<T> : ISet<T>
{
    private readonly List<T> _backingStore = [];

    public int Count => _backingStore.Count;
    public bool IsReadOnly => false;

    public bool Add(T item)
    {
        _backingStore.Add(item);
        return true;
    }
    public void Clear() => _backingStore.Clear();
    public bool Contains(T item) => _backingStore.Contains(item);
    public void CopyTo(T[] array, int arrayIndex) => _backingStore.CopyTo(array, arrayIndex);
    public void ExceptWith(IEnumerable<T> other) => _backingStore.RemoveAll(other.Contains);
    public IEnumerator<T> GetEnumerator() => _backingStore.GetEnumerator();
    public void IntersectWith(IEnumerable<T> other) => _backingStore.RemoveAll(x => !other.Contains(x));
    public bool IsProperSubsetOf(IEnumerable<T> other)
    {
        if (!other.TryGetNonEnumeratedCount(out var otherCount))
        {
            otherCount = other.Count();
        }
        return Count < otherCount && IsSubsetOf(other);
    }
    public bool IsProperSupersetOf(IEnumerable<T> other)
    {
        if (!other.TryGetNonEnumeratedCount(out var otherCount))
        {
            otherCount = other.Count();
        }
        return Count > otherCount && IsSupersetOf(other);
    }
    public bool IsSubsetOf(IEnumerable<T> other)
    {
        foreach (var item in this)
        {
            if (!other.Contains(item))
            {
                return false;
            }
        }
        return true;
    }
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
    public bool Overlaps(IEnumerable<T> other) => _backingStore.Find(other.Contains) is not null;
    public bool Remove(T item) => _backingStore.Remove(item);
    public bool SetEquals(IEnumerable<T> other)
    {
        if (!other.TryGetNonEnumeratedCount(out var otherCount))
        {
            otherCount = other.Count();
        }
        return Count == otherCount && _backingStore.All(other.Contains);
    }
    public void SymmetricExceptWith(IEnumerable<T> other)
    {
        foreach (var item in other)
        {
            if (Contains(item))
            {
                _ = Remove(item);
            }
            else
            {
                _ = Add(item);
            }
        }
    }
    public void UnionWith(IEnumerable<T> other) => _backingStore.AddRange(other.Where(o => !Contains(o)));

    void ICollection<T>.Add(T item) => Add(item);
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
