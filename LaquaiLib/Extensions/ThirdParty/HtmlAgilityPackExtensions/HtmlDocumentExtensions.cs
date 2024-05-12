using HtmlAgilityPack;

using HtmlDocument = HtmlAgilityPack.HtmlDocument;

namespace LaquaiLib.Extensions.ThirdParty.HtmlAgilityPackExtensions;

/// <summary>
/// Provides Extension Methods for the <see cref="HtmlDocument"/> Type.
/// </summary>
public static class HtmlDocumentExtensions
{
    /// <summary>
    /// Creates an <see cref="HtmlNode"/> using this <see cref="HtmlDocument"/>, the new node's <paramref name="name"/> and a <paramref name="config"/> <see cref="Action{T}"/> that may configure it.
    /// </summary>
    /// <param name="htmlDoc">The <see cref="HtmlDocument"/> to associate with the new node.</param>
    /// <param name="name">The element name of the new node.</param>
    /// <param name="config">An <see cref="Action{T}"/> that may configure the new node.</param>
    /// <returns>A reference to the created node.</returns>
    public static HtmlNode CreateElement(this HtmlDocument htmlDoc, string name, Action<HtmlNode> config)
    {
        var element = htmlDoc.CreateElement(name);
        config(element);
        return element;
    }
}
