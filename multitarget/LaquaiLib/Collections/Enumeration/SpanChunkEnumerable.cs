using System.Diagnostics;

namespace LaquaiLib.Collections.Enumeration;

/// <summary>
/// Implements the enumerator pattern to enumerate slices of a <see cref="ReadOnlySpan{T}"/> in chunks of a specified size.
/// </summary>
/// <param name="source">The <see cref="ReadOnlySpan{T}"/> to enumerate the segments of.</param>
/// <param name="chunkSize">The maximum size of each chunk to enumerate. If 
public ref struct SpanChunkEnumerable<T>(ReadOnlySpan<T> source, int chunkSize)
{
    private readonly ReadOnlySpan<T> _source = source;
    private readonly int _chunkSize = chunkSize;

    private int chunkIndex = 0;

    /// <summary>
    /// Retrieves the current segment at which the enumerator is positioned.
    /// </summary>
    public ReadOnlySpan<T> Current { get; private set; }

    private byte state = 1;
    /// <summary>
    /// Returns the current instance. For use in <see langword="foreach"/> statements.
    /// </summary>
    public readonly SpanChunkEnumerable<T> GetEnumerator() => this;
    /// <summary>
    /// Attempts to advance the enumerator to the next chunk in the source span.
    /// </summary>
    /// <returns><see langword="true"/> if the advancement has succeeded, otherwise <see langword="false"/> if the enumerator has passed the end of the span.</returns>
    public bool MoveNext()
    {
        switch (state)
        {
            case 0:
                return false;
            case 1:
            {
                if (_source.Length <= _chunkSize)
                {
                    Current = _source;
                    state = 0;
                    return true;
                }
                else
                {
                    state = 2;
                    goto case 2;
                }
            }
            case 2:
            {
                var start = chunkIndex * _chunkSize;
                if (start >= _source.Length)
                {
                    state = 0;
                    return false;
                }
                var end = start + _chunkSize;
                if (end > _source.Length)
                {
                    end = _source.Length;
                }
                Current = _source[start..end];
                chunkIndex++;
                if (end == _source.Length)
                {
                    state = 0;
                }
                return true;
            }
        }

        Debug.Fail("Invalid state");
        return false;
    }
}