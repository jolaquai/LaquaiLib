namespace LaquaiLib.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="IAsyncEnumerator{T}"/> Type.
/// </summary>
public static class IAsyncEnumeratorExtensions
{
    /// <summary>
    /// Chains the specified <see cref="IAsyncEnumerator{T}"/> instances into a single <see cref="IAsyncEnumerator{T}"/>.
    /// </summary>
    /// <typeparam name="T">The Type of elements the <see cref="IAsyncEnumerator{T}"/> instances yield.</typeparam>
    /// <param name="toChain">The <see cref="IAsyncEnumerator{T}"/> instances to chain together.</param>
    /// <returns>An <see cref="IAsyncEnumerator{T}"/> implementation that iterates over each <paramref name="toChain"/> in turn.</returns>
    public static IAsyncEnumerator<T> Chain<T>(IAsyncEnumerator<T>[] toChain) => new AsyncEnumeratorCombiner<T>(toChain);
    /// <summary>
    /// Chains the specified <see cref="IAsyncEnumerator{T}"/> instances with the specified <paramref name="source"/> into a single <see cref="IAsyncEnumerator{T}"/>.
    /// </summary>
    /// <typeparam name="T">The Type of elements the <see cref="IAsyncEnumerator{T}"/> instances yield.</typeparam>
    /// <param name="source">The <see cref="IAsyncEnumerator{T}"/> to start with.</param>
    /// <param name="with">The <see cref="IAsyncEnumerator{T}"/> instances to chain together.</param>
    /// <returns>An <see cref="IAsyncEnumerator{T}"/> implementation that iterates over each <paramref name="source"/> and <paramref name="with"/> in turn.</returns>
    public static IAsyncEnumerator<T> Chain<T>(this IAsyncEnumerator<T> source, params ReadOnlySpan<IAsyncEnumerator<T>> with)
    {
        if (source is AsyncEnumeratorCombiner<T> combiner)
        {
            combiner.AddIterators(with);
            return combiner;
        }
        return new AsyncEnumeratorCombiner<T>([source, .. with]);
    }
}

file struct AsyncEnumeratorCombiner<T>(params ReadOnlySpan<IAsyncEnumerator<T>> iterators) : IAsyncEnumerator<T>
{
    private IAsyncEnumerator<T>[] _iterators = [.. iterators];
    public T Current { get; private set; }
    public readonly async ValueTask DisposeAsync()
    {
        foreach (var iterator in _iterators)
        {
            await iterator.DisposeAsync().ConfigureAwait(false);
        }
    }
    public async ValueTask<bool> MoveNextAsync()
    {
        foreach (var iterator in _iterators)
        {
            if (await iterator.MoveNextAsync().ConfigureAwait(false))
            {
                Current = iterator.Current;
                return true;
            }
        }
        return false;
    }
    internal void AddIterators(params ReadOnlySpan<IAsyncEnumerator<T>> with) => _iterators = [.. _iterators, .. with];
}
