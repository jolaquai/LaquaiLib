using System.Diagnostics.CodeAnalysis;
using System.Xml.Linq;

using LaquaiLib.Extensions;

namespace LaquaiLib.Util;

/// <summary>
/// Implements <see cref="IEqualityComparer{T}"/> for <see cref="XElement"/> that ignores the order of attributes and elements, otherwise preserving deep equality rules.
/// </summary>
public sealed class XElementEquivalenceComparer : IEqualityComparer<XElement>
{
    /// <summary>
    /// Gets the singleton instance of this comparer.
    /// </summary>
    public static XElementEquivalenceComparer Instance { get; } = new XElementEquivalenceComparer();
    private XElementEquivalenceComparer() { }

    /// <summary>
    /// Determines if two <see cref="XElement"/> instances are equal, ignoring the order of attributes and elements.
    /// </summary>
    /// <param name="x">The first <see cref="XElement"/> to compare.</param>
    /// <param name="y">The second <see cref="XElement"/> to compare.</param>
    /// <returns><see langword="true"/> if the elements compare as equal, otherwise <see langword="false"/>.</returns>
    public bool Equals(XElement? x, XElement? y)
    {
        if (ReferenceEquals(x, y))
        {
            return true;
        }
        if (x is null || y is null)
        {
            return x == y;
        }

        if (!CompareSingle(x, y))
        {
            return false;
        }

        return x.Descendants().SequenceEquivalent(y.Descendants(), Instance);
    }
    private static bool CompareSingle(XElement x, XElement y)
    {
        if (x.Name != y.Name)
        {
            return false;
        }
        if (x.HasElements != y.HasElements)
        {
            return false;
        }
        if (!x.Attributes().SequenceEquivalent(y.Attributes()))
        {
            return false;
        }
        return true;
    }
    public int GetHashCode([DisallowNull] XElement obj)
    {
        var hash = default(HashCode);
        hash.Add(obj.Name);
        foreach (var attribute in obj.Attributes())
        {
            hash.Add(attribute.Name);
            hash.Add(attribute.Value);
        }
        return hash.ToHashCode();
    }
}
