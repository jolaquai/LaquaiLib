using System.Diagnostics;

namespace LaquaiLib.Collections.Enumeration;

/// <summary>
/// Implements the enumerator pattern to enumerate the segments in a source <see cref="ReadOnlySpan{T}"/> of <typeparamref name="T"/> that are separated by the specified <paramref name="split"/> sequence.
/// </summary>
/// <param name="source">The <see cref="ReadOnlySpan{T}"/> to enumerate the segments of.</param>
/// <param name="split">The <typeparamref name="T"/>s to use as delimiters.</param>
public ref struct SpanSplitBySequenceEnumerable<T>(ReadOnlySpan<T> source, ReadOnlySpan<T> split, IEqualityComparer<T> equalityComparer = null)
{
    private ReadOnlySpan<T> _source = source;
    private readonly ReadOnlySpan<T> _sequence = split;
    private readonly IEqualityComparer<T> _equalityComparer = equalityComparer;

    /// <summary>
    /// Retrieves the current segment at which the enumerator is positioned.
    /// </summary>
    public ReadOnlySpan<T> Current { get; private set; }

    private byte state = 1;
    /// <summary>
    /// Returns the current instance. For use in <see langword="foreach"/> statements.
    /// </summary>
    public readonly SpanSplitBySequenceEnumerable<T> GetEnumerator() => this;
    /// <summary>
    /// Attempts to advance the enumerator to the next segment in the source span.
    /// </summary>
    /// <returns><see langword="true"/> if the advancement has succeeded, otherwise <see langword="false"/> if the enumerator has passed the end of the span.</returns>
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

                var end = _source.IndexOf(_sequence, _equalityComparer);
                if (end == -1)
                {
                    Current = _source;
                    _source = [];
                    return true;
                }
                Current = _source[..end];
                _source = _source[(end + _sequence.Length)..];
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
