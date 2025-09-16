#pragma warning disable IDE0058 // Expression value is never used

using System.Numerics;

namespace LaquaiLib.Collections;

/// <summary>
/// Contains factory methods for <see cref="ConcurrentSet{T}"/>.
/// </summary>
public static class ConcurrentSet
{
    /// <summary>
    /// Creates a new <see cref="ConcurrentSet{T}"/> containing the specified values, using the default equality comparer for <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of elements in the set.</typeparam>
    /// <param name="values">The values to include in the set.</param>
    /// <returns>The created set.</returns>
    public static ConcurrentSet<T> Create<T>(params ReadOnlySpan<T> values) where T : notnull => Create(values, EqualityComparer<T>.Default);
    /// <summary>
    /// Creates a new <see cref="ConcurrentSet{T}"/> containing the specified values, using the specified equality comparer.
    /// </summary>
    /// <typeparam name="T">The type of elements in the set.</typeparam>
    /// <param name="values">The values to include in the set.</param>
    /// <param name="equalityComparer">The equality comparer to use, or <c>null</c> to use the default equality comparer for <typeparamref name="T"/>.</param>
    /// <returns>The created set.</returns>
    public static ConcurrentSet<T> Create<T>(ReadOnlySpan<T> values, IEqualityComparer<T> equalityComparer) where T : notnull
    {
        var set = new ConcurrentSet<T>(equalityComparer);
        for (var i = 0; i < values.Length; i++)
        {
            var stripe = set.GetStripe(values[i]);
            stripe.Set.Add(values[i]);
        }
        Interlocked.Exchange(ref set._count, values.Length);
        return set;
    }
}

/// <summary>
/// Implements a thread-safe <see cref="ISet{T}"/>.
/// </summary>
[CollectionBuilder(typeof(ConcurrentSet), nameof(ConcurrentSet.Create))]
public sealed class ConcurrentSet<T> : ISet<T> where T : notnull
{
    private const int DefaultStripes = 16;
    private readonly Stripe[] _stripes;
    private readonly IEqualityComparer<T> _comparer;
    internal int _count; // approximate; maintained exactly via locks below

    internal sealed class Stripe(IEqualityComparer<T> comparer)
    {
        public readonly Lock Lock = new();
        public readonly HashSet<T> Set = new HashSet<T>(comparer);
    }

    /// <summary>
    /// Initializes a new <see cref="ConcurrentSet{T}"/> using the default equality comparer for <typeparamref name="T"/>.
    /// </summary>
    public ConcurrentSet() : this(EqualityComparer<T>.Default) { }
    /// <summary>
    /// Initializes a new <see cref="ConcurrentSet{T}"/> using the specified equality comparer.
    /// </summary>
    /// <param name="comparer">The equality comparer to use, or <c>null</c> to use the default equality comparer for <typeparamref name="T"/>.</param>
    public ConcurrentSet(IEqualityComparer<T> comparer)
    {
        var s = (int)BitOperations.RoundUpToPowerOf2(DefaultStripes);

        _stripes = new Stripe[s];
        _comparer = comparer ?? EqualityComparer<T>.Default;
        for (var i = 0; i < _stripes.Length; i++)
        {
            _stripes[i] = new Stripe(_comparer);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal Stripe GetStripe(T item)
    {
        var h = _comparer.GetHashCode(item);
        var idx = (uint)h & (uint)(_stripes.Length - 1);
        return _stripes[(int)idx];
    }

    /// <inheritdoc/>
    public int Count
    {
        get
        {
            var total = 0;
            // lock each stripe briefly and sum
            for (var i = 0; i < _stripes.Length; i++)
            {
                var stripe = _stripes[i];
                lock (stripe.Lock)
                {
                    total += stripe.Set.Count;
                }
            }
            return total;
        }
    }
    /// <inheritdoc/>
    public bool IsReadOnly => false;

    /// <inheritdoc/>
    public bool Add(T item)
    {
        var stripe = GetStripe(item);
        lock (stripe.Lock)
        {
            if (stripe.Set.Add(item))
            {
                // keep _count as redundant; not used for correctness but cheap to maintain
                Interlocked.Increment(ref _count);
                return true;
            }
            return false;
        }
    }
    /// <inheritdoc/>
    public bool Contains(T item)
    {
        var stripe = GetStripe(item);
        lock (stripe.Lock)
        {
            return stripe.Set.Contains(item);
        }
    }
    /// <inheritdoc/>
    public bool Remove(T item)
    {
        var stripe = GetStripe(item);
        lock (stripe.Lock)
        {
            if (stripe.Set.Remove(item))
            {
                Interlocked.Decrement(ref _count);
                return true;
            }
            return false;
        }
    }
    /// <inheritdoc/>
    public void Clear()
    {
        // lock stripes in order to avoid deadlocks
        foreach (var stripe in _stripes)
        {
            stripe.Lock.Enter();
        }

        try
        {
            foreach (var stripe in _stripes)
            {
                stripe.Set.Clear();
            }

            Interlocked.Exchange(ref _count, 0);
        }
        finally
        {
            for (var i = _stripes.Length - 1; i >= 0; i--)
            {
                _stripes[i].Lock.Exit();
            }
        }
    }
    /// <inheritdoc/>
    public void CopyTo(T[] array, int arrayIndex)
    {
        ArgumentNullException.ThrowIfNull(array);
        ArgumentOutOfRangeException.ThrowIfNegative(arrayIndex);

        var items = new T[Count];
        var lastIndex = 0;
        foreach (var stripe in _stripes)
        {
            lock (stripe.Lock)
            {
                stripe.Set.CopyTo(items, lastIndex);
                lastIndex += stripe.Set.Count;
            }

            if (arrayIndex + items.Length > array.Length)
            {
                throw new ArgumentException("Destination array is too small.");
            }
        }

        items.CopyTo(array, arrayIndex);
    }
    /// <inheritdoc/>
    public void ExceptWith(IEnumerable<T> other)
    {
        ArgumentNullException.ThrowIfNull(other);
        foreach (var item in other)
        {
            Remove(item);
        }
    }
    /// <inheritdoc/>
    public void IntersectWith(IEnumerable<T> other)
    {
        ArgumentNullException.ThrowIfNull(other);
        var otherSet = new HashSet<T>(other, _comparer);
        foreach (var stripe in _stripes)
        {
            lock (stripe.Lock)
            {
                var toRemove = stripe.Set.Where(x => !otherSet.Contains(x)).ToList();
                foreach (var r in toRemove)
                {
                    if (stripe.Set.Remove(r))
                    {
                        Interlocked.Decrement(ref _count);
                    }
                }
            }
        }
    }
    /// <inheritdoc/>
    public void UnionWith(IEnumerable<T> other)
    {
        ArgumentNullException.ThrowIfNull(other);
        foreach (var item in other)
        {
            Add(item);
        }
    }
    /// <inheritdoc/>
    public void SymmetricExceptWith(IEnumerable<T> other)
    {
        ArgumentNullException.ThrowIfNull(other);
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
        ArgumentNullException.ThrowIfNull(other);
        var otherSet = new HashSet<T>(other, _comparer);
        foreach (var stripe in _stripes)
        {
            lock (stripe.Lock)
            {
                foreach (var item in stripe.Set)
                {
                    if (!otherSet.Contains(item))
                    {
                        return false;
                    }
                }
            }
        }
        return true;
    }
    /// <inheritdoc/>
    public bool IsSupersetOf(IEnumerable<T> other)
    {
        ArgumentNullException.ThrowIfNull(other);
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
        ArgumentNullException.ThrowIfNull(other);
        var otherSet = new HashSet<T>(other, _comparer);
        var c = Count;
        return c < otherSet.Count && IsSubsetOf(otherSet);
    }
    /// <inheritdoc/>
    public bool IsProperSupersetOf(IEnumerable<T> other)
    {
        ArgumentNullException.ThrowIfNull(other);
        var otherSet = new HashSet<T>(other, _comparer);
        var c = Count;
        return c > otherSet.Count && IsSupersetOf(otherSet);
    }
    /// <inheritdoc/>
    public bool Overlaps(IEnumerable<T> other)
    {
        ArgumentNullException.ThrowIfNull(other);
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
        ArgumentNullException.ThrowIfNull(other);
        var otherSet = new HashSet<T>(other, _comparer);
        var c = Count;
        if (c != otherSet.Count)
        {
            return false;
        }

        return IsSubsetOf(otherSet);
    }
    /// <inheritdoc/>
    public IEnumerator<T> GetEnumerator()
    {
        // snapshot all items to avoid holding locks during enumeration
        var items = new T[Count];
        CopyTo(items, 0);
        return ((IEnumerable<T>)items).GetEnumerator();
    }
    /// <inheritdoc/>
    void ICollection<T>.Add(T item) => Add(item);
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}