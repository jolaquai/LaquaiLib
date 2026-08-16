using System.Diagnostics.CodeAnalysis;

namespace LaquaiLib.Collections;

/// <summary>
/// Implements an <see cref="IEqualityComparer"/> that considers two sequences equal if they yield equal elements in the same order.
/// </summary>
/// <remarks>
/// Sequences of unrelated concrete types compare just fine against one another; an <see cref="int"/>[] may equal a <see cref="List{T}"/> of <see cref="int"/>.
/// Prefer <see cref="SequenceEqualityComparer{T}"/> whenever the element type is known statically; it avoids boxing every single element.
/// </remarks>
public sealed class SequenceEqualityComparer : IEqualityComparer<ICollection>, IEqualityComparer<IList>, IEqualityComparer<Array>, IEqualityComparer<IEnumerable>, IEqualityComparer<object>, IEqualityComparer
{
    // null means "default object equality", which doubles as the signal that elements may be compared bitwise where their type allows it
    private readonly IEqualityComparer _inner;
    private SequenceEqualityComparer(IEqualityComparer inner)
    {
        _inner = inner;
    }

    /// <summary>
    /// Gets a <see cref="SequenceEqualityComparer"/> that compares elements the way <see cref="EqualityComparer{T}.Default"/> for <see cref="object"/> does.
    /// </summary>
    public static readonly SequenceEqualityComparer Default = new SequenceEqualityComparer(null);

    /// <summary>
    /// Creates an <see cref="SequenceEqualityComparer"/> that uses the specified <see cref="IEqualityComparer"/> to compare the elements of the sequences.
    /// </summary>
    /// <param name="inner">The <see cref="IEqualityComparer"/> to use to compare the elements of the sequences.</param>
    /// <returns>The created <see cref="SequenceEqualityComparer"/>.</returns>
    public static SequenceEqualityComparer Create(IEqualityComparer inner)
    {
        ArgumentNullException.ThrowIfNull(inner);
        return ReferenceEquals(inner, EqualityComparer<object>.Default) ? Default : new SequenceEqualityComparer(inner);
    }

    /// <inheritdoc/>
    public bool Equals(IEnumerable x, IEnumerable y) => EqualsCore(x, y); // intentional re-route through there since we might be able to find a more specific type
    /// <inheritdoc/>
    public bool Equals(IList x, IList y) => EqualsCore(x, y);
    /// <inheritdoc/>
    public bool Equals(ICollection x, ICollection y) => EqualsCore(x, y);
    /// <inheritdoc/>
    public new bool Equals(object x, object y) => EqualsCore(x, y);
    /// <inheritdoc/>
    public bool Equals(Array x, Array y)
    {
        if (ReferenceEquals(x, y))
            return true;
        if (x is null || y is null || x.Length != y.Length)
            return false;

        // covariance makes this exactly "single-dimensional, zero-based, reference-typed elements", which is precisely when the data may be viewed as object references
        if (x is object[] left && y is object[] right)
            return ObjectSpansEqual(left, right);

        if (_inner is null)
        {
            var type = x.GetType();
            if (type == y.GetType())
            {
                var elementSize = SequenceHelpers.GetBitwiseElementSize(type);
                if (elementSize != 0)
                    return SequenceHelpers.BytesEqual(ref MemoryMarshal.GetArrayDataReference(x), ref MemoryMarshal.GetArrayDataReference(y), (long)x.Length * elementSize);
            }
        }

        return EnumerablesEqual(x, y);
    }

    /// <summary>
    /// Determines sequence equality between two objects, narrowing to the most specific shape both of them expose.
    /// </summary>
    /// <exception cref="NotSupportedException">Thrown when neither <paramref name="x"/> nor <paramref name="y"/> is enumerable.</exception>
    private bool EqualsCore(object x, object y)
    {
        if (ReferenceEquals(x, y))
            return true;
        if (x is null || y is null)
            return false;

        if (x is Array ax && y is Array ay)
            return Equals(ax, ay);

        var ex = x as IEnumerable;
        var ey = y as IEnumerable;
        if (ex is null && ey is null)
            throw new NotSupportedException($"Neither {x.GetType().FullName} nor {y.GetType().FullName} is supported by {nameof(SequenceEqualityComparer)}.");
        if (ex is null || ey is null)
            return false;

        // cheapest possible rejection before anything gets enumerated
        if (x is ICollection cx && y is ICollection cy && cx.Count != cy.Count)
            return false;
        if (x is IList lx && y is IList ly && IsIndexable(lx) && IsIndexable(ly))
            return ListsEqual(lx, ly);
        return EnumerablesEqual(ex, ey);
    }
    // only one side can still be an Array here, and Array's IList indexer throws for anything but single-dimensional zero-based instances
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsIndexable(IList list) => list is not Array array || array.GetType().IsSZArray;
    private bool ObjectSpansEqual(ReadOnlySpan<object> x, ReadOnlySpan<object> y)
    {
        for (var i = 0; i < x.Length; i++)
            if (!ElementEquals(x[i], y[i]))
                return false;
        return true;
    }
    private bool ListsEqual(IList x, IList y)
    {
        var count = x.Count;
        if (count != y.Count)
            return false;
        for (var i = 0; i < count; i++)
            if (!ElementEquals(x[i], y[i]))
                return false;
        return true;
    }
    /// <summary>
    /// Determines sequence equality by advancing enumerators for both <see cref="IEnumerable"/>s simultaneously and comparing each element at corresponding positions in the sequence.
    /// </summary>
    private bool EnumerablesEqual(IEnumerable x, IEnumerable y)
    {
        IEnumerator e1 = null;
        IEnumerator e2 = null;
        try
        {
            e1 = x.GetEnumerator();
            e2 = y.GetEnumerator();
            while (true)
            {
                var advanced1 = e1.MoveNext();
                var advanced2 = e2.MoveNext();
                if (advanced1 != advanced2)
                    return false;
                if (!advanced1)
                    return true;
                if (!ElementEquals(e1.Current, e2.Current))
                    return false;
            }
        }
        finally
        {
            (e1 as IDisposable)?.Dispose();
            (e2 as IDisposable)?.Dispose();
        }
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool ElementEquals(object x, object y) => _inner is not null ? _inner.Equals(x, y) : x is null ? y is null : x.Equals(y);

    /// <inheritdoc/>
    public int GetHashCode([DisallowNull] IEnumerable obj) => HashCore(obj);
    /// <inheritdoc/>
    public int GetHashCode([DisallowNull] Array obj) => HashCore(obj);
    /// <inheritdoc/>
    public int GetHashCode([DisallowNull] IList obj) => HashCore(obj);
    /// <inheritdoc/>
    public int GetHashCode([DisallowNull] ICollection obj) => HashCore(obj);
    /// <inheritdoc/>
    public int GetHashCode(object obj) => HashCore(obj);
    /// <summary>
    /// Combines the hash codes of every element of <paramref name="obj"/> in order.
    /// </summary>
    /// <exception cref="NotSupportedException">Thrown when <paramref name="obj"/> is not enumerable.</exception>
    private int HashCore(object obj)
    {
        if (obj is null)
            return 0;

        var hc = new HashCode();
        switch (obj)
        {
            case object[] array:
            {
                foreach (var item in array)
                    hc.Add(HashElement(item));
                break;
            }
            // must precede IList; IList indexing throws RankException for anything but rank-1 zero-based arrays
            case Array array:
            {
                foreach (var item in array)
                    hc.Add(HashElement(item));
                break;
            }
            case IList list:
            {
                for (var i = 0; i < list.Count; i++)
                    hc.Add(HashElement(list[i]));
                break;
            }
            case IEnumerable enumerable:
            {
                foreach (var item in enumerable)
                    hc.Add(HashElement(item));
                break;
            }
            default:
            {
                throw new NotSupportedException($"The type {obj.GetType().FullName} is not supported by {nameof(SequenceEqualityComparer)}.");
            }
        }
        return hc.ToHashCode();
    }
    // null is hashed as 0 instead of being handed to _inner; the overwhelming majority of IEqualityComparer implementations throw on it
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int HashElement(object item) => item is null ? 0 : _inner is not null ? _inner.GetHashCode(item) : item.GetHashCode();
}

/// <summary>
/// Implements an <see cref="IEqualityComparer{T}"/> that considers two sequences of <typeparamref name="T"/> equal if they yield equal elements in the same order.
/// </summary>
/// <typeparam name="T">The type of the elements of the sequences to compare.</typeparam>
/// <remarks>
/// Sequences of unrelated concrete types compare just fine against one another; a <typeparamref name="T"/>[] may equal a <see cref="List{T}"/>.
/// </remarks>
public sealed class SequenceEqualityComparer<T> : IEqualityComparer<T[]>, IEqualityComparer<List<T>>, IEqualityComparer<IList<T>>, IEqualityComparer<ICollection<T>>, IEqualityComparer<IEnumerable<T>>, IEqualityComparer
{
    // 0 when T's elements cannot be compared by their raw bytes
    private static readonly int _bitwiseElementSize = SequenceHelpers.GetBitwiseSize(typeof(T));

    // null means EqualityComparer<T>.Default, which doubles as the signal that elements may be compared bitwise where T allows it
    private readonly IEqualityComparer<T> _inner;
    private SequenceEqualityComparer(IEqualityComparer<T> inner)
    {
        _inner = inner;
    }

    /// <summary>
    /// Gets a <see cref="SequenceEqualityComparer{T}"/> that compares elements using <see cref="EqualityComparer{T}.Default"/>.
    /// </summary>
    public static readonly SequenceEqualityComparer<T> Default = new SequenceEqualityComparer<T>(null);

    /// <summary>
    /// Creates a <see cref="SequenceEqualityComparer{T}"/> that uses the specified <see cref="IEqualityComparer{T}"/> to compare the elements of the sequences.
    /// </summary>
    /// <param name="inner">The <see cref="IEqualityComparer{T}"/> to use to compare the elements of the sequences.</param>
    /// <returns>The created <see cref="SequenceEqualityComparer{T}"/>.</returns>
    public static SequenceEqualityComparer<T> Create(IEqualityComparer<T> inner)
    {
        ArgumentNullException.ThrowIfNull(inner);
        return ReferenceEquals(inner, EqualityComparer<T>.Default) ? Default : new SequenceEqualityComparer<T>(inner);
    }

    /// <inheritdoc/>
    public bool Equals(T[] x, T[] y) => ReferenceEquals(x, y) || (x is not null && y is not null && SpansEqual(x, y));
    /// <inheritdoc/>
    public bool Equals(List<T> x, List<T> y) => ReferenceEquals(x, y) || (x is not null && y is not null && SpansEqual(CollectionsMarshal.AsSpan(x), CollectionsMarshal.AsSpan(y)));
    /// <inheritdoc/>
    public bool Equals(IList<T> x, IList<T> y) => EqualsCore(x, y);
    /// <inheritdoc/>
    public bool Equals(ICollection<T> x, ICollection<T> y) => EqualsCore(x, y);
    /// <inheritdoc/>
    public bool Equals(IEnumerable<T> x, IEnumerable<T> y) => EqualsCore(x, y);
    bool IEqualityComparer.Equals(object x, object y)
    {
        if (ReferenceEquals(x, y))
            return true;
        if (x is null || y is null)
            return false;
        if (x is IEnumerable<T> ex && y is IEnumerable<T> ey)
            return EqualsCore(ex, ey);
        throw new NotSupportedException($"{x.GetType().FullName} and {y.GetType().FullName} are not both sequences of {typeof(T).FullName}.");
    }

    private bool EqualsCore(IEnumerable<T> x, IEnumerable<T> y)
    {
        if (ReferenceEquals(x, y))
            return true;
        if (x is null || y is null)
            return false;

        if (TryGetSpan(x, out var sx) && TryGetSpan(y, out var sy))
            return SpansEqual(sx, sy);
        // cheapest possible rejection before anything gets enumerated
        if (x is ICollection<T> cx && y is ICollection<T> cy && cx.Count != cy.Count)
            return false;
        if (x is IList<T> lx && y is IList<T> ly)
            return ListsEqual(lx, ly);
        return EnumerablesEqual(x, y);
    }
    private bool SpansEqual(ReadOnlySpan<T> x, ReadOnlySpan<T> y)
    {
        if (x.Length != y.Length)
            return false;
        // the length guard keeps the byte count from overflowing for very large arrays of multi-byte elements
        if (_inner is null && _bitwiseElementSize != 0)
            return SequenceHelpers.BytesEqual(
                ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(x)),
                ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(y)),
                (long)x.Length * _bitwiseElementSize
            );
        return x.SequenceEqual(y, _inner);
    }
    private bool ListsEqual(IList<T> x, IList<T> y)
    {
        var count = x.Count;
        if (count != y.Count)
            return false;
        var comparer = _inner ?? EqualityComparer<T>.Default;
        for (var i = 0; i < count; i++)
            if (!comparer.Equals(x[i], y[i]))
                return false;
        return true;
    }
    private bool EnumerablesEqual(IEnumerable<T> x, IEnumerable<T> y)
    {
        IEnumerator<T> e1 = null;
        IEnumerator<T> e2 = null;
        try
        {
            e1 = x.GetEnumerator();
            e2 = y.GetEnumerator();
            var comparer = _inner ?? EqualityComparer<T>.Default;
            while (true)
            {
                var advanced1 = e1.MoveNext();
                var advanced2 = e2.MoveNext();
                if (advanced1 != advanced2)
                    return false;
                if (!advanced1)
                    return true;
                if (!comparer.Equals(e1.Current, e2.Current))
                    return false;
            }
        }
        finally
        {
            e1?.Dispose();
            e2?.Dispose();
        }
    }

    /// <inheritdoc/>
    public int GetHashCode([DisallowNull] T[] obj) => HashCore(obj);
    /// <inheritdoc/>
    public int GetHashCode([DisallowNull] List<T> obj) => HashCore(obj);
    /// <inheritdoc/>
    public int GetHashCode([DisallowNull] IList<T> obj) => HashCore(obj);
    /// <inheritdoc/>
    public int GetHashCode([DisallowNull] ICollection<T> obj) => HashCore(obj);
    /// <inheritdoc/>
    public int GetHashCode([DisallowNull] IEnumerable<T> obj) => HashCore(obj);
    int IEqualityComparer.GetHashCode(object obj) => obj switch
    {
        null => 0,
        IEnumerable<T> enumerable => HashCore(enumerable),
        _ => throw new NotSupportedException($"The type {obj.GetType().FullName} is not a sequence of {typeof(T).FullName}.")
    };

    /// <summary>
    /// Combines the hash codes of every element of <paramref name="obj"/> in order.
    /// </summary>
    private int HashCore(IEnumerable<T> obj)
    {
        if (obj is null)
            return 0;

        var hc = new HashCode();
        if (TryGetSpan(obj, out var span))
            for (var i = 0; i < span.Length; i++)
            {
                hc.Add(HashElement(span[i]));
            }
        else if (obj is IList<T> list)
            for (var i = 0; i < list.Count; i++)
            {
                hc.Add(HashElement(list[i]));
            }
        else
            foreach (var item in obj)
            {
                hc.Add(HashElement(item));
            }
        return hc.ToHashCode();
    }
    // matches what HashCode.Add<T> would produce for the default comparer, which keeps this consistent with the non-generic comparer
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int HashElement(T item) => item is null ? 0 : _inner is not null ? _inner.GetHashCode(item) : item.GetHashCode();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool TryGetSpan(IEnumerable<T> source, out ReadOnlySpan<T> span)
    {
        switch (source)
        {
            case T[]:
                span = Unsafe.As<T[]>(source);
                return true;
            case List<T>:
                span = CollectionsMarshal.AsSpan(Unsafe.As<List<T>>(source));
                return true;
            default:
                span = default;
                return false;
        }
    }
}

static file class SequenceHelpers
{
    private static readonly ConcurrentDictionary<Type, int> _bitwiseElementSizes = [];

    /// <summary>
    /// Returns the element size of <paramref name="arrayType"/> if its elements compare equal exactly when their bytes do, otherwise 0.
    /// </summary>
    public static int GetBitwiseElementSize(Type arrayType) => _bitwiseElementSizes.GetOrAdd(arrayType, static type => GetBitwiseSize(type.GetElementType()));
    /// <summary>
    /// Returns the size of <paramref name="type"/> if its values compare equal exactly when their bytes do, otherwise 0.
    /// </summary>
    public static int GetBitwiseSize(Type type)
    {
        if (type is null)
            return 0;
        if (type.IsEnum)
            type = type.GetEnumUnderlyingType();
        if (!type.IsPrimitive)
            return 0;
        return Type.GetTypeCode(type) switch
        {
            TypeCode.Boolean or TypeCode.SByte or TypeCode.Byte => sizeof(byte),
            TypeCode.Char or TypeCode.Int16 or TypeCode.UInt16 => sizeof(short),
            TypeCode.Int32 or TypeCode.UInt32 => sizeof(int),
            TypeCode.Int64 or TypeCode.UInt64 => sizeof(long),
            // Single and Double deliberately fall through; -0.0 equals 0.0 but differs bitwise
            _ => type == typeof(nint) || type == typeof(nuint) ? nint.Size : 0
        };
    }

    /// <summary>
    /// Compares <paramref name="byteCount"/> bytes at <paramref name="left"/> and <paramref name="right"/>, chunked so that counts beyond <see cref="int.MaxValue"/> still work.
    /// </summary>
    public static bool BytesEqual(ref byte left, ref byte right, long byteCount)
    {
        ref var l = ref left;
        ref var r = ref right;
        while (byteCount > 0)
        {
            var chunk = (int)Math.Min(byteCount, int.MaxValue);
            if (!MemoryMarshal.CreateReadOnlySpan(ref l, chunk).SequenceEqual(MemoryMarshal.CreateReadOnlySpan(ref r, chunk)))
                return false;
            l = ref Unsafe.Add(ref l, chunk);
            r = ref Unsafe.Add(ref r, chunk);
            byteCount -= chunk;
        }
        return true;
    }
}
