using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;

using LaquaiLib.Interfaces;

namespace LaquaiLib.Numerics;

/// <summary>
/// Represents a vector of arbitrary dimensions.
/// <typeparamref name="T"/> must implement <see cref="ISignedNumber{TSelf}"/>.
/// </summary>
public readonly struct Vector<T> : IEnumerable<T>,
    IEquatable<Vector<T>>,
    IStructuralEquatable,
    ICloneable<Vector<T>>,
    IAdditionOperators<Vector<T>, Vector<T>, Vector<T>>,
    ISubtractionOperators<Vector<T>, Vector<T>, Vector<T>>,
    IMultiplyOperators<Vector<T>, Vector<T>, Vector<T>>,
    IMultiplyOperators<Vector<T>, T, Vector<T>>,
    IDivisionOperators<Vector<T>, T, Vector<T>>
    where T : ISignedNumber<T>, IComparisonOperators<T, T, bool>, IModulusOperators<T, T, T>
{
    private readonly T[] _coordinates;

    /// <summary>
    /// Gets the coordinate at the specified <paramref name="index"/>.
    /// </summary>
    /// <param name="index">The index of the coordinate to get.</param>
    /// <returns>The coordinate at the specified <paramref name="index"/>.</returns>
    public T this[int index] => _coordinates[index];
    /// <summary>
    /// Gets the dimensionality of the <see cref="Vector{T}"/>.
    /// </summary>
    public int Dimension => _coordinates.Length;
    /// <summary>
    /// Gets whether this <see cref="Vector{T}"/> is equivalent to a null vector (that is, all values are zero).
    /// </summary>
    public bool IsZero => _coordinates.All(static v => v == T.Zero);

    // Always copy the values to prevent modification of the vector
    /// <summary>
    /// Creates a new <see cref="Vector{T}"/> using the specified values.
    /// </summary>
    /// <param name="values">The coordinates of the vector.</param>
    public Vector(params T[] values)
    {
        if (values.Length == 0)
        {
            throw new ArgumentException("At least one value must be provided.", nameof(values));
        }

        _coordinates = System.Runtime.CompilerServices.Unsafe.As<T[]>(values.Clone());
    }
    /// <summary>
    /// Creates a new <see cref="Vector{T}"/> using the specified values.
    /// </summary>
    /// <param name="values">The coordinates of the vector.</param>
    public Vector(params IEnumerable<T> values)
    {
        if (!values.Any())
        {
            throw new ArgumentException("At least one value must be provided.", nameof(values));
        }

        _coordinates = [.. values];
    }
    /// <summary>
    /// Creates a new <see cref="Vector{T}"/> using the specified values.
    /// </summary>
    /// <param name="values">The coordinates of the vector.</param>
    public Vector(params ReadOnlySpan<T> values)
    {
        if (values.Length == 0)
        {
            throw new ArgumentException("At least one value must be provided.", nameof(values));
        }

        _coordinates = values.ToArray();
    }

    /// <summary>
    /// Creates a copy of the <see cref="Vector{T}"/>'s coordinates and returns them as an array.
    /// </summary>
    /// <returns>A copy of the <see cref="Vector{T}"/>'s coordinates as an array.</returns>
    public T[] ToArray() => System.Runtime.CompilerServices.Unsafe.As<T[]>(_coordinates.Clone());

    /// <summary>
    /// Adds two <see cref="Vector{T}"/>s.
    /// </summary>
    /// <param name="left">The first vector to add.</param>
    /// <param name="right">The second vector to add.</param>
    /// <returns>A new <see cref="Vector{T}"/> that is the result of the addition.</returns>
    public static Vector<T> operator +(Vector<T> left, Vector<T> right) => new Vector<T>(left._coordinates.Zip(right._coordinates, static (l, r) => l + r));
    /// <summary>
    /// Subtracts a <see cref="Vector{T}"/> from another.
    /// </summary>
    /// <param name="left">The vector to subtract from.</param>
    /// <param name="right">The vector to subtract.</param>
    /// <returns>A new <see cref="Vector{T}"/> that is the result of the subtraction.</returns>
    public static Vector<T> operator -(Vector<T> left, Vector<T> right) => new Vector<T>(left._coordinates.Zip(right._coordinates, static (l, r) => l - r));
    // I agree, defining this and making it throw unconditionally is ugly, but I want implicit choosing of a multiplication method impossible by design
    /// <summary>
    /// Throws a <see cref="NotSupportedException"/> as vector multiplication has two definitions.
    /// </summary>
    [DoesNotReturn]
    [Obsolete($"Vector multiplication has two definitions; use the methods {nameof(DotProduct)} and {nameof(CrossProduct)} instead.", true)]
    public static Vector<T> operator *(Vector<T> left, Vector<T> right) => throw new NotSupportedException($"Vector multiplication has two definitions; use the methods {nameof(DotProduct)} and {nameof(CrossProduct)} instead.");
    /// <summary>
    /// Multiplies a <see cref="Vector{T}"/> by a scalar.
    /// </summary>
    /// <param name="left">The vector to multiply.</param>
    /// <param name="right">The scalar to multiply by.</param>
    /// <returns>A new <see cref="Vector{T}"/> that is the result of the multiplication.</returns>
    public static Vector<T> operator *(Vector<T> left, T right) => new Vector<T>(left._coordinates.Select(v => v * right));
    /// <summary>
    /// Divides a <see cref="Vector{T}"/> by a scalar.
    /// </summary>
    /// <param name="left">The vector to divide.</param>
    /// <param name="right">The scalar to divide by.</param>
    /// <returns>A new <see cref="Vector{T}"/> that is the result of the division.</returns>
    public static Vector<T> operator /(Vector<T> left, T right) => new Vector<T>(left._coordinates.Select(v => v / right));
    /// <summary>
    /// Determines if this <see cref="Vector{T}"/> is equal to another.
    /// </summary>
    /// <param name="left">The first <see cref="Vector{T}"/>.</param>
    /// <param name="right">The second <see cref="Vector{T}"/>.</param>
    /// <returns><see langword="true"/> if the vectors are equal, otherwise <see langword="false"/>.</returns>
    public static bool operator ==(Vector<T> left, Vector<T> right) => left.Equals(right);
    /// <summary>
    /// Determines if this <see cref="Vector{T}"/> is not equal to another.
    /// </summary>
    /// <param name="left">The first <see cref="Vector{T}"/>.</param>
    /// <param name="right">The second <see cref="Vector{T}"/>.</param>
    /// <returns><see langword="true"/> if the vectors are not equal, otherwise <see langword="false"/>.</returns>
    public static bool operator !=(Vector<T> left, Vector<T> right) => !(left == right);

    /// <summary>
    /// Creates a new <see cref="Vector{T}"/> with all values set to zero (a zero vector) of the specified dimension.
    /// </summary>
    /// <param name="dimensions">The number of dimensions the new <see cref="Vector{T}"/> should have.</param>
    /// <returns>A new <see cref="Vector{T}"/> with all values set to zero.</returns>
    public static Vector<T> Zero(int dimensions) => new Vector<T>(Enumerable.Repeat(T.Zero, dimensions));

    /// <summary>
    /// Calculates the cross product (vector product) of this <see cref="Vector{T}"/> and another.
    /// </summary>
    /// <param name="other">The <see cref="Vector{T}"/> to calculate the cross product with.</param>
    /// <returns>A new <see cref="Vector{T}"/> that is the result of the cross product.</returns>
    public Vector<T> CrossProduct(Vector<T> other)
    {
        if (_coordinates.Length != 3 || other._coordinates.Length != 3)
        {
            throw new InvalidOperationException("The cross product is only defined for 3-dimensional vectors.");
        }

        return new Vector<T>(
            (_coordinates[1] * other._coordinates[2]) - (_coordinates[2] * other._coordinates[1]),
            (_coordinates[2] * other._coordinates[0]) - (_coordinates[0] * other._coordinates[2]),
            (_coordinates[0] * other._coordinates[1]) - (_coordinates[1] * other._coordinates[0])
        );
    }
    /// <summary>
    /// Calculates the dot product (scalar product) of this <see cref="Vector{T}"/> and another.
    /// </summary>
    /// <param name="other">The <see cref="Vector{T}"/> to calculate the dot product with.</param>
    /// <returns>A new <see cref="Vector{T}"/> that is the result of the dot product.</returns>
    public T DotProduct(Vector<T> other)
    {
        if (_coordinates.Length != other._coordinates.Length)
        {
            throw new InvalidOperationException($"The dot product is only defined for vectors of the same dimension. Use {nameof(DotProductUnequal)} to allow for vectors of different dimensions.");
        }
        var vals = _coordinates;
        return RandomMath.Sum(1, _coordinates.Length, i => vals[i] * other._coordinates[i]);
    }
    /// <summary>
    /// Calculates the dot product (scalar product) of this <see cref="Vector{T}"/> and another, allowing for <see cref="Vector{T}"/>s of different dimensions.
    /// The missing values are filled with <see cref="INumberBase{TSelf}.Zero"/> of <typeparamref name="T"/>.
    /// </summary>
    /// <param name="other">The <see cref="Vector{T}"/> to calculate the dot product with.</param>
    /// <returns>A new <see cref="Vector{T}"/> that is the result of the dot product.</returns>
    public T DotProductUnequal(Vector<T> other)
    {
        var larger = _coordinates.Length > other._coordinates.Length ? _coordinates : other._coordinates;
        var smaller = _coordinates.Length > other._coordinates.Length ? other._coordinates : _coordinates;
        var filler = Enumerable.Repeat(T.Zero, larger.Length - smaller.Length);
        return RandomMath.Sum(1, larger.Length, i => (i < smaller.Length ? smaller[i] : T.Zero) * larger[i]);
    }
    /// <summary>
    /// Calculates the triple product of this <see cref="Vector{T}"/> and two others.
    /// </summary>
    /// <param name="a">The first <see cref="Vector{T}"/> to calculate the triple product with.</param>
    /// <param name="b">The second <see cref="Vector{T}"/> to calculate the triple product with.</param>
    /// <returns>A new <see cref="Vector{T}"/> that is the result of the triple product.</returns>
    public T TripleProduct(Vector<T> a, Vector<T> b) => CrossProduct(a).DotProduct(b);

    /// <summary>
    /// Determines if this and the passed <see cref="Vector{T}"/> are linearly dependent.
    /// </summary>
    /// <param name="comp">The first <see cref="Vector{T}"/>.</param>
    /// <returns><see langword="true"/> if the vectors are linearly dependent, otherwise <see langword="false"/>.</returns>
    public bool IsLinearlyDependent(Vector<T> comp)
    {
        if (this == comp)
        {
            return true;
        }
        var coordinates = _coordinates.Zip(comp._coordinates, static (l, r) => l / r);
        using var enumerator = coordinates.GetEnumerator();
        if (!enumerator.MoveNext())
        {
            return false;
        }

        var first = enumerator.Current;
        while (enumerator.MoveNext())
        {
            if (!enumerator.Current.Equals(first))
            {
                return false;
            }
        }
        return true;
    }
    /// <summary>
    /// Determines if this and the two passed <see cref="Vector{T}"/> are linearly dependent.
    /// </summary>
    /// <param name="first">The first <see cref="Vector{T}"/>.</param>
    /// <param name="second">The second <see cref="Vector{T}"/>.</param>
    /// <returns><see langword="true"/> if the vectors are linearly dependent, otherwise <see langword="false"/>.</returns>
    public bool IsLinearlyDependent(Vector<T> first, Vector<T> second) => TripleProduct(first, second) == T.Zero;
    /// <summary>
    /// Determines if this and the three passed <see cref="Vector{T}"/> are linearly dependent.
    /// This always returns <see langword="true"/> in 3D space.
    /// </summary>
    /// <param name="first">The first <see cref="Vector{T}"/>.</param>
    /// <param name="second">The second <see cref="Vector{T}"/>.</param>
    /// <param name="third">The third <see cref="Vector{T}"/>.</param>
    /// <returns><see langword="true"/> if the vectors are linearly dependent, otherwise <see langword="false"/>.</returns>
    [SuppressMessage("Style", "IDE0060")]
    public bool IsLinearlyDependent(Vector<T> first, Vector<T> second, Vector<T> third) => true;

    /// <summary>
    /// Creates a new <see cref="Vector{T}"/> that has the same values as this <see cref="Vector{T}"/>, with the specified number of dimensions.
    /// All missing values are filled with the default value of <typeparamref name="T"/>.
    /// </summary>
    /// <param name="dimensions">The number of dimensions the new <see cref="Vector{T}"/> should have.</param>
    /// <returns>A new <see cref="Vector{T}"/> that has the same values as this <see cref="Vector{T}"/>, with the specified number of dimensions.</returns>
    public Vector<T> Expand(int dimensions) => new Vector<T>([.. _coordinates, .. Enumerable.Repeat(T.Zero, dimensions - _coordinates.Length)]);
    /// <summary>
    /// Creates a new <see cref="Vector{T}"/> that has the same values as this <see cref="Vector{T}"/>, with the specified values appended.
    /// </summary>
    /// <param name="values">The values to append to the new <see cref="Vector{T}"/>.</param>
    /// <returns>A new <see cref="Vector{T}"/> that has the same values as this <see cref="Vector{T}"/>, with the specified values appended.</returns>
    public Vector<T> Expand(params T[] values) => new Vector<T>([.. _coordinates, .. values]);
    /// <inheritdoc cref="Expand(T[])"/>
    public Vector<T> Expand(IEnumerable<T> values) => new Vector<T>([.. _coordinates, .. values]);
    /// <inheritdoc cref="Expand(T[])"/>
    public Vector<T> Expand(ReadOnlySpan<T> values) => new Vector<T>([.. _coordinates, .. values]);
    /// <summary>
    /// Creates a new <see cref="Vector{T}"/> that has its coordinates reduced as much as possible.
    /// This is really only helpful for normal vectors of surfaces or the <see cref="CrossProduct(Vector{T})"/> of two vectors if used for the sole purpose of finding a perpendicular vector, for example.
    /// </summary>
    /// <returns>A new <see cref="Vector{T}"/> that has its coordinates reduced as much as possible.</returns>
    public Vector<T> Simplify()
    {
        var gcd = RandomMath.GCD(_coordinates);
        if (gcd == T.One)
        {
            return this;
        }
        return new Vector<T>(_coordinates.Select(v => v / gcd));
    }
    /// <summary>
    /// Creates a new <see cref="Vector{T}"/> that has the last <paramref name="count"/> coordinates removed.
    /// </summary>
    /// <param name="count">The number of dimensions to remove.</param>
    /// <returns>A new <see cref="Vector{T}"/> that has the last <paramref name="count"/> coordinates removed.</returns>
    public Vector<T> Reduce(int count) => new Vector<T>(_coordinates.AsSpan()[..^count]);
    /// <summary>
    /// Creates a new <see cref="Vector{T}"/> that contains only the first <paramref name="count"/> coordinates.
    /// </summary>
    /// <param name="count">The number of dimensions to keep.</param>
    /// <returns>A new <see cref="Vector{T}"/> that contains only the first <paramref name="count"/> coordinates.</returns>
    public Vector<T> ReduceTo(int count) => new Vector<T>(_coordinates.AsSpan()[..count]);

    /// <summary>
    /// Calculates the absolute value of this <see cref="Vector{T}"/> (that is, the distance from the origin).
    /// </summary>
    /// <returns>The absolute value of this <see cref="Vector{T}"/>.</returns>
    public T Abs()
    {
        var vals = _coordinates;
        return (T)(object)Math.Sqrt((double)(object)RandomMath.Sum(1, vals.Length, i => vals[i] * vals[i]));
    }

    #region Interface implementations
    /// <inheritdoc/>
    public bool Equals(Vector<T> other) => _coordinates.SequenceEqual(other._coordinates);
    /// <summary>
    /// Checks if all passed <see cref="Vector{T}"/> are equal to this vector (and as such, each other).
    /// </summary>
    /// <param name="others">The <see cref="Vector{T}"/> to check for equality.</param>
    /// <returns><see langword="true"/> if all <see cref="Vector{T}"/> are equal to this vector, otherwise <see langword="false"/>.</returns>
    public bool Equals(params ReadOnlySpan<Vector<T>> others)
    {
        for (var i = 0; i < others.Length; i++)
        {
            if (!_coordinates.SequenceEqual(others[i]))
            {
                return false;
            }
        }
        return true;
    }
    /// <inheritdoc/>
    public override bool Equals(object obj) => obj is Vector<T> other && Equals(other);
    /// <inheritdoc/>
    public override int GetHashCode() => ((IStructuralEquatable)this).GetHashCode(EqualityComparer<T>.Default);
    bool IStructuralEquatable.Equals(object other, IEqualityComparer comparer)
    {
        ArgumentNullException.ThrowIfNull(comparer);
        if (other is Vector<T> vector && vector._coordinates.Length == _coordinates.Length)
        {
            if (comparer is IEqualityComparer<T> typedComparer)
            {
                for (var i = 0; i < _coordinates.Length; i++)
                {
                    if (!typedComparer.Equals(_coordinates[i], vector._coordinates[i]))
                    {
                        return false;
                    }
                }
                return true;
            }
            else
            {
                for (var i = 0; i < _coordinates.Length; i++)
                {
                    if (!comparer.Equals(_coordinates[i], vector._coordinates[i]))
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        return false;
    }
    int IStructuralEquatable.GetHashCode(IEqualityComparer comparer)
    {
        ArgumentNullException.ThrowIfNull(comparer);
        var typedComparer = comparer as IEqualityComparer<T>;

        var hc = default(HashCode);
        if (typedComparer is not null)
        {
            // Stolen straight from Array.IStructuralEquatable.GetHashCode
            for (var i = _coordinates.Length >= 8 ? _coordinates.Length - 8 : 0; i < _coordinates.Length; i++)
            {
                hc.Add(typedComparer.GetHashCode(_coordinates[i]));
            }
        }
        else
        {
            // Stolen straight from Array.IStructuralEquatable.GetHashCode
            for (var i = _coordinates.Length >= 8 ? _coordinates.Length - 8 : 0; i < _coordinates.Length; i++)
            {
                hc.Add(comparer.GetHashCode(_coordinates[i]));
            }
        }

        return hc.ToHashCode();
    }

    /// <inheritdoc/>
    public IEnumerator<T> GetEnumerator() => (IEnumerator<T>)_coordinates.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    /// <inheritdoc/>
    public Vector<T> Clone() => new Vector<T>(_coordinates);
    #endregion
}
