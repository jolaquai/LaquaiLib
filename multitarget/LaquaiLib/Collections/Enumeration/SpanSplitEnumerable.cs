using System.Diagnostics;

namespace LaquaiLib.Collections.Enumeration;

/// <summary>
/// Implements the enumerator pattern to enumerate the segments in a source <see cref="ReadOnlySpan{T}"/> of <typeparamref name="T"/> that are separated by any of the values specified by <paramref name="splits"/>.
/// </summary>
/// <param name="source">The <see cref="ReadOnlySpan{T}"/> to enumerate the segments of.</param>
/// <param name="splits">The <typeparamref name="T"/>s to use as delimiters.</param>
/// <param name="equalityComparer">The <see cref="IEqualityComparer{T}"/> to use for comparing the elements in the source span with the splits.</param>
public ref struct SpanSplitEnumerable<T>(ReadOnlySpan<T> source, ReadOnlySpan<T> splits, IEqualityComparer<T> equalityComparer = null)
{
    private ReadOnlySpan<T> _source = source;
    private readonly ReadOnlySpan<T> _ts = splits;
    private readonly IEqualityComparer<T> _equalityComparer = equalityComparer ?? EqualityComparer<T>.Default;

    /// <summary>
    /// Retrieves the current segment at which the enumerator is positioned.
    /// </summary>
    public ReadOnlySpan<T> Current { get; private set; }

    private byte state = 1;
    /// <summary>
    /// Returns the current instance. For use in <see langword="foreach"/> statements.
    /// </summary>
    public readonly SpanSplitEnumerable<T> GetEnumerator() => this;
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

#if NET9_0
                var end = -1;
                for (var i = 0; i < _source.Length; i++)
                {
                    for (var k = 0; k < _ts.Length; k++)
                    {
                        if (_equalityComparer.Equals(_source[i], _ts[k]))
                        {
                            end = i;
                            break;
                        }
                    }
                    if (end != -1)
                    {
                        break;
                    }
                }
#elif NET10_0_OR_GREATER
                var end = _source.IndexOfAny(_ts, _equalityComparer);
#endif
                if (end == -1)
                {
                    Current = _source;
                    _source = [];
                    return true;
                }
                Current = _source[..end];
                _source = _source[++end..];
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
