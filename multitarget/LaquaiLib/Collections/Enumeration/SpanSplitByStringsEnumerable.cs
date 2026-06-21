using System.Diagnostics;

namespace LaquaiLib.Collections.Enumeration;

/// <summary>
/// Implements the enumerator pattern to enumerate the segments in a source <see cref="ReadOnlySpan{T}"/> of <see langword="char"/>s that are separated by any of the specified <see langword="string"/>s.
/// </summary>
public ref struct SpanSplitByStringsEnumerable
{
    private ReadOnlySpan<char> _source;
    private readonly StringComparison _stringComparison;
    private readonly StringComparer _comparer;
    private readonly StringSplitOptions _stringSplitOptions;
    private readonly HashSet<string> _searchValues;

    /// <param name="source">The <see cref="ReadOnlySpan{T}"/> to enumerate the segments of.</param>
    /// <param name="strings">The <see cref="ReadOnlySpan{T}"/>s to use as delimiters.</param>
    /// <param name="stringComparison">The <see cref="StringComparison"/> behavior to employ when searching for the delimiters. Defaults to <see cref="StringComparison.CurrentCulture"/>.</param>
    public SpanSplitByStringsEnumerable(ReadOnlySpan<char> source, ReadOnlySpan<string> strings, StringComparison stringComparison = StringComparison.CurrentCulture, StringSplitOptions stringSplitOptions = StringSplitOptions.None)
    {
        _source = source;
        _stringComparison = stringComparison;
        _stringSplitOptions = stringSplitOptions;

        _comparer = StringComparer.FromComparison(_stringComparison);
        _searchValues = [with(StringComparer.FromComparison(_stringComparison))];
        for (var i = 0; i < strings.Length; i++)
        {
            _ = _searchValues.Add(strings[i]);
        }
    }

    private byte state = 1;
    /// <summary>
    /// Retrieves the current segment at which the enumerator is positioned.
    /// </summary>
    public ReadOnlySpan<char> Current { get; private set; }

    /// <summary>
    /// Returns the current instance. For use in <see langword="foreach"/> statements.
    /// </summary>
    public readonly SpanSplitByStringsEnumerable GetEnumerator() => this;
    /// <summary>
    /// Attempts to advance the enumerator to the next segment in the source span.
    /// </summary>
    public bool MoveNext()
    {
        var trim = _stringSplitOptions.HasFlag(StringSplitOptions.TrimEntries);
        var removeEmpty = _stringSplitOptions.HasFlag(StringSplitOptions.RemoveEmptyEntries);
        switch (state)
        {
            case 1:
            {
                state = 2;
                if (_source.Length == 0)
                {
                    // An empty source yields a single empty segment, unless empty entries are removed.
                    _source = [];
                    state = 4;
                    if (removeEmpty)
                    {
                        return false;
                    }
                    Current = [];
                    return true;
                }
                goto case 2;
            }
            case 2:
            {
                while (true)
                {
                    if (_source.Length == 0)
                    {
                        return false;
                    }

                    // Find the earliest-occurring delimiter so the result doesn't depend on the
                    // (nondeterministic) iteration order of the delimiter set.
                    var end = -1;
                    var str = "";
                    foreach (var searchValue in _searchValues)
                    {
                        var idx = _source.IndexOf(searchValue, _stringComparison);
                        if (idx != -1 && (end == -1 || idx < end))
                        {
                            end = idx;
                            str = searchValue;
                        }
                    }

                    ReadOnlySpan<char> segment;
                    if (end == -1)
                    {
                        segment = _source;
                        _source = [];
                        state = 4;
                    }
                    else
                    {
                        segment = _source[..end];
                        _source = _source[(end + str.Length)..];
                        // A delimiter at the very end implies a final, empty segment.
                        if (_source.Length == 0)
                        {
                            state = 3;
                        }
                    }

                    if (trim)
                    {
                        segment = segment.Trim();
                    }
                    if (segment.Length == 0 && removeEmpty)
                    {
                        // _source has already been advanced past the delimiter, so this is safe.
                        continue;
                    }
                    Current = segment;
                    return true;
                }
            }
            case 3:
            {
                state = 4;
                if (removeEmpty)
                {
                    return false;
                }
                Current = [];
                return true;
            }
            case 4:
                return false;
        }

        Debug.Fail("Invalid state");
        return false;
    }
}