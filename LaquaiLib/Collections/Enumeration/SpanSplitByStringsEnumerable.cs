using System.Diagnostics;

namespace LaquaiLib.Collections.Enumeration;

/// <summary>
/// Implements the enumerator pattern to enumerate the segments in a source <see cref="ReadOnlySpan{T}"/> of <see langword="char"/>s that are separated by any of the <see langword="string"/>s specified by <paramref name="strings"/>.
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
        _searchValues = new HashSet<string>(StringComparer.FromComparison(_stringComparison));
        for (var i = 0; i < strings.Length; i++)
        {
            _searchValues.Add(strings[i]);
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
        switch (state)
        {
            case 1:
            {
                if (_source.Length == 0)
                {
                    Current = _source;
                    _source = [];
                    state = 2;
                    return true;
                }
                state = 2;
                goto case 2;
            }
            case 2:
            {
                if (_source.Length == 0)
                {
                    return false;
                }

                var end = -1;
                var str = "";
                foreach (var searchValue in _searchValues)
                {
                    end = _source.IndexOf(searchValue, _stringComparison);
                    str = searchValue;
                    if (end != -1)
                    {
                        break;
                    }
                }
                if (end == -1)
                {
                    Current = _source;
                    _source = [];
                    return true;
                }
                Current = _source[..end];
                if (Current.Length == 0 && _stringSplitOptions.HasFlag(StringSplitOptions.RemoveEmptyEntries))
                {
                    return MoveNext();
                }
                if (_stringSplitOptions.HasFlag(StringSplitOptions.TrimEntries))
                {
                    _source = _source[++end..];
                    return true;
                }
                _source = _source[(end + str.Length)..];
                if (_source.Length == 0)
                {
                    state = 3;
                }
                return true;
            }
            case 3:
            {
                Current = [];
                state = 4;
                return true;
            }
            case 4:
                return false;
        }

        Debug.Fail("Invalid state");
        return false;
    }
}