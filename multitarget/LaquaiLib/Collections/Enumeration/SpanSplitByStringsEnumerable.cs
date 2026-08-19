using System.Buffers;
using System.Diagnostics;

namespace LaquaiLib.Collections.Enumeration;

/// <summary>
/// Implements the enumerator pattern to enumerate the segments in a source <see cref="ReadOnlySpan{T}"/> of <see langword="char"/>s that are separated by any of the specified <see langword="string"/>s.
/// </summary>
public ref struct SpanSplitByStringsEnumerable(ReadOnlySpan<char> source, ReadOnlySpan<string> strings, StringComparison stringComparison = StringComparison.Ordinal, StringSplitOptions stringSplitOptions = StringSplitOptions.None)
{
    private readonly string[] _v = strings.ToArray();
    private readonly SearchValues<string> _sv = SearchValues.Create(strings, stringComparison);
    private readonly bool _trim = stringSplitOptions.HasFlag(StringSplitOptions.TrimEntries);
    private readonly bool _removeEmpty = stringSplitOptions.HasFlag(StringSplitOptions.RemoveEmptyEntries);

    private ReadOnlySpan<char> _source = source;
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
                _v.AsSpan().Sort(static (x, y) => y.Length.CompareTo(x.Length));
                if (_source.Length == 0)
                {
                    // An empty source yields a single empty segment, unless empty entries are removed.
                    state = 4;
                    if (_removeEmpty)
                        return false;
                    Current = [];
                    return true;
                }
                state = 2;
                goto case 2;
            }
            case 2:
            {
                while (true)
                {
                    if (_source.Length == 0)
                        return false;

                    // Find the earliest-occurring delimiter so the result doesn't depend on the
                    // (nondeterministic) iteration order of the delimiter set.
                    var end = -1;
                    var str = "";
                    var idx = _source.IndexOfAny(_sv);
                    if (idx != -1 && (end == -1 || idx < end))
                    {
                        end = idx;
                        // find what's actually there
                        foreach (var sv in _v)
                            if (_source.Slice(idx, sv.Length).Equals(sv, stringComparison))
                                str = sv;
                        Debug.Assert(!string.IsNullOrWhiteSpace(str),
                            "ReadOnlySpan<char>.IndexOfAny(SearchValues<string>) says there's a match here, but a needle-wise Equals with the same StringComparison came up empty");
                    }

                    ReadOnlySpan<char> segment;
                    if (end == -1)
                    {
                        segment = _source;
                        state = 4;
                    }
                    else
                    {
                        segment = _source[..end];
                        _source = _source[(end + str.Length)..];
                        // A delimiter at the very end implies a final, empty segment.
                        if (_source.Length == 0)
                            state = 3;
                    }

                    if (_trim)
                        segment = segment.Trim();
                    if (segment.Length == 0 && _removeEmpty)
                        // source has already been advanced past the delimiter, so this is safe.
                        continue;
                    Current = segment;
                    return true;
                }
            }
            case 3:
            {
                state = 4;
                if (_removeEmpty)
                    return false;
                Current = [];
                return true;
            }
            case 4:
            {
                return false;
            }
            default:
            {
                Debug.Fail("Invalid state");
                return false;
            }
        }
    }
}