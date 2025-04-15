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
    public static IAsyncEnumerable<T> Concat<T>(IAsyncEnumerable<T>[] toChain)
    {
        if (toChain.Any(e => e is null))
        {
            throw new ArgumentNullException(nameof(toChain), "One or more of the provided IAsyncEnumerable<T> instances is null.");
        }
        return new AsyncEnumerableCombiner<T>(toChain);
    }

    /// <summary>
    /// Chains the specified <see cref="IAsyncEnumerable{T}"/> instances with the specified <paramref name="source"/> into a single <see cref="IAsyncEnumerable{T}"/>.
    /// </summary>
    /// <typeparam name="T">The Type of elements the <see cref="IAsyncEnumerable{T}"/> instances yield.</typeparam>
    /// <param name="source">The <see cref="IAsyncEnumerable{T}"/> to start with.</param>
    /// <param name="with">The <see cref="IAsyncEnumerable{T}"/> instances to chain together.</param>
    /// <returns>An <see cref="IAsyncEnumerable{T}"/> implementation that iterates over each <paramref name="source"/> and <paramref name="with"/> in turn.</returns>
    public static IAsyncEnumerable<T> Concat<T>(this IAsyncEnumerable<T> source, params ReadOnlySpan<IAsyncEnumerable<T>> with)
    {
        ArgumentNullException.ThrowIfNull(source);
        for (var i = 0; i < with.Length; i++)
        {
            ArgumentNullException.ThrowIfNull(with[i]);
        }

        return new AsyncEnumerableCombiner<T>([source, .. with]);
    }
}

// Combines multiple IAsyncEnumerable<T> instances into a single one by just iterating over each one in turn
file struct AsyncEnumerableCombiner<T>(params ReadOnlySpan<IAsyncEnumerable<T>> iterators) : IAsyncEnumerable<T>
{
    private IAsyncEnumerable<T>[] _iterators = [.. iterators];
    public readonly async IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        for (var i = 0; i < _iterators.Length; i++)
        {
            var iterator = _iterators[i];
            await foreach (var item in iterator.WithCancellation(cancellationToken).ConfigureAwait(false))
            {
                yield return item;
            }
        }
    }
    internal void AddIterators(params ReadOnlySpan<IAsyncEnumerable<T>> with) => _iterators = [.. _iterators, .. with];
}