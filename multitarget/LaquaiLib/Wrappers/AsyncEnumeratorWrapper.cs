namespace LaquaiLib.Wrappers;

/// <summary>
/// Wraps an <see cref="IEnumerator{T}"/> as an <see cref="IAsyncEnumerator{T}"/> to allow asynchronous consumption.
/// Every <see cref="MoveNextAsync"/> call delegates to <paramref name="from"/>'s <see cref="IEnumerator.MoveNext"/> method, but runs it on a background thread to avoid blocking the calling thread.
/// May be useful in scenarios when the time between iterations may be long, such as when the enumerator does CPU-bound work that shouldn't run on the calling thread.
/// </summary>
/// <typeparam name="T">The Type of elements the <see cref="IEnumerator{T}"/> yields.</typeparam>
/// <param name="from">The <see cref="IEnumerator{T}"/> to wrap.</param>
/// <param name="cancellationToken">The <see cref="CancellationToken"/> to use for cancellation.</param>
public struct AsyncEnumeratorWrapper<T>(IEnumerator<T> from, CancellationToken cancellationToken = default) : IAsyncEnumerator<T>
{
    /// <inheritdoc/>
    public readonly T Current => from.Current;
    /// <inheritdoc/>
    public readonly ValueTask DisposeAsync()
    {
        from.Dispose();
        GC.SuppressFinalize(this);
        return ValueTask.CompletedTask;
    }
    /// <inheritdoc/>
    public readonly async ValueTask<bool> MoveNextAsync()
    {
        cancellationToken.ThrowIfCancellationRequested();
        return await Task.Run(function: from.MoveNext, cancellationToken).ConfigureAwait(false);
    }
}
