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

        using var uncancelled = tokens.Where(t => !t.IsCancellationRequested).GetEnumerator();
        if (!uncancelled.MoveNext())
        {
            return Task.CompletedTask;
        }

        var tcs = new TaskCompletionSource();
        var remaining = 0;
        do
        {
            if (uncancelled.Current.IsCancellationRequested)
            {
                continue;
            }
            remaining++;
            _ = uncancelled.Current.Register(() =>
            {
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    _ = tcs.TrySetResult();
                }
            });
        } while (uncancelled.MoveNext());

        return tcs.Task;
    }
}
