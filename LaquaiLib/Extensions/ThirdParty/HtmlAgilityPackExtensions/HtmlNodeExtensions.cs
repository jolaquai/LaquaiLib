using HtmlAgilityPack;

namespace LaquaiLib.Extensions.ThirdParty.HtmlAgilityPackExtensions;

/// <summary>
/// Provides Extension Methods for the <see cref="HtmlNode"/> Type.
/// </summary>
public static class HtmlNodeExtensions
{
    /// <summary>
    /// Adds child nodes to the <see cref="HtmlNode"/>.
    /// </summary>
    /// <param name="node">The <see cref="HtmlNode"/>to add child nodes to.</param>
    /// <param name="children">The child nodes to add to the <paramref name="node"/>.</param>
    public static void AppendChildren(this HtmlNode node, params HtmlNode[] children)
    {
        foreach (var child in children)
        {
            _ = node.AppendChild(child);
        }
    }
}
