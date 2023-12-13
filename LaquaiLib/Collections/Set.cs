using System.Collections;
using System.Collections.Frozen;
using System.Collections.Immutable;

namespace LaquaiLib.Collections;

/// <summary>
/// Represents an unsorted collection of unique items.
/// </summary>
/// <typeparam name="T">The type of the items.</typeparam>
public class Set<T> : ISet<T>
{
    private List<T> Items { get; } = [];

    // TODO: .ctors
    // TODO: Docs

    public IEqualityComparer<T> EqualityComparer { get; set; }

    public int Count => Items.Count;
    public bool IsReadOnly => false;
    public bool Add(T item)
    {
        var add = !Items.Contains(item);
        if (add)
        {
            Items.Add(item);
        }
        return add;
    }
    public void Clear() => Items.Clear();
    public bool Contains(T item) => Items.Contains(item, EqualityComparer);
    public void CopyTo(T[] array, int arrayIndex) => Items.CopyTo(array, arrayIndex);
    public void ExceptWith(IEnumerable<T> other) => Items.RemoveAll(other.Contains);
    public IEnumerator<T> GetEnumerator() => Items.GetEnumerator();
    public void IntersectWith(IEnumerable<T> other) => Items.RemoveAll(i => !other.Contains(i));
    public bool IsProperSubsetOf(IEnumerable<T> other)
    {
        var otherSet = new Set<T>();
        otherSet.UnionWith(other);
        return IsProperSubsetOf(otherSet);
    }
    public bool IsProperSupersetOf(IEnumerable<T> other)
    {
        var otherSet = new Set<T>();
        otherSet.UnionWith(other);
        return IsProperSupersetOf(otherSet);
    }
    public bool IsSubsetOf(IEnumerable<T> other)
    {
        var otherSet = new Set<T>();
        otherSet.UnionWith(other);
        return IsSubsetOf(otherSet);
    }
    public bool IsSupersetOf(IEnumerable<T> other)
    {
        var otherSet = new Set<T>();
        otherSet.UnionWith(other);
        return IsSupersetOf(otherSet);
    }
    public bool Overlaps(IEnumerable<T> other)
    {
        var otherSet = new Set<T>();
        otherSet.UnionWith(other);
        return Overlaps(otherSet);
    }
    public bool Remove(T item)
    {
        var remove = Items.Contains(item);
        if (remove)
        {
            Items.Remove(item);
        }
        return remove;
    }
    public bool SetEquals(IEnumerable<T> other)
    {
        var otherSet = new Set<T>();
        otherSet.UnionWith(other);
        return SetEquals(otherSet);
    }
    public void SymmetricExceptWith(IEnumerable<T> other)
    {
        var otherSet = new Set<T>();
        otherSet.UnionWith(other);
        SymmetricExceptWith(otherSet);
    }
    public void UnionWith(IEnumerable<T> other)
    {
        foreach (var item in other)
        {
            Add(item);
        }
    }
    void ICollection<T>.Add(T item) => Add(item);
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// Freezes the set into a <see cref="FrozenSet{T}"/>.
    /// </summary>
    /// <returns>The frozen set.</returns>
    public FrozenSet<T> ToFrozenSet() => FrozenSet.ToFrozenSet(Items);
    public SortedSet<T> ToSortedSet() => new SortedSet<T>(Items);
    public ImmutableSortedSet<T> ToImmutableSortedSet() => ImmutableSortedSet.CreateRange(Items);
}
