using System.Collections;
using System.Runtime.CompilerServices;

namespace LaquaiLib.Wrappers;

/// <summary>
/// Simplifies the creation of a hash code from a collection of objects.
/// With reference to the same objects, this class will always return exactly the same result as creating a <see cref="HashCode"/> and adding objects to it one by one.
/// </summary>
[CollectionBuilder(typeof(CollectionHashCode), "Create")]
public readonly struct CollectionHashCode
    : IEnumerable<object>,
    IEquatable<object>,
    IEquatable<CollectionHashCode>,
    IEquatable<int>
{
    private readonly List<object> _references = [];
    private readonly HashCode _hashCode;

    /// <inheritdoc/>
    public readonly IEnumerator<object> GetEnumerator() => _references.GetEnumerator();
    readonly IEnumerator IEnumerable.GetEnumerator() => _references.GetEnumerator();

    /// <summary>
    /// Creates a new <see cref="CollectionHashCode"/> and adds the given objects to the internal <see cref="HashCode"/>.
    /// </summary>
    /// <param name="parameters">The objects to add to the internal <see cref="HashCode"/>.</param>
    /// <returns>The created <see cref="CollectionHashCode"/>.</returns>
    public static CollectionHashCode Create(params object[] parameters)
    {
        var chc = new CollectionHashCode();
        chc.AddRange(parameters);
        return chc;
    }

    /// <summary>
    /// Instantiates a new <see cref="CollectionHashCode"/> and adds the given objects to the internal <see cref="HashCode"/>.
    /// </summary>
    /// <param name="objects">The objects to add to the internal <see cref="HashCode"/>.</param>
    public CollectionHashCode(params object[] objects)
    {
        ArgumentNullException.ThrowIfNull(objects, nameof(objects));
        foreach (var obj in objects)
        {
            _hashCode.Add(obj);
        }
    }
    /// <summary>
    /// Instantiates a new <see cref="CollectionHashCode"/> and adds the given objects to the internal <see cref="HashCode"/>.
    /// </summary>
    /// <param name="objects">The objects to add to the internal <see cref="HashCode"/>.</param>
    public CollectionHashCode(IEnumerable<object> objects)
    {
        ArgumentNullException.ThrowIfNull(objects, nameof(objects));
        foreach (var obj in objects)
        {
            _hashCode.Add(obj);
        }
    }

    /// <summary>
    /// Adds a single object to the internal <see cref="HashCode"/>.
    /// </summary>
    /// <param name="obj">The object to add to the internal <see cref="HashCode"/>.</param>
    public void Add(object obj) => _hashCode.Add(obj);
    /// <summary>
    /// Adds one or more objects to the internal <see cref="HashCode"/>.
    /// </summary>
    /// <param name="objects">The objects to add to the internal <see cref="HashCode"/>.</param>
    public void AddRange(params object[] objects)
    {
        ArgumentNullException.ThrowIfNull(objects, nameof(objects));
        foreach (var obj in objects)
        {
            _hashCode.Add(obj);
        }
    }
    /// <summary>
    /// Adds objects from a sequence to the internal <see cref="HashCode"/>.
    /// </summary>
    /// <param name="objects">The objects to add to the internal <see cref="HashCode"/>.</param>
    public void AddRange(IEnumerable<object> objects)
    {
        ArgumentNullException.ThrowIfNull(objects, nameof(objects));
        foreach (var obj in objects)
        {
            _hashCode.Add(obj);
        }
    }

    /// <summary>
    /// Calculates the final hash code of this instance after consecutive <see cref="Add(object)"/>, <see cref="AddRange(IEnumerable{object})"/> or <see cref="AddRange(IEnumerable{object})"/> calls.
    /// </summary>
    /// <returns></returns>
    public readonly int ToHashCode() => _hashCode.ToHashCode();

    #region Equality overrides
    /// <summary>
    /// Indicates whether this <see cref="CollectionHashCode"/> instance is equal to another <see cref="object"/>.
    /// </summary>
    /// <param name="obj">The other <see cref="object"/> instance.</param>
    /// <returns><see langword="true"/> if this instance is equal to <paramref name="obj"/>, otherwise <see langword="false"/>.</returns>
    public override readonly bool Equals(object? obj) => obj is CollectionHashCode other && ToHashCode() == other.ToHashCode();

    /// <summary>
    /// Computes the hash code of this <see cref="CollectionHashCode"/> instance. Since it represents its contained objects, it will always return the same result as <see cref="ToHashCode"/>.
    /// </summary>
    /// <returns>The result of <see cref="HashCode.ToHashCode"/> of the internal <see cref="HashCode"/> instance.</returns>
    public override readonly int GetHashCode() => ToHashCode();

    /// <summary>
    /// Indicates whether two <see cref="CollectionHashCode"/> instances are equal.
    /// </summary>
    /// <param name="left">The first <see cref="CollectionHashCode"/> instance.</param>
    /// <param name="right">The second <see cref="CollectionHashCode"/> instance.</param>
    /// <returns><see langword="true"/> if <paramref name="left"/> is equal to <paramref name="right"/>, otherwise <see langword="false"/>.</returns>
    public static bool operator ==(CollectionHashCode left, CollectionHashCode right) => left.Equals(right);
    /// <summary>
    /// Indicates whether two <see cref="CollectionHashCode"/> instances are not equal.
    /// </summary>
    /// <param name="left">The first <see cref="CollectionHashCode"/> instance.</param>
    /// <param name="right">The second <see cref="CollectionHashCode"/> instance.</param>
    /// <returns><see langword="true"/> if <paramref name="left"/> is not equal to <paramref name="right"/>, otherwise <see langword="false"/>.</returns>
    public static bool operator !=(CollectionHashCode left, CollectionHashCode right) => !(left == right);

    /// <summary>
    /// Indicates whether this <see cref="CollectionHashCode"/> instance is equal to another <see cref="CollectionHashCode"/> instance.
    /// </summary>
    /// <param name="other">The other <see cref="CollectionHashCode"/> instance.</param>
    /// <returns><see langword="true"/> if this instance is equal to <paramref name="other"/>, otherwise <see langword="false"/>.</returns>
    public readonly bool Equals(CollectionHashCode other) => Equals((object)other);
    /// <summary>
    /// Indicates whether the hash code of this <see cref="CollectionHashCode"/> instance is equal to another hash code.
    /// </summary>
    /// <param name="hashCode">The other hash code.</param>
    /// <returns><see langword="true"/> if this instance is equal to <paramref name="hashCode"/>, otherwise <see langword="false"/>.</returns>
    public readonly bool Equals(int hashCode) => ToHashCode() == hashCode;
    #endregion
}
