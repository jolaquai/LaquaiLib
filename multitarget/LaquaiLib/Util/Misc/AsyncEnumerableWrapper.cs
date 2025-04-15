namespace LaquaiLib.Util.Misc;

/// <summary>
/// Wraps an <see cref="IEnumerable{T}"/> as an <see cref="IAsyncEnumerable{T}"/> to allow asynchronous consumption.
/// Every <c>MoveNextAsync</c> call of the underlying enumerator is awaited in a new <see cref="Task" />.
/// May be useful in scenarios when the time between iterations may be long, such as when reading from a network stream or when every enumerator step is expensive.
/// <para/><b>Warning!</b> Do NOT use this <see langword="struct"/> right before an aggregating operation. Instead, use the corresponding aggregation methods from <see cref="Extensions.ALinq.IEnumerableExtensions"/>. This method is intended for use when <c>MoveNext</c> calls on an <see cref="IEnumerator{T}"/> are expected to be computationally expensive or time-consuming; every <c>MoveNext</c> call is wrapped in a new <see cref="Task"/> and <see langword="await"/>ed. To reduce overhead, usage of the asynchronous methods in <see cref="Extensions.ALinq.IEnumerableExtensions"/> is recommended (which batch the entire enumeration and potential allocation of the aggregation result into a single <see cref="Task"/>).
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
    public readonly IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) => new AsyncEnumeratorWrapper<T>(_from.GetEnumerator(), cancellationToken);
}
