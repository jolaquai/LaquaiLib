namespace LaquaiLib.Collections.Enumeration;

/// <summary>
/// Implements the enumerator pattern to enumerate the segments in a source <see cref="ReadOnlySpan{T}"/> of <see langword="char"/>s that are separated by any of the <see langword="char"/>s specified by <paramref name="chars"/>.
/// </summary>
/// <param name="source">The <see cref="ReadOnlySpan{T}"/> to enumerate the segments of.</param>
/// <param name="chars">The <see langword="char"/>s to use as delimiters.</param>
public ref struct SpanSplitByCharEnumerator(ReadOnlySpan<char> source, ReadOnlySpan<char> chars)
{
    private ReadOnlySpan<char> _source = source;
    private readonly ReadOnlySpan<char> _chars = chars;

    /// <summary>
    /// Retrieves the current segment at which the enumerator is positioned.
    /// </summary>
    public ReadOnlySpan<char> Current { get; private set; }

    /// <summary>
    /// Returns the current instance. For use in <see langword="foreach"/> statements.
    /// </summary>
    public readonly SpanSplitByCharEnumerator GetEnumerator() => this;
    /// <summary>
    /// Attempts to advance the enumerator to the next segment in the source span.
    /// </summary>
    /// <returns><see langword="true"/> if the advancement has succeeded, otherwise <see langword="false"/> if the enumerator has passed the end of the span.</returns>
    public bool MoveNext()
    {
        if (_source.Length == 0)
        {
            return false;
        }

        var end = _source.IndexOfAny(_chars);
        if (end == -1)
        {
            Current = _source;
            _source = [];
            return true;
        }
        Current = _source[..end];
        _source = _source[++end..];
        return true;
    }
}