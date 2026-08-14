using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace LaquaiLib.Collections;

/// <summary>
/// Implements a 
/// </summary>
public sealed class SequenceEqualityComparer : IEqualityComparer<ICollection>, IEqualityComparer<IList>, IEqualityComparer<Array>, IEqualityComparer<IEnumerable>, IEqualityComparer
{
    private readonly IEqualityComparer _inner;
    private SequenceEqualityComparer(IEqualityComparer inner)
    {
        _inner = inner;
    }

    /// <summary>
    /// Creates an <see cref="SequenceEqualityComparer"/> that uses the specified <see cref="IEqualityComparer"/> to compare the elements of the sequences.
    /// </summary>
    /// <param name="inner">The <see cref="IEqualityComparer"/> to use to compare the elements of the sequences.</param>
    /// <returns>The created <see cref="SequenceEqualityComparer"/>.</returns>
    public static SequenceEqualityComparer Create(IEqualityComparer inner)
    {
        ArgumentNullException.ThrowIfNull(inner);
        return new SequenceEqualityComparer(inner);
    }

    /// <inheritdoc/>
    public bool Equals(IEnumerable x, IEnumerable y) => ObjectsEqualCore(x, y); // intentional re-route through there since we might be able to find a more specific type
    /// <inheritdoc/>
    public bool Equals(Array x, Array y)
    {
        if (x.Length != y.Length)
            return false;
        var type = x.GetType();
        if (type == y.GetType())
        {
            var underlying = x.GetType().GetElementType();
            var equalsMethod = _typedArrayEqualsMethod.MakeGenericMethod(underlying);
            return (bool)equalsMethod.Invoke(null, [x, y]);
        }
        return EqualsSlow(x, y);
    }
    /// <inheritdoc/>
    public bool Equals(IList x, IList y)
    {
        if (x.Count != y.Count)
            return false;

        var type = x.GetType();
        if (type == y.GetType() && type.GetGenericTypeDefinition() == typeof(List<>))
        {
            var underlying = type.GetGenericArguments()[0];
            var equalsMethod = _typedListEqualsMethod.MakeGenericMethod(underlying);
            return (bool)equalsMethod.Invoke(null, [x, y]);
        }
        return EqualsSlow(x, y);
    }
    /// <inheritdoc/>
    public bool Equals(ICollection x, ICollection y)
    {
        if (x.Count != y.Count)
            return false;
        return EqualsSlow(x, y);
    }
    /// <inheritdoc/>
    public new bool Equals(object x, object y) => ObjectsEqualCore(x, y);
    /// <summary>
    /// Determines sequence equality by advancing enumerators for both <see cref="IEnumerable"/>s simultaneously and comparing each element at corresponding positions in the sequence.
    /// </summary>
    private bool EqualsSlow(IEnumerable x, IEnumerable y)
    {
        var e1 = x?.GetEnumerator();
        var e2 = y?.GetEnumerator();
        try
        {
            if (e1 is null || e2 is null)
                return e1 == e2;

            while (true)
            {
                var advanced1 = e1.MoveNext();
                var advanced2 = e2.MoveNext();
                if (advanced1 != advanced2)
                    return false;
                if (!advanced1)
                    return true;
                if (!_inner.Equals(e1.Current, e2.Current))
                    return false;
            }
        }
        finally
        {
            if (e1 is IDisposable d1)
                d1.Dispose();
            if (e2 is IDisposable d2)
                d2.Dispose();
        }
    }
    private bool ObjectsEqualCore(object x, object y)
    {
        if (x is null || y is null)
            return x == y;

        switch (x)
        {
            case Array left:
            {
                if (y is Array right)
                    return Equals(left, right);
                return EqualsSlow(left, y as IEnumerable);
            }
            case IList left:
            {
                if (y is IList right)
                    return Equals(left, right);
                return EqualsSlow(left, y as IEnumerable);
            }
            case ICollection left:
            {
                if (y is ICollection right)
                    return Equals(left, right);
                return EqualsSlow(left, y as IEnumerable);
            }
            case IEnumerable left:
            {
                if (y is IEnumerable right)
                    return Equals(left, right);
                return EqualsSlow(left, y as IEnumerable);
            }
            default:
            {
                throw new NotSupportedException($"The type {x.GetType().FullName} is not supported by {nameof(SequenceEqualityComparer)}.");
            }
        }
    }

    /// <inheritdoc/>
    public int GetHashCode([DisallowNull] IEnumerable obj)
    {
        var enumerator = obj.GetEnumerator();
        return HashEnumerator(enumerator);
    }
    /// <inheritdoc/>
    public int GetHashCode([DisallowNull] Array obj)
    {
        var enumerator = obj.GetEnumerator();
        return HashEnumerator(enumerator);
    }
    /// <inheritdoc/>
    public int GetHashCode([DisallowNull] IList obj)
    {
        var enumerator = obj.GetEnumerator();
        return HashEnumerator(enumerator);
    }
    /// <inheritdoc/>
    public int GetHashCode([DisallowNull] ICollection obj)
    {
        var enumerator = obj.GetEnumerator();
        return HashEnumerator(enumerator);
    }
    /// <inheritdoc/>
    public int GetHashCode(object obj)
    {
        return obj switch
        {
            Array array => GetHashCode(array),
            IList list => GetHashCode(list),
            ICollection collection => GetHashCode(collection),
            IEnumerable enumerable => GetHashCode(enumerable),
            _ => throw new NotSupportedException($"The type {obj.GetType().FullName} is not supported by {nameof(SequenceEqualityComparer)}."),
        };
    }
    /// <summary>
    /// Uses <see cref="HashCode"/> to combine every element the specified <paramref name="enumerator"/> visits while it is enumerated.
    /// </summary>
    private static int HashEnumerator(IEnumerator enumerator)
    {
        var hc = new HashCode();
        while (enumerator.MoveNext())
        {
            var x = enumerator.Current;
            hc.Add(x);
        }
        return hc.ToHashCode();
    }

    #region Helpers
    private static readonly MethodInfo _typedArrayEqualsMethod = typeof(SequenceEqualityComparer).GetMethod(nameof(TypedArrayEquals), BindingFlags.NonPublic | BindingFlags.Static);
    private static readonly MethodInfo _typedListEqualsMethod = typeof(SequenceEqualityComparer).GetMethod(nameof(TypedListEquals), BindingFlags.NonPublic | BindingFlags.Static);
    private static bool TypedArrayEquals<T>(T[] x, T[] y) => x.AsSpan().SequenceEqual(y.AsSpan());
    private static bool TypedListEquals<T>(List<T> x, List<T> y) => UnsafeUtils.Accessors.ListAccessors<T>._items(x).AsSpan(0, x.Count).SequenceEqual(UnsafeUtils.Accessors.ListAccessors<T>._items(y).AsSpan(0, y.Count));
    #endregion
}

#if false
/// <inheritdoc/>
public sealed class SequenceEqualityComparer<T> : IEqualityComparer<ICollection<T>>, IEqualityComparer<IList<T>>, IEqualityComparer<T[]>, IEqualityComparer<ISet<T>>, IEqualityComparer<IReadOnlySet<T>>, IEqualityComparer<IEnumerable<T>>, IEqualityComparer
{
}
#endif
