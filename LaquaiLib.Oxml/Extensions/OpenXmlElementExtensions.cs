using System.Xml.Linq;

using LaquaiLib.Extensions.LinqXml;

namespace LaquaiLib.Oxml.Extensions;

/// <summary>
/// Provides extension methods for <see cref="OpenXmlElement"/> and derived types.
/// </summary>
public static class OpenXmlElementExtensions
{
    /// <summary>
    /// Determines if this element is the only child of its parent.
    /// This takes into account various *Properties elements that should not be counted as children.
    /// </summary>
    /// <param name="element">The element to check.</param>
    /// <returns><see langword="true"/> if <paramref name="element"/> is the only child of its parent, otherwise <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsOnlyChild(this OpenXmlElement element) => element.IsOnlyChildOf(element.Parent);
    /// <summary>
    /// Determines if this element is the only child of <paramref name="potentialParent"/>.
    /// If <paramref name="potentialParent"/> is not the actual parent of <paramref name="element"/>, this method returns <see langword="false"/>.
    /// </summary>
    /// <param name="element">The element to check.</param>
    /// <param name="potentialParent">The potential parent to check against.</param>
    /// <returns><see langword="true"/> if <paramref name="element"/> is the only child of <paramref name="potentialParent"/>, otherwise <see langword="false"/>.</returns>
    public static bool IsOnlyChildOf(this OpenXmlElement element, OpenXmlElement? potentialParent)
    {
        if (element.Parent != potentialParent)
        {
            return false;
        }
        if (element.Parent is null)
        {
            return false;
        }

        var children = element.Parent.ChildElements
            .Where(c => !c.GetType().Name.EndsWith("Properties", StringComparison.Ordinal))
        .ToArray();

        return element.Parent.ChildElements.Count == 1;
    }
    /// <summary>
    /// Determines if this element is the only child of its parent of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of the child element.</typeparam>
    /// <param name="element">The element to check.</param>
    /// <returns>The result of the check.</returns>
    public static bool IsOnlyChildOfType<T>(this T element)
        where T : OpenXmlElement
    {
        if (element.Parent is null)
        {
            return false;
        }

        var children = element.Parent.ChildElements.OfType<T>().ToArray();
        return children.Length == 1;
    }

    /// <summary>
    /// Determines whether two <see cref="OpenXmlElement"/> instances are equal, taking into account their entire element trees.
    /// </summary>
    /// <param name="current">The first <see cref="OpenXmlElement"/> to compare.</param>
    /// <param name="other">The second <see cref="OpenXmlElement"/> to compare.</param>
    /// <returns><see langword="true"/> if the specified <see cref="OpenXmlElement"/> instances are equal, otherwise <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool DeepEquals(this OpenXmlElement current, OpenXmlElement other) => OpenXmlElementComparers.Default.Equals(current, other);
    /// <summary>
    /// Returns a normalized version of the <see cref="OpenXmlElement.OuterXml"/> property, removing all namespace declarations and prefixes, and all RSID attributes.
    /// </summary>
    /// <param name="element">The element to normalize.</param>
    /// <returns>The normalized XML as an <see cref="XElement"/>.</returns>
    public static XElement GetNormalizedOuterXml(this OpenXmlElement element)
    {
        var xElement = XElement.Parse(element.OuterXml);
        // Remove all namespace declarations and ns: prefixes
        foreach (var d in xElement.DescendantsAndSelf())
        {
            d.Name = d.Name.LocalName;
            RemoveNamespaces(xElement);
            RemoveRsid(xElement);
        }
        return xElement;
    }
    private static void RemoveRsid(XElement xElement)
    {
        foreach (var attribute in xElement.Attributes().Where(a => a.Name.LocalName.StartsWith("rsid", StringComparison.OrdinalIgnoreCase)).ToArray())
        {
            attribute.Remove();
        }
        foreach (var descendant in xElement.Descendants())
        {
            RemoveRsid(descendant);
        }
    }
    private static void RemoveNamespaces(XElement xElement)
    {
        foreach (var attribute in xElement.Attributes().Where(a => a.IsNamespaceDeclaration).ToArray())
        {
            attribute.Remove();
        }
        foreach (var attribute in xElement.Attributes().Where(a => a.Name.Namespace != XNamespace.Xml && a.Name.Namespace != XNamespace.None).ToArray())
        {
            xElement.SetAttributeValue(attribute.Name.LocalName, attribute.Value);
            attribute.Remove();
        }
        foreach (var descendant in xElement.Descendants())
        {
            RemoveNamespaces(descendant);
        }
    }
}
