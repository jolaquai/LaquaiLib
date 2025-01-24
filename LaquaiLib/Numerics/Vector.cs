using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;

using LaquaiLib.Interfaces;

namespace LaquaiLib.Numerics;

/// <summary>
/// Represents a vector of arbitrary dimensions.
/// <typeparamref name="T"/> must implement <see cref="INumber{TSelf}"/>.
/// </summary>
public readonly struct Vector<T> : IEnumerable<T>,
    IEquatable<Vector<T>>,
    IStructuralEquatable,
    ICloneable<Vector<T>>,
    IMultiplyOperators<Vector<T>, Vector<T>, Vector<T>>,
    IMultiplyOperators<Vector<T>, T, Vector<T>>,
    IAdditionOperators<Vector<T>, Vector<T>, Vector<T>>,
    ISubtractionOperators<Vector<T>, Vector<T>, Vector<T>>
    where T : INumber<T>
{
    /// <summary>
    /// Gets the values of the <see cref="Vector{T}"/>.
    /// </summary>
    private readonly T[] _values;

    /// <summary>
    /// Gets the coordinate at the specified <paramref name="index"/>.
    /// </summary>
    /// <param name="index">The index of the coordinate to get.</param>
    /// <returns>The coordinate at the specified <paramref name="index"/>.</returns>
    public T this[int index] => _values[index];
    /// <summary>
    /// Gets the dimension of the <see cref="Vector{T}"/>.
    /// </summary>
    public int Dimension => _values.Length;
    /// <summary>
    /// Gets whether this <see cref="Vector{T}"/> is equivalent to a null vector (that is, all values are zero).
    /// </summary>
    public bool IsNull => _values.All(v => v == T.Zero);

    // Always copy the values to prevent modification of the vector
    /// <summary>
    /// Creates a new <see cref="Vector{T}"/> using the specified values.
    /// </summary>
    /// <param name="values">The coordinates of the vector.</param>
    public Vector(params T[] values) => _values = values.ToArray();
    /// <summary>
    /// Creates a new <see cref="Vector{T}"/> using the specified values.
    /// </summary>
    /// <param name="values">The coordinates of the vector.</param>
    public Vector(IEnumerable<T> values) => _values = values.ToArray();
    /// <summary>
    /// Creates a new <see cref="Vector{T}"/> using the specified values.
    /// </summary>
    /// <param name="values">The coordinates of the vector.</param>
    public Vector(ReadOnlySpan<T> values) => _values = values.ToArray();

    /// <summary>
    /// Creates a copy of the <see cref="Vector{T}"/>'s coordinates and returns them as an array.
    /// </summary>
    /// <returns>A copy of the <see cref="Vector{T}"/>'s coordinates as an array.</returns>
    public T[] ToArray() => _values.ToArray();

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
    public static Vector<T> operator *(Vector<T> left, T right) => new Vector<T>(left._values.Select(v => v * right));
    /// <summary>
    /// Adds two <see cref="Vector{T}"/>s.
    /// </summary>
    /// <param name="left">The first vector to add.</param>
    /// <param name="right">The second vector to add.</param>
    /// <returns>A new <see cref="Vector{T}"/> that is the result of the addition.</returns>
    public static Vector<T> operator +(Vector<T> left, Vector<T> right) => new Vector<T>(left._values.Zip(right._values, (l, r) => l + r));
    /// <summary>
    /// Subtracts a <see cref="Vector{T}"/> from another.
    /// </summary>
    /// <param name="left">The vector to subtract from.</param>
    /// <param name="right">The vector to subtract.</param>
    /// <returns>A new <see cref="Vector{T}"/> that is the result of the subtraction.</returns>
    public static Vector<T> operator -(Vector<T> left, Vector<T> right) => new Vector<T>(left._values.Zip(right._values, (l, r) => l - r));
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
    /// Creates a new <see cref="Vector{T}"/> with all values set to zero (a null vector) of the specified dimension.
    /// </summary>
    /// <param name="dimensions">The number of dimensions the new <see cref="Vector{T}"/> should have.</param>
    /// <returns>A new <see cref="Vector{T}"/> with all values set to zero.</returns>
    public static Vector<T> Null(int dimensions) => new Vector<T>(Enumerable.Repeat(T.Zero, dimensions));

    /// <summary>
    /// Calculates the cross product (vector product) of this <see cref="Vector{T}"/> and another.
    /// </summary>
    /// <param name="other">The <see cref="Vector{T}"/> to calculate the cross product with.</param>
    /// <returns>A new <see cref="Vector{T}"/> that is the result of the cross product.</returns>
    public Vector<T> CrossProduct(Vector<T> other)
    {
        if (_values.Length != 3 || other._values.Length != 3)
        {
            throw new InvalidOperationException("The cross product is only defined for 3-dimensional vectors.");
        }

        return new Vector<T>(
            (_values[1] * other._values[2]) - (_values[2] * other._values[1]),
            (_values[2] * other._values[0]) - (_values[0] * other._values[2]),
            (_values[0] * other._values[1]) - (_values[1] * other._values[0])
        );
    }
    /// <summary>
    /// Calculates the dot product (scalar product) of this <see cref="Vector{T}"/> and another.
    /// </summary>
    /// <param name="other">The <see cref="Vector{T}"/> to calculate the dot product with.</param>
    /// <returns>A new <see cref="Vector{T}"/> that is the result of the dot product.</returns>
    public Vector<T> DotProduct(Vector<T> other)
    {
        if (_values.Length != other._values.Length)
        {
            throw new InvalidOperationException($"The dot product is only defined for vectors of the same dimension. Use {nameof(DotProductUnequal)} to allow for vectors of different dimensions.");
        }
        return new Vector<T>(_values.Zip(other._values, (l, r) => l * r));
    }
    /// <summary>
    /// Calculates the dot product (scalar product) of this <see cref="Vector{T}"/> and another, allowing for <see cref="Vector{T}"/>s of different dimensions.
    /// The missing values are filled with <see cref="INumberBase{TSelf}.Zero"/> of <typeparamref name="T"/>.
    /// </summary>
    /// <param name="other">The <see cref="Vector{T}"/> to calculate the dot product with.</param>
    /// <returns>A new <see cref="Vector{T}"/> that is the result of the dot product.</returns>
    public Vector<T> DotProductUnequal(Vector<T> other)
    {
        var larger = _values.Length > other._values.Length ? _values : other._values;
        var smaller = _values.Length > other._values.Length ? other._values : _values;
        var filler = Enumerable.Repeat(T.Zero, larger.Length - smaller.Length);
        return new Vector<T>(larger.Zip([.. smaller, .. filler], (l, r) => l * r));
    }
    /// <summary>
    /// Calculates the triple product of this <see cref="Vector{T}"/> and two others.
    /// </summary>
    /// <param name="a">The first <see cref="Vector{T}"/> to calculate the triple product with.</param>
    /// <param name="b">The second <see cref="Vector{T}"/> to calculate the triple product with.</param>
    /// <returns>A new <see cref="Vector{T}"/> that is the result of the triple product.</returns>
    public Vector<T> TripleProduct(Vector<T> a, Vector<T> b) => CrossProduct(a).DotProduct(b);

    /// <summary>
    /// Determines if this and the passed <see cref="Vector{T}"/> are linearly dependent.
    /// </summary>
    /// <param name="comp">The first <see cref="Vector{T}"/>.</param>
    /// <returns><see langword="true"/> if the vectors are linearly dependent, otherwise <see langword="false"/>.</returns>
    public bool IsLinearlyDependent(Vector<T> comp);
    /// <summary>
    /// Determines if this and the two passed <see cref="Vector{T}"/> are linearly dependent.
    /// </summary>
    /// <param name="first">The first <see cref="Vector{T}"/>.</param>
    /// <param name="second">The second <see cref="Vector{T}"/>.</param>
    /// <returns><see langword="true"/> if the vectors are linearly dependent, otherwise <see langword="false"/>.</returns>
    public bool IsLinearlyDependent(Vector<T> first, Vector<T> second);
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
    public Vector<T> Expand(int dimensions) => new Vector<T>([.. _values, .. Enumerable.Repeat(T.Zero, dimensions - _values.Length)]);
    /// <summary>
    /// Creates a new <see cref="Vector{T}"/> that has the same values as this <see cref="Vector{T}"/>, with the specified values appended.
    /// </summary>
    /// <param name="values">The values to append to the new <see cref="Vector{T}"/>.</param>
    /// <returns>A new <see cref="Vector{T}"/> that has the same values as this <see cref="Vector{T}"/>, with the specified values appended.</returns>
    public Vector<T> Expand(params T[] values) => new Vector<T>([.. _values, .. values]);
    /// <inheritdoc cref="Expand(T[])"/>
    public Vector<T> Expand(IEnumerable<T> values) => new Vector<T>([.. _values, .. values]);
    /// <inheritdoc cref="Expand(T[])"/>
    public Vector<T> Expand(ReadOnlySpan<T> values) => new Vector<T>([.. _values, .. values]);
    /// <summary>
    /// Creates a new <see cref="Vector{T}"/> that has its coordinates reduced as much as possible.
    /// This is really only helpful for normal vectors of surfaces or the <see cref="CrossProduct(Vector{T})"/> of two vectors if used for the sole purpose of finding a perpendicular vector, for example.
    /// </summary>
    /// <returns>A new <see cref="Vector{T}"/> that has its coordinates reduced as much as possible.</returns>
    public Vector<T> Simplify();

    /// <inheritdoc/>
    public bool Equals(Vector<T> other) => _values.SequenceEqual(other._values);
    /// <summary>
    /// Checks if all passed <see cref="Vector{T}"/> are equal to this vector (and as such, each other).
    /// </summary>
    /// <param name="others">The <see cref="Vector{T}"/> to check for equality.</param>
    /// <returns><see langword="true"/> if all <see cref="Vector{T}"/> are equal to this vector, otherwise <see langword="false"/>.</returns>
    public bool Equals(params ReadOnlySpan<Vector<T>> others)
    {
        for (var i = 0; i < others.Length; i++)
        {
            if (!_values.SequenceEqual(others[i]))
            {
                return false;
            }
        }
        return true;
    }
    /// <inheritdoc/>
    public override bool Equals(object obj) => obj is Vector<T> other && Equals(other);
    /// <inheritdoc/>
    public override int GetHashCode() => _values.GetHashCode();
    bool IStructuralEquatable.Equals(object other, IEqualityComparer comparer)
    {
        ArgumentNullException.ThrowIfNull(comparer);
        if (other is Vector<T> vector && vector._values.Length == _values.Length)
        {
            for (var i = 0; i < _values.Length; i++)
            {
                if (!comparer.Equals(_values[i], vector._values[i]))
                {
                    return false;
                }
            }
            return true;
        }

        return false;
    }
    int IStructuralEquatable.GetHashCode(IEqualityComparer comparer)
    {
        ArgumentNullException.ThrowIfNull(comparer);

        var hc = default(HashCode);
        // Stolen straight from Array.IStructuralEquatable.GetHashCode
        for (var i = _values.Length >= 8 ? _values.Length - 8 : 0; i < _values.Length; i++)
        {
            hc.Add(comparer.GetHashCode(_values[i]));
        }

        return hc.ToHashCode();
    }

    /// <inheritdoc/>
    public IEnumerator<T> GetEnumerator() => (IEnumerator<T>)_values.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    /// <inheritdoc/>
    public Vector<T> Clone() => new Vector<T>(_values);
}
