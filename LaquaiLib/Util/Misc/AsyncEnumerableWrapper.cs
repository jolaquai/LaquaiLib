namespace LaquaiLib.Util.Misc;

/// <summary>
/// Wraps an <see cref="IEnumerable{T}"/> as an <see cref="IAsyncEnumerable{T}"/> to allow asynchronous consumption.
/// May be useful in scenarios when the time between iterations may be long, such as when reading from a network stream.
/// </summary>
/// <typeparam name="T">The Type of elements the <see cref="IEnumerable{T}"/> yields.</typeparam>
/// <param name="from">The <see cref="IEnumerable{T}"/> to wrap.</param>
/// <remarks>
/// <see cref="ParallelQuery{TSource}"/> is supported.
/// </remarks>
public readonly struct AsyncEnumerableWrapper<T>(IEnumerable<T> from) : IAsyncEnumerable<T>
{
    /// <summary>
    /// Gets an empty <see cref="AsyncEnumerableWrapper{T}"/> (that is, an instance that yields no elements).
    /// </summary>
    public static AsyncEnumerableWrapper<T> Empty { get; } = new AsyncEnumerableWrapper<T>([]);

    private readonly IEnumerable<T> _from = from;
    /// <inheritdoc/>
    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) => new AsyncEnumeratorWrapper<T>(_from.GetEnumerator());
}
