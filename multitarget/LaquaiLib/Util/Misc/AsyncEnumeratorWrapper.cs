namespace LaquaiLib.Util.Misc;

/// <summary>
/// Wraps an <see cref="IEnumerator{T}"/> as an <see cref="IAsyncEnumerator{T}"/> to allow asynchronous consumption.
/// Every <see cref="MoveNextAsync"/> call is awaited in a new <see cref="Task" />.
/// May be useful in scenarios when the time between iterations may be long, such as when reading from a network stream or when every enumerator step is expensive.
/// <para/><b>Warning!</b> Do NOT use this <see langword="struct"/> right before an aggregating operation. Instead, use the corresponding aggregation methods from <see cref="Extensions.ALinq.IEnumerableExtensions"/>. This method is intended for use when <c>MoveNext</c> calls on an <see cref="IEnumerator{T}"/> are expected to be computationally expensive or time-consuming; every <c>MoveNext</c> call is wrapped in a new <see cref="Task"/> and <see langword="await"/>ed. To reduce overhead, usage of the asynchronous methods in <see cref="Extensions.ALinq.IEnumerableExtensions"/> is recommended (which batch the entire enumeration and potential allocation of the aggregation result into a single <see cref="Task"/>).
/// </summary>
/// <typeparam name="T">The Type of elements the <see cref="IEnumerator{T}"/> yields.</typeparam>
/// <param name="from">The <see cref="IEnumerator{T}"/> to wrap.</param>
public readonly struct AsyncEnumeratorWrapper<T>(IEnumerator<T> from, CancellationToken cancellationToken = default) : IAsyncEnumerator<T>
{
    /// <summary>
    /// Gets an empty <see cref="AsyncEnumeratorWrapper{T}"/> (that is, an instance that yields no elements).
    /// </summary>
    public static AsyncEnumeratorWrapper<T> Empty { get; } = new AsyncEnumeratorWrapper<T>([]);

    private readonly IEnumerator<T> _from = from;
    private readonly CancellationToken _cancellationToken = cancellationToken;

    /// <summary>
    /// Initializes a new instance of the <see cref="AsyncEnumeratorWrapper{T}"/> <see langword="struct"/> using an <see cref="IEnumerable{T}"/>.
    /// </summary>
    /// <param name="from">The <see cref="IEnumerable{T}"/> to wrap.</param>
    /// <remarks>
    /// <see cref="ParallelQuery{TSource}"/> is supported.
    /// </remarks>
    public AsyncEnumeratorWrapper(IEnumerable<T> from, CancellationToken cancellationToken = default) : this(from.GetEnumerator(), cancellationToken) { }

    /// <inheritdoc/>
    public readonly T Current => _from.Current;
    /// <inheritdoc/>
    public readonly ValueTask DisposeAsync()
    {
        _from.Dispose();
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc/>
    public readonly async ValueTask<bool> MoveNextAsync() => await Task.Run(_from.MoveNext, _cancellationToken).ConfigureAwait(false);
}
