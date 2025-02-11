using System.Buffers;

namespace LaquaiLib.Collections.Enumeration;

/// <summary>
/// Implements the enumerator pattern to enumerate the segments in a source <see cref="ReadOnlySpan{T}"/> of <typeparamref name="T"/> that are separated by any of the values specified by <paramref name="splits"/>. <typeparamref name="T"/> must implement <see cref="IEquatable{T}"/>.
/// </summary>
/// <param name="source">The <see cref="ReadOnlySpan{T}"/> to enumerate the segments of.</param>
/// <param name="splits">The <typeparamref name="T"/>s to use as delimiters.</param>
public ref struct SpanSplitEnumerator<T>(ReadOnlySpan<T> source, ReadOnlySpan<T> splits)
    where T : IEquatable<T>
{
    private ReadOnlySpan<T> _source = source;
    private readonly ReadOnlySpan<T> _ts = splits;

    /// <summary>
    /// Retrieves the current segment at which the enumerator is positioned.
    /// </summary>
    public ReadOnlySpan<T> Current { get; private set; }

    /// <summary>
    /// Returns the current instance. For use in <see langword="foreach"/> statements.
    /// </summary>
    public readonly SpanSplitEnumerator<T> GetEnumerator() => this;
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

        var end = _source.IndexOfAny(_ts);
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

/// <summary>
/// Implements the enumerator pattern to enumerate the segments in a source <see cref="ReadOnlySpan{T}"/> of <typeparamref name="T"/> that are separated by the specified <paramref name="split"/> sequence. <typeparamref name="T"/> must implement <see cref="IEquatable{T}"/>.
/// </summary>
/// <param name="source">The <see cref="ReadOnlySpan{T}"/> to enumerate the segments of.</param>
/// <param name="split">The <typeparamref name="T"/>s to use as delimiters.</param>
public ref struct SpanSplitBySequenceEnumerator<T>(ReadOnlySpan<T> source, ReadOnlySpan<T> split)
    where T : IEquatable<T>
{
    private ReadOnlySpan<T> _source = source;
    private readonly ReadOnlySpan<T> _sequence = split;

    /// <summary>
    /// Retrieves the current segment at which the enumerator is positioned.
    /// </summary>
    public ReadOnlySpan<T> Current { get; private set; }

    /// <summary>
    /// Returns the current instance. For use in <see langword="foreach"/> statements.
    /// </summary>
    public readonly SpanSplitBySequenceEnumerator<T> GetEnumerator() => this;
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

        var end = _source.IndexOf(_sequence);
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

/// <summary>
/// Implements the enumerator pattern to enumerate the segments in a source <see cref="ReadOnlySpan{T}"/> of <typeparamref name="T"/> that are separated by any of the values specified by <paramref name="splits"/>. <typeparamref name="T"/> needn't implement <see cref="IEquatable{T}"/>.
/// </summary>
/// <param name="source">The <see cref="ReadOnlySpan{T}"/> to enumerate the segments of.</param>
/// <param name="splits">The <typeparamref name="T"/>s to use as delimiters.</param>
public ref struct GeneralSpanSplitEnumerator<T>(ReadOnlySpan<T> source, ReadOnlySpan<T> splits)
{
    private ReadOnlySpan<T> _source = source;
    private readonly ReadOnlySpan<T> _splits = splits;

    /// <summary>
    /// Retrieves the current segment at which the enumerator is positioned.
    /// </summary>
    public ReadOnlySpan<T> Current { get; private set; }

    /// <summary>
    /// Returns the current instance. For use in <see langword="foreach"/> statements.
    /// </summary>
    public readonly GeneralSpanSplitEnumerator<T> GetEnumerator() => this;
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

        // Find the ends manually, unfortunately by inspecting each element
        var end = -1;
        for (var i = 0; i < _source.Length; i++)
        {
            for (var j = 0; j < _splits.Length; j++)
            {
                if (_source[i].Equals(_splits[j]))
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

/// <summary>
/// Implements the enumerator pattern to enumerate the segments in a source <see cref="ReadOnlySpan{T}"/> of <typeparamref name="T"/> that are separated by the specified <paramref name="split"/> sequence. <typeparamref name="T"/> needn't implement <see cref="IEquatable{T}"/>.
/// </summary>
/// <param name="source">The <see cref="ReadOnlySpan{T}"/> to enumerate the segments of.</param>
/// <param name="split">The <typeparamref name="T"/>s to use as delimiters.</param>
public ref struct GeneralSpanSplitBySequenceEnumerator<T>(ReadOnlySpan<T> source, ReadOnlySpan<T> split)
{
    private ReadOnlySpan<T> _source = source;
    private readonly ReadOnlySpan<T> _sequence = split;

    /// <summary>
    /// Retrieves the current segment at which the enumerator is positioned.
    /// </summary>
    public ReadOnlySpan<T> Current { get; private set; }

    /// <summary>
    /// Returns the current instance. For use in <see langword="foreach"/> statements.
    /// </summary>
    public readonly GeneralSpanSplitBySequenceEnumerator<T> GetEnumerator() => this;
    /// <summary>
    /// Attempts to advance the enumerator to the next segment in the source span.
    /// </summary>
    /// <returns><see langword="true"/> if the advancement has succeeded, otherwise <see langword="false"/> if the enumerator has passed the end of the span.</returns>
    public bool MoveNext()
    {
        if (_source.Length == 0 || _source.Length < _sequence.Length)
        {
            return false;
        }

        // Find the ends manually, unfortunately by inspecting each element
        var end = -1;
        for (var i = 0; i < _source.Length; i++)
        {
            if (_source.Length - i < _sequence.Length)
            {
                break;
            }
            if (_source.Slice(i, _sequence.Length).SequenceEqual(_sequence))
            {
                end = i;
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
        _source = _source[++end..];
        return true;
    }
}
