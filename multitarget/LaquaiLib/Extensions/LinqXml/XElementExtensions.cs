using System.Xml.Linq;

namespace LaquaiLib.Extensions.LinqXml;

/// <summary>
/// Provides extension methods for the <see cref="XElement"/> Type.
/// </summary>
public static class XElementExtensions
{
    extension(XElement source)
    {
        /// <summary>
        /// Returns a collection of the sibling elements of this node, in document order.
        /// </summary>
        /// <param name="source">The <see cref="XElement"/> to get the siblings of.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="XElement"/> containing the sibling elements of this node, in document order.</returns>
        public IEnumerable<XElement> Siblings() => source.ElementsBeforeSelf().Concat(source.ElementsAfterSelf()).InDocumentOrder();
        /// <summary>
        /// Returns a collection of the sibling elements of this node, in document order. Only elements that have a matching <see cref="XName"/> are included in the collection.
        /// </summary>
        /// <param name="source">The <see cref="XElement"/> to get the siblings of.</param>
        /// <param name="name">The <see cref="XName"/> to match.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="XElement"/> containing the sibling elements of this node, in document order. Only elements that have a matching <see cref="XName"/> are included in the collection.</returns>
        public IEnumerable<XElement> Siblings(XName name) => source.ElementsBeforeSelf(name).Concat(source.ElementsAfterSelf(name)).InDocumentOrder();
    }
}
