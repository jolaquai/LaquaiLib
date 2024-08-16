using System.Xml.Linq;

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

    /// <summary>
    /// Returns an array of the <see cref="OpenXmlElement"/>s that lie between <paramref name="child1"/> and <paramref name="child2"/> in the child element list of <paramref name="element"/>.
    /// This method fully enumerates the child elements of <paramref name="element"/>.
    /// </summary>
    /// <param name="element">The element to search in.</param>
    /// <param name="child1">The first child element.</param>
    /// <param name="child2">The second child element.</param>
    /// <returns>The array as specified.</returns>
    /// <remarks>
    /// If either specified element is not a child of <paramref name="element"/>, an <see cref="ArgumentException"/> is thrown.
    /// <para/>If <paramref name="child2"/> appears before <paramref name="child1"/> in the child element list, the order of the elements in the returned array is reversed as well.
    /// </remarks>
    public static OpenXmlElement[] ElementsBetween(this OpenXmlElement element, OpenXmlElement child1, OpenXmlElement child2)
    {
        ArgumentNullException.ThrowIfNull(element);
        ArgumentNullException.ThrowIfNull(child1);
        ArgumentNullException.ThrowIfNull(child2);

        var children = element.ChildElements.ToArray();
        var index1 = Array.IndexOf(children, child1);
        var index2 = Array.IndexOf(children, child2);

        if (index1 == -1)
        {
            throw new ArgumentException($"{nameof(child1)} must be a child of {nameof(element)}.", nameof(child1));
        }
        if (index2 == -1)
        {
            throw new ArgumentException($"{nameof(child2)} must be a child of {nameof(element)}.", nameof(child2));
        }

        var start = Math.Min(index1, index2);
        var end = Math.Max(index1, index2);
        return children[start..(end + 1)];
    }

    /// <summary>
    /// Replaces the specified element with new content. The new element must not have parents.
    /// </summary>
    /// <typeparam name="TFrom">The type of the element that is replaced.</typeparam>
    /// <typeparam name="TTo">The type of the new element.</typeparam>
    /// <param name="element">The element to replace.</param>
    /// <param name="newElement">The new element to replace the specified element with.</param>
    /// <returns>A reference to the new element. Its parent is the same as the parent of the removed element.</returns>
    public static TTo ReplaceWith<TFrom, TTo>(this TFrom element, TTo newElement)
        where TFrom : OpenXmlElement
        where TTo : OpenXmlElement
    {
        element.InsertAfterSelf(newElement);
        element.Remove();
        return newElement;
    }
    /// <summary>
    /// Replaces the specified element with new content. The new elements must not have parents.
    /// </summary>
    /// <typeparam name="T">The type of the element that is replaced.</typeparam>
    /// <param name="element">The element to replace.</param>
    /// <param name="newElements">The new elements to replace the specified element with.</param>
    /// <returns>A reference to the removed element (which no longer has a parent).</returns>
    /// <remarks>
    /// Because this overload may insert multiple elements, the returned reference is to the removed element and not to one of the inserted elements.
    /// </remarks>
    public static T ReplaceWith<T>(this T element, params ReadOnlySpan<OpenXmlElement> newElements)
        where T : OpenXmlElement
    {
        for (var i = newElements.Length - 1; i >= 0; i--)
        {
            element.InsertAfterSelf(newElements[i]);
        }
        element.Remove();
        return element;
    }
    /// <summary>
    /// Inserts the specified new elements at the specified index into the child list of the specified element. The order of <paramref name="newElements"/> is preserved.
    /// Resolving the specified <paramref name="index"/> unfortunately requires fully enumerating the child elements of <paramref name="element"/>.
    /// </summary>
    /// <typeparam name="T">The type of the element that is inserted into.</typeparam>
    /// <param name="element">The element to insert into.</param>
    /// <param name="index">The index to insert the first new element at.</param>
    /// <param name="newElements">The new elements to insert into the specified element.</param>
    public static void InsertAt<T>(this T element, Index index, params ReadOnlySpan<OpenXmlElement> newElements)
        where T : OpenXmlCompositeElement
    {
        var i = index.GetOffset(element.ChildElements.Count);
        foreach (var newElement in newElements)
        {
            element.InsertAt(newElement, i++);
        }
    }
    /// <summary>
    /// Creates an array of deep clones of the specified elements.
    /// </summary>
    /// <param name="elements">The elements to clone.</param>
    /// <returns>The created clones.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static OpenXmlElement[] CloneAll(this IEnumerable<OpenXmlElement> elements) => elements.Select(e => e.CloneNode(true)).ToArray();
}
