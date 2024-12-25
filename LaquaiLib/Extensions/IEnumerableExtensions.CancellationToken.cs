namespace LaquaiLib.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="IEnumerable{T}"/> of <see cref="CancellationToken"/> Type.
/// </summary>
public static class IEnumerableExtensionsCancellationToken
{
    /// <summary>
    /// Creates a <see cref="Task{TResult}"/> that completes successfully when any of the source sequence's <see cref="CancellationToken"/>s is cancelled.
    /// </summary>
    /// <param name="tokens">The source sequence of <see cref="CancellationToken"/>s.</param>
    /// <returns>The <see cref="Task"/> as described. Its result is the <see cref="CancellationToken"/> that was cancelled first.</returns>
    public static Task<CancellationToken> WhenAnyCancelled(this IEnumerable<CancellationToken> tokens)
    {
        var tcs = new TaskCompletionSource<CancellationToken>();
        foreach (var token in tokens)
        {
            _ = token.Register(() => tcs.TrySetResult(token));
        }
        return tcs.Task;
    }
    /// <summary>
    /// Creates a <see cref="Task"/> that completes successfully when all of the source sequence's <see cref="CancellationToken"/>s are cancelled.
    /// </summary>
    /// <param name="tokens">The source sequence of <see cref="CancellationToken"/>s.</param>
    /// <returns>The <see cref="Task"/> as described.</returns>
    public static Task WhenAllCancelled(this IEnumerable<CancellationToken> tokens)
    {
        if (!tokens.Any())
        {
            return Task.CompletedTask;
        }

        var asList = tokens as IReadOnlyList<CancellationToken> ?? [.. tokens];
        var remaining = asList.Count - asList.Count(t => t.IsCancellationRequested);
        if (remaining == 0)
        {
            return Task.CompletedTask;
        }

        var tcs = new TaskCompletionSource();
        foreach (var token in asList)
        {
            _ = token.Register(() =>
            {
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    _ = tcs.TrySetResult();
                }
            });
        }

        return tcs.Task;
    }
}
