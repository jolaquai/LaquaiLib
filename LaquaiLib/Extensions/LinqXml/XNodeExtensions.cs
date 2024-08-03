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
    public static void ReplaceWith(this XNode source, params object?[] content)
    {
        source.AddAfterSelf(content);
        source.Remove();
    }
}
