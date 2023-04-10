using System.Xml;
using System.Xml.Linq;

using DocumentFormat.OpenXml.Bibliography;

namespace LaquaiLib.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="XmlWriter"/> Type.
/// </summary>
public static class XmlWriterExtensions
{
    /// <summary>
    /// Writes the specified <paramref name="xNode"/> to this <see cref="XmlWriter"/>.
    /// </summary>
    /// <param name="xmlWriter">The <see cref="XmlWriter"/> instance to write <paramref name="xNode"/> to.</param>
    /// <param name="xNode">The <see cref="XNode"/> to serialize and write to <paramref name="xmlWriter"/>.</param>
    public static void WriteXNode(this XmlWriter xmlWriter, XNode xNode)
    {
        if (xNode is XElement element)
        {
            if (element is XRepetition repetition)
            {
                for (var i = 0; i < repetition.Count; i++)
                {
                    Write(xmlWriter, element); 
                }
            }
            else
            {
                Write(xmlWriter, element);
            }
        }
        else
        {
            xmlWriter.WriteRaw(xNode.ToString());
        }

        static void Write(XmlWriter xmlWriter, XElement element)
        {
            xmlWriter.WriteStartElement(element.Name.LocalName);

            // Write attributes
            foreach (var attr in element.Attributes())
            {
                xmlWriter.WriteStartAttribute(attr.Name.LocalName);
                xmlWriter.WriteValue(attr.Value);
                xmlWriter.WriteEndAttribute();
            }

            // Write child elements
            foreach (var child in element.Nodes())
            {
                xmlWriter.WriteXNode(child);
            }

            xmlWriter.WriteEndElement();
        }
    }
}
