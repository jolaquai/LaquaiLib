namespace LaquaiLib.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="IEnumerable{T}"/> Type.
/// </summary>
public static class IAsyncEnumerableExtensions
{
    /// <summary>
    /// Chains the specified <see cref="IAsyncEnumerable{T}"/> instances into a single <see cref="IAsyncEnumerable{T}"/>.
    /// </summary>
    /// <typeparam name="T">The Type of elements the <see cref="IAsyncEnumerable{T}"/> instances yield.</typeparam>
    /// <param name="toChain">The <see cref="IAsyncEnumerable{T}"/> instances to chain together.</param>
    /// <returns>An <see cref="IAsyncEnumerable{T}"/> implementation that iterates over each <paramref name="toChain"/> in turn.</returns>
    public static IAsyncEnumerable<T> Concat<T>(IAsyncEnumerable<T>[] toChain) => new AsyncEnumerableCombiner<T>(toChain);
    /// <summary>
    /// Chains the specified <see cref="IAsyncEnumerable{T}"/> instances with the specified <paramref name="source"/> into a single <see cref="IAsyncEnumerable{T}"/>.
    /// </summary>
    /// <typeparam name="T">The Type of elements the <see cref="IAsyncEnumerable{T}"/> instances yield.</typeparam>
    /// <param name="source">The <see cref="IAsyncEnumerable{T}"/> to start with.</param>
    /// <param name="with">The <see cref="IAsyncEnumerable{T}"/> instances to chain together.</param>
    /// <returns>An <see cref="IAsyncEnumerable{T}"/> implementation that iterates over each <paramref name="source"/> and <paramref name="with"/> in turn.</returns>
    public static IAsyncEnumerable<T> Concat<T>(this IAsyncEnumerable<T> source, params ReadOnlySpan<IAsyncEnumerable<T>> with) => new AsyncEnumerableCombiner<T>([source, .. with]);
}

// Combines multiple IAsyncEnumerable<T> instances into a single one by just iterating over each one in turn
file readonly struct AsyncEnumerableCombiner<T>(params IAsyncEnumerable<T>[] iterators) : IAsyncEnumerable<T>
{
    private readonly IAsyncEnumerable<T>[] _iterators = iterators;
    public readonly async IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        foreach (var iterator in _iterators)
        {
            await foreach (var item in iterator.WithCancellation(cancellationToken).ConfigureAwait(false))
            {
                yield return item;
            }
        }
    }
}