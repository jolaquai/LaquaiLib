namespace LaquaiLib.Wrappers;

/// <summary>
/// Wraps an <see cref="IEnumerator{T}"/> as an <see cref="IAsyncEnumerator{T}"/> to allow asynchronous consumption.
/// Every <see cref="MoveNextAsync"/> call is awaited in a new <see cref="Task"/>.
/// May be useful in scenarios when the time between iterations may be long, such as when reading from a network stream or when every enumerator step is expensive.
/// </summary>
/// <typeparam name="T">The Type of elements the <see cref="IEnumerator{T}"/> yields.</typeparam>
/// <param name="from">The <see cref="IEnumerator{T}"/> to wrap.</param>
/// <param name="cancellationToken">The <see cref="CancellationToken"/> to use for cancellation.</param>
public sealed class AsyncEnumeratorWrapper<T>(IEnumerator<T> from, CancellationToken cancellationToken = default) : IAsyncEnumerator<T>
{
    /// <summary>
    /// Gets an empty <see cref="AsyncEnumeratorWrapper{T}"/> (that is, an instance that yields no elements).
    /// </summary>
    public static AsyncEnumeratorWrapper<T> Empty { get; } = new AsyncEnumeratorWrapper<T>([]);

    /// <summary>
    /// Initializes a new <see cref="AsyncEnumeratorWrapper{T}"/> using an <see cref="IEnumerable{T}"/>.
    /// </summary>
    /// <param name="from">The <see cref="IEnumerable{T}"/> to wrap.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to use for cancellation.</param>
    public AsyncEnumeratorWrapper(IEnumerable<T> from, CancellationToken cancellationToken = default) : this(from.GetEnumerator(), cancellationToken) { }

    /// <inheritdoc/>
    public T Current => from.Current;
    /// <inheritdoc/>
    public ValueTask DisposeAsync()
    {
        from.Dispose();
        GC.SuppressFinalize(this);
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc/>
    public async ValueTask<bool> MoveNextAsync() => await Task.Run(from.MoveNext, cancellationToken).ConfigureAwait(false);
}
