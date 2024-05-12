using System.Collections;
using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Reflection;
using System.Reflection.Emit;

namespace LaquaiLib.Collections;

/// <summary>
/// Represents an unsorted collection of unique items.
/// </summary>
/// <typeparam name="T">The type of the items.</typeparam>
public class Set<T> : ISet<T>, IEquatable<Set<T>>, IEquatable<IEnumerable<T>>
{
    private List<T> Items { get; } = [];

    /// <summary>
    /// Initializes a new, empty <see cref="Set{T}"/> with the default equality comparer.
    /// </summary>
    public Set() : this(EqualityComparer<T>.Default) { }
    /// <summary>
    /// Initializes a new <see cref="Set{T}"/> with items copied from the specified <paramref name="collection"/> and the default equality comparer.
    /// </summary>
    /// <param name="collection">An <see cref="IEnumerable{T}"/> whose items are copied to the new <see cref="Set{T}"/>.</param>
    public Set(IEnumerable<T> collection) : this(collection, EqualityComparer<T>.Default) { }
    /// <summary>
    /// Initializes a new, empty <see cref="Set{T}"/> with the specified <paramref name="equalityComparer"/>.
    /// </summary>
    /// <param name="equalityComparer">The <see cref="IEqualityComparer{T}"/> to use for equality comparisons.</param>
    public Set(IEqualityComparer<T> equalityComparer) : this([], equalityComparer) { }
    /// <summary>
    /// Initializes a new <see cref="Set{T}"/> with items copied from the specified <paramref name="collection"/> and the specified <paramref name="equalityComparer"/>.
    /// </summary>
    /// <param name="collection">An <see cref="IEnumerable{T}"/> whose items are copied to the new <see cref="Set{T}"/>.</param>
    /// <param name="equalityComparer">The <see cref="IEqualityComparer{T}"/> to use for equality comparisons.</param>
    public Set(IEnumerable<T> collection, IEqualityComparer<T> equalityComparer)
    {
        EqualityComparer = equalityComparer;
        UnionWith(collection);
    }

    /// <summary>
    /// The <see cref="IEqualityComparer{T}"/> used for equality comparisons.
    /// </summary>
    public IEqualityComparer<T> EqualityComparer { get; set; }

    /// <summary>
    /// The number of items in the <see cref="Set{T}"/>.
    /// </summary>
    public int Count => Items.Count;
    /// <summary>
    /// Whether the <see cref="Set{T}"/> is read-only.
    /// </summary>
    public bool IsReadOnly => false;
    /// <summary>
    /// Attempts to add the specified item to the <see cref="Set{T}"/>.
    /// </summary>
    /// <param name="item">The item to add.</param>
    /// <returns><see langword="true"/> if the addition was successful (that is, if the <see cref="Set{T}"/> did not previously contain the item), otherwise <see langword="false"/>.</returns>
    public bool Add(T item)
    {
        var add = !Contains(item);
        if (add)
        {
            Items.Add(item);
        }
        return add;
    }
    /// <summary>
    /// Removes all items from the <see cref="Set{T}"/>.
    /// </summary>
    public void Clear() => Items.Clear();
    /// <summary>
    /// Determines whether the <see cref="Set{T}"/> contains the specified item.
    /// </summary>
    /// <param name="item">The item to check for.</param>
    /// <returns><see langword="true"/> if the <see cref="Set{T}"/> contains the item, otherwise <see langword="false"/>.</returns>
    public bool Contains(T item) => Items.Contains(item, EqualityComparer);
    /// <summary>
    /// Determines whether the <see cref="Set{T}"/> contains the specified item.
    /// </summary>
    /// <param name="item">The item to check for.</param>
    /// <param name="comparer">The <see cref="IEqualityComparer{T}"/> to use for equality comparisons.</param>
    /// <returns><see langword="true"/> if the <see cref="Set{T}"/> contains the item, otherwise <see langword="false"/>.</returns>
    public bool Contains(T item, IEqualityComparer<T> comparer = null) => Items.Contains(item, comparer ?? EqualityComparer ?? EqualityComparer<T>.Default);
    /// <summary>
    /// Copies all items in the <see cref="Set{T}"/> to the specified <paramref name="array"/>, starting at the specified <paramref name="arrayIndex"/>.
    /// </summary>
    /// <param name="array">The array to copy the items to.</param>
    /// <param name="arrayIndex">The index in the array to start copying at.</param>
    public void CopyTo(T[] array, int arrayIndex) => Items.CopyTo(array, arrayIndex);
    /// <summary>
    /// Removes all items in the <see cref="Set{T}"/> that are also in the specified <paramref name="other"/> collection.
    /// </summary>
    /// <param name="other">The collection of items to remove.</param>
    public void ExceptWith(IEnumerable<T> other) => Items.RemoveAll(other.Contains);
    /// <summary>
    /// Returns an <see cref="IEnumerator{T}"/> that may be used to iterate over the items in the <see cref="Set{T}"/>.
    /// The order in which it enumerates the items is undefined.
    /// </summary>
    /// <returns>The enumerator as described.</returns>
    public IEnumerator<T> GetEnumerator() => Items.GetEnumerator();
    /// <summary>
    /// Keeps only the items in the <see cref="Set{T}"/> that are also in the specified <paramref name="other"/> collection.
    /// </summary>
    /// <param name="other">The collection of items to keep.</param>
    public void IntersectWith(IEnumerable<T> other) => Items.RemoveAll(i => !other.Contains(i));
    /// <summary>
    /// Determines whether the <see cref="Set{T}"/> is a proper subset of the specified <paramref name="other"/> collection (that is, whether all items in the <see cref="Set{T}"/> are also in the <paramref name="other"/> collection, but the <paramref name="other"/> collection contains at least one item that is not in the <see cref="Set{T}"/>).
    /// </summary>
    /// <param name="other">The collection to compare to.</param>
    /// <returns><see langword="true"/> if the <see cref="Set{T}"/> is a proper subset of the <paramref name="other"/> collection, otherwise <see langword="false"/>.</returns>
    public bool IsProperSubsetOf(IEnumerable<T> other)
    {
        var otherEnumerated = other as T[] ?? other.ToArray();
        return Count < otherEnumerated.Length && Items.TrueForAll(otherEnumerated.Contains);
    }
    /// <summary>
    /// Determines whether the <see cref="Set{T}"/> is a proper superset of the specified <paramref name="other"/> collection (that is, whether all items in the <paramref name="other"/> collection are also in the <see cref="Set{T}"/>, but the <see cref="Set{T}"/> contains at least one item that is not in the <paramref name="other"/> collection).
    /// </summary>
    /// <param name="other">The collection to compare to.</param>
    /// <returns><see langword="true"/> if the <see cref="Set{T}"/> is a proper superset of the <paramref name="other"/> collection, otherwise <see langword="false"/>.</returns>
    public bool IsProperSupersetOf(IEnumerable<T> other)
    {
        var otherEnumerated = other as T[] ?? other.ToArray();
        return Count > otherEnumerated.Length && Array.TrueForAll(otherEnumerated, Contains);
    }
    /// <summary>
    /// Determines whether the <see cref="Set{T}"/> is a subset of the specified <paramref name="other"/> collection (that is, whether all items in the <see cref="Set{T}"/> are also in the <paramref name="other"/> collection).
    /// </summary>
    /// <param name="other">The collection to compare to.</param>
    /// <returns><see langword="true"/> if the <see cref="Set{T}"/> is a subset of the <paramref name="other"/> collection, otherwise <see langword="false"/>.</returns>
    public bool IsSubsetOf(IEnumerable<T> other)
    {
        var otherEnumerated = other as T[] ?? other.ToArray();
        return Count <= otherEnumerated.Length && Items.TrueForAll(otherEnumerated.Contains);
    }
    /// <summary>
    /// Determines whether the <see cref="Set{T}"/> is a superset of the specified <paramref name="other"/> collection (that is, whether all items in the <paramref name="other"/> collection are also in the <see cref="Set{T}"/>).
    /// </summary>
    /// <param name="other">The collection to compare to.</param>
    /// <returns><see langword="true"/> if the <see cref="Set{T}"/> is a superset of the <paramref name="other"/> collection, otherwise <see langword="false"/>.</returns>
    public bool IsSupersetOf(IEnumerable<T> other)
    {
        var otherEnumerated = other as T[] ?? other.ToArray();
        return Count >= otherEnumerated.Length && Array.TrueForAll(otherEnumerated, Contains);
    }
    /// <summary>
    /// Determines whether the <see cref="Set{T}"/> overlaps with the specified <paramref name="other"/> collection (that is, whether the <see cref="Set{T}"/> contains at least one item that is also in the <paramref name="other"/> collection).
    /// </summary>
    /// <param name="other">The collection to compare to.</param>
    /// <returns><see langword="true"/> if the <see cref="Set{T}"/> overlaps with the <paramref name="other"/> collection, otherwise <see langword="false"/>.</returns>
    public bool Overlaps(IEnumerable<T> other) => other.Any(Contains);
    /// <summary>
    /// Removes the specified item from the <see cref="Set{T}"/>.
    /// </summary>
    /// <param name="item">The item to remove.</param>
    /// <returns><see langword="true"/> if the removal was successful (that is, if the <see cref="Set{T}"/> contained the item), otherwise <see langword="false"/>.</returns>
    public bool Remove(T item)
    {
        var remove = Items.Contains(item);
        if (remove)
        {
            Items.Remove(item);
        }
        return remove;
    }
    /// <summary>
    /// Determines whether the <see cref="Set{T}"/> is equal to the specified <paramref name="other"/> collection (that is, whether the <see cref="Set{T}"/> contains the same items as the <paramref name="other"/> collection).
    /// </summary>
    /// <param name="other">The collection to compare to.</param>
    /// <returns><see langword="true"/> if the <see cref="Set{T}"/> is equal to the <paramref name="other"/> collection, otherwise <see langword="false"/>.</returns>
    public bool SetEquals(IEnumerable<T> other)
    {
        if (other is null)
        {
            return false;
        }

        var otherEnumerated = other as T[] ?? other.ToArray();
        if (Count != otherEnumerated.Length)
        {
            return false;
        }

        return Array.TrueForAll(otherEnumerated, Contains);
    }
    /// <summary>
    /// Removes all items from the <see cref="Set{T}"/> that are also in the specified <paramref name="other"/> collection, and adds all items from the <paramref name="other"/> collection that are not already in the <see cref="Set{T}"/>.
    /// </summary>
    /// <param name="other">The collection of items to operate on.</param>
    public void SymmetricExceptWith(IEnumerable<T> other)
    {
        foreach (var item in other)
        {
            if (Contains(item))
            {
                Remove(item);
            }
            else
            {
                Add(item);
            }
        }
    }
    /// <summary>
    /// Adds all items from the specified <paramref name="other"/> collection to the <see cref="Set{T}"/> that are not already in the <see cref="Set{T}"/>.
    /// </summary>
    /// <param name="other">The collection of items to add.</param>
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
    /// <summary>
    /// Creates a <see cref="SortedSet{T}"/> from the <see cref="Set{T}"/> using the default comparer for <typeparamref name="T"/>.
    /// </summary>
    /// <returns>The created <see cref="SortedSet{T}"/>.</returns>
    public SortedSet<T> ToSortedSet() => new SortedSet<T>(Items);
    public SortedSet<T> ToSortedSet(IComparer<T> comparer) => new SortedSet<T>(Items, comparer);
    public SortedSet<T> ToSortedSet(Comparison<T> comparison)
    {
        // Create dynamic assembly
        var newAssembly = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("DynamicAssembly"), AssemblyBuilderAccess.Run);
        var newModule = newAssembly.DefineDynamicModule("DynamicModule");
        var newType = newModule.DefineType("DynamicComparerType", TypeAttributes.Public, parent: null, interfaces: [typeof(IComparer<T>)]);
        var newMethod = newType.DefineMethod("Compare", MethodAttributes.Public, CallingConventions.Standard, typeof(int), [typeof(T), typeof(T)]);
        var ilGen = newMethod.GetILGenerator();
        ilGen.Emit(OpCodes.Ldarg_1);
        ilGen.Emit(OpCodes.Ldarg_2);
        ilGen.Emit(OpCodes.Call, comparison.Method);
        ilGen.Emit(OpCodes.Ret);
        var newTypeInstance = newType.CreateType();
        var newTypeInstanceInstance = newTypeInstance.GetConstructor([])?.Invoke(null);
        return new SortedSet<T>(Items, (IComparer<T>)newTypeInstanceInstance);
    }
    /// <summary>
    /// Creates an <see cref="ImmutableSortedSet{T}"/> from the <see cref="Set{T}"/>
    /// </summary>
    /// <returns>The created <see cref="ImmutableSortedSet{T}"/>.</returns>
    public ImmutableSortedSet<T> ToImmutableSortedSet() => ImmutableSortedSet.CreateRange(Items);

    /// <summary>
    /// Compares the <see cref="Set{T}"/> to the specified <paramref name="other"/> <see cref="Set{T}"/> for equality.
    /// </summary>
    /// <param name="other">The <see cref="Set{T}"/> to compare to.</param>
    /// <returns><see langword="true"/> if the <see cref="Set{T}"/> is equal to the <paramref name="other"/> <see cref="Set{T}"/>, otherwise <see langword="false"/>.</returns>
    public bool Equals(Set<T>? other) => other is not null && SetEquals(other);
    /// <summary>
    /// Compares the <see cref="Set{T}"/> to the specified <paramref name="other"/> collection for equality.
    /// </summary>
    /// <param name="other">The collection to compare to.</param>
    /// <returns><see langword="true"/> if the <see cref="Set{T}"/> is equal to the <paramref name="other"/> collection, otherwise <see langword="false"/>.</returns>
    public bool Equals(IEnumerable<T>? other) => other is not null && SetEquals(other);
}
