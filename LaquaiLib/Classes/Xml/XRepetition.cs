namespace System.Xml.Linq;

/// <summary>
/// Represents an <see cref="Linq.XElement"/> that is repeated a certain number of times.
/// </summary>
public class XRepetition : XElement
{
    /// <summary>
    /// The number of times this element is repeated when it is serialized to XML. If left unchanged, this is <c>1</c>.
    /// </summary>
    public int Count { get; set; } = 1;
    /// <summary>
    /// The <see cref="Linq.XElement"/> this <see cref="XRepetition"/> wraps.
    /// </summary>
    public XElement XElement { get; set; }

    #region Constructor BS
    /// <summary>
    /// Instantiates an <see cref="XRepetition"/> from an existing <see cref="Linq.XElement"/> and the number of times it is to be repeated.
    /// </summary>
    /// <param name="other">The <see cref="Linq.XElement"/> this <see cref="XRepetition"/> wraps.</param>
    /// <param name="count">The number of times this element is repeated when it is serialized to XML.</param>
    public XRepetition(XElement other, int count) : base(other)
    {
        Count = count;
        XElement = other;
    }

    /// <summary>
    /// Instantiates an <see cref="XRepetition"/> from an <see cref="XName"/> and the number of times it is to be repeated.
    /// </summary>
    /// <param name="name">An <see cref="XName"/> instance that represents the name of the <see cref="Linq.XElement"/>.</param>
    /// <param name="count">The number of times this element is repeated when it is serialized to XML.</param>
    public XRepetition(XName name, int count) : base(name)
    {
        Count = count;
        XElement = new XElement(name);
    }

    /// <summary>
    /// Instantiates an <see cref="XRepetition"/> from an existing <see cref="XStreamingElement"/> and the number of times it is to be repeated.
    /// </summary>
    /// <param name="other">The <see cref="XStreamingElement"/> this <see cref="XRepetition"/> wraps.</param>
    /// <param name="count">The number of times this element is repeated when it is serialized to XML.</param>
    public XRepetition(XStreamingElement other, int count) : base(other)
    {
        Count = count;
        XElement = new XElement(other);
    }

    /// <summary>
    /// Instantiates an <see cref="XRepetition"/> from an <see cref="XName"/>, a content <see cref="object"/> and the number of times it is to be repeated.
    /// </summary>
    /// <param name="name">An <see cref="XName"/> instance that represents the name of the <see cref="Linq.XElement"/>.</param>
    /// <param name="content">The content of the <see cref="Linq.XElement"/>.</param>
    /// <param name="count">The number of times this element is repeated when it is serialized to XML.</param>
    public XRepetition(XName name, object? content, int count) : base(name, content)
    {
        Count = count;
        XElement = new XElement(name, content);
    }

    /// <summary>
    /// Instantiates an <see cref="XRepetition"/> from an <see cref="XName"/>, an <see cref="Array"/> of content <see cref="object"/>s and the number of times it is to be repeated.
    /// </summary>
    /// <param name="name">An <see cref="XName"/> instance that represents the name of the <see cref="Linq.XElement"/>.</param>
    /// <param name="content">An <see cref="Array"/> of content <see cref="object"/>s of the <see cref="Linq.XElement"/>.</param>
    /// <param name="count">The number of times this element is repeated when it is serialized to XML.</param>
    public XRepetition(XName name, object?[] content, int count) : base(name, content)
    {
        Count = count;
        XElement = new XElement(name, content);
    }

    /// <summary>
    /// Instantiates an <see cref="XRepetition"/> from an <see cref="XName"/> and an <see cref="Array"/> of content <see cref="object"/>s.
    /// </summary>
    /// <param name="name">An <see cref="XName"/> instance that represents the name of the <see cref="Linq.XElement"/>.</param>
    /// <param name="content">An <see cref="Array"/> of content <see cref="object"/>s of the <see cref="Linq.XElement"/>.</param>
    public XRepetition(XName name, params object?[] content) : base(name, content)
    {
        XElement = new XElement(name, content);
    }

    /// <summary>
    /// Instantiates an <see cref="XRepetition"/> from an existing <see cref="Linq.XElement"/>.
    /// </summary>
    /// <param name="other">The <see cref="Linq.XElement"/> this <see cref="XRepetition"/> wraps.</param>
    public XRepetition(XElement other) : base(other)
    {
        XElement = other;
    }

    /// <summary>
    /// Instantiates an <see cref="XRepetition"/> from an <see cref="XName"/>.
    /// </summary>
    /// <param name="name">An <see cref="XName"/> instance that represents the name of the <see cref="Linq.XElement"/>.</param>
    public XRepetition(XName name) : base(name)
    {
        XElement = new XElement(name);
    }

    /// <summary>
    /// Instantiates an <see cref="XRepetition"/> from an <see cref="XName"/> and a content <see cref="object"/>.
    /// </summary>
    /// <param name="name">An <see cref="XName"/> instance that represents the name of the <see cref="Linq.XElement"/>.</param>
    /// <param name="content">The content of the <see cref="Linq.XElement"/>.</param>
    public XRepetition(XName name, object? content) : base(name, content)
    {
        XElement = new XElement(name, content);
    }

    /// <summary>
    /// Instantiates an <see cref="XRepetition"/> from an existing <see cref="XStreamingElement"/>.
    /// </summary>
    /// <param name="other">The <see cref="XStreamingElement"/> this <see cref="XRepetition"/> wraps.</param>
    public XRepetition(XStreamingElement other) : base(other)
    {
        XElement = new XElement(other);
    }
    #endregion
}