using System.Diagnostics;

namespace LaquaiLib.Extensions;

public static partial class IEnumerableExtensions
{
    extension(IEnumerable<CancellationToken> tokens)
    {
        /// <summary>
        /// Creates a <see cref="Task{TResult}"/> that completes successfully when any of the source sequence's <see cref="CancellationToken"/>s is cancelled.
        /// </summary>
        /// <param name="tokens">The source sequence of <see cref="CancellationToken"/>s.</param>
        /// <returns>The <see cref="Task"/> as described. Its result is the <see cref="CancellationToken"/> that was cancelled first.</returns>
        public Task<CancellationToken> WhenAnyCancelled()
        {
            var tcs = new TaskCompletionSource<CancellationToken>();
            foreach (var token in tokens)
            {
                if (token.IsCancellationRequested)
                {
                    _ = tcs.TrySetResult(token);
                    return tcs.Task;
                }

                _ = token.Register(() => tcs.TrySetResult(token));
            }
            return tcs.Task;
        }
        /// <summary>
        /// Creates a <see cref="Task"/> that completes successfully when all of the source sequence's <see cref="CancellationToken"/>s are cancelled.
        /// </summary>
        /// <param name="tokens">The source sequence of <see cref="CancellationToken"/>s.</param>
        /// <returns>The <see cref="Task"/> as described.</returns>
        public Task WhenAllCancelled()
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
        /// <summary>
        /// Throws an <see cref="OperationCanceledException"/> if any of the source sequence's <see cref="CancellationToken"/>s is cancelled.
        /// </summary>
        /// <param name="tokens"></param>
        [StackTraceHidden]
        public void ThrowIfAnyCancelled()
        {
            foreach (var token in tokens)
            {
                token.ThrowIfCancellationRequested();
            }
        }
        /// <summary>
        /// Throws an <see cref="OperationCanceledException"/> if all of the source sequence's <see cref="CancellationToken"/>s are cancelled.
        /// </summary>
        /// <param name="tokens">The source sequence of <see cref="CancellationToken"/>s.</param>
        /// <exception cref="OperationCanceledException">Thrown when all of the source sequence's <see cref="CancellationToken"/>s are cancelled.</exception>
        [StackTraceHidden]
        public void ThrowIfAllCancelled()
        {
            if (!tokens.Any())
            {
                return;
            }
            var count = 0;
            var cancelled = 0;
            foreach (var token in tokens)
            {
                count++;
                if (token.IsCancellationRequested)
                {
                    cancelled++;
                }
            }
            if (count == cancelled)
            {
                throw new OperationCanceledException();
            }
        }
    }
}
