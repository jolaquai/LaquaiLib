using System.Xml.Linq;

namespace LaquaiLib.Extensions.LinqXml;

/// <summary>
/// Provides extension methods for the <see cref="XNode"/> type.
/// </summary>
public static class XNodeExtensions
{
    /// <summary>
    /// Replaces this node with the specified replacement <paramref name="content"/>.
    /// </summary>
    /// <param name="source">The <see cref="XNode"/> to replace.</param>
    /// <param name="content">The content to replace the node with.</param>
    /// <returns>A reference to the removed node (which no longer has a parent).</returns>
    /// <remarks>
    /// Because this overload may insert multiple elements, the returned reference is to the removed element and not to one of the inserted elements.
    /// </remarks>
    public static XNode ReplaceWith(this XNode source, params ReadOnlySpan<object?> content)
    {
        source.AddAfterSelf(content.ToArray());
        source.Remove();
        return source;
    }
}
