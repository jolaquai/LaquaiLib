using System.Buffers;

namespace LaquaiLib.Collections.Enumeration;

/// <summary>
/// Implements the enumerator pattern to enumerate the segments in a source <see cref="ReadOnlySpan{T}"/> of <see langword="char"/>s that are separated by any of the <see langword="string"/>s specified by <paramref name="strings"/>.
/// </summary>
/// <param name="source">The <see cref="ReadOnlySpan{T}"/> to enumerate the segments of.</param>
/// <param name="strings">The <see cref="ReadOnlySpan{T}"/>s to use as delimiters.</param>
/// <param name="stringComparison">The <see cref="StringComparison"/> behavior to employ when searching for the delimiters. Defaults to <see cref="StringComparison.CurrentCulture"/>.</param>
public ref struct SpanSplitByStringEnumerator(ReadOnlySpan<char> source, ReadOnlySpan<string> strings, StringComparison stringComparison = StringComparison.CurrentCulture)
{
    private ReadOnlySpan<char> _source = source;
    private readonly SearchValues<string> _searchValues = SearchValues.Create(strings, stringComparison);

    /// <summary>
    /// Retrieves the current segment at which the enumerator is positioned.
    /// </summary>
    public ReadOnlySpan<char> Current { get; private set; }

    /// <summary>
    /// Returns the current instance. For use in <see langword="foreach"/> statements.
    /// </summary>
    public readonly SpanSplitByStringEnumerator GetEnumerator() => this;
    /// <summary>
    /// Attempts to advance the enumerator to the next segment in the source span.
    /// </summary>
    public bool MoveNext()
    {
        if (_source.Length == 0)
        {
            return false;
        }

        var end = _source.IndexOfAny(_searchValues);
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