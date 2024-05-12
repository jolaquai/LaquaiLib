using System.Xml.Linq;

namespace LaquaiLib.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="XNode"/> Type.
/// </summary>
public static class XNodeExtensions
{
    /// <summary>
    /// Returns a collection of the sibling nodes of this node, in document order.
    /// </summary>
    /// <param name="source">The <see cref="XNode"/> to get the siblings of.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="XNode"/> containing the sibling nodes of this node, in document order.</returns>
    public static IEnumerable<XNode> SiblingNodes(this XNode source) => source.NodesBeforeSelf().Concat(source.NodesAfterSelf()).InDocumentOrder();
}
