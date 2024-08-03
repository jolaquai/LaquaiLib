using System.Xml.Linq;

namespace LaquaiLib.Extensions.LinqXml;

/// <summary>
/// Provides extention methods for the <see cref="XAttribute"/> type.
/// </summary>
public static class XAttributeExtensions
{
    /// <summary>
    /// Replaces this <see cref="XAttribute"/> with another one.
    /// </summary>
    /// <param name="attribute">The <see cref="XAttribute"/> to replace.</param>
    /// <param name="other">The <see cref="XAttribute"/> to replace <paramref name="attribute"/> with. If <see langword="null"/>, <paramref name="attribute"/> is removed without replacement.</param>
    public static void ReplaceWith(this XAttribute attribute, XAttribute? other)
    {
        var addTo = attribute.Parent;
        attribute.Remove();
        if (other is not null)
        {
            addTo.Add(other);
        }
    }
}
