namespace LaquaiLib.Extensions;

/// <summary>
/// Provides extensions for <see cref="Task"/>, <see cref="Task{TResult}"/>, <see cref="ValueTask"/> and <see cref="ValueTask{TResult}"/>.
/// </summary>
public static class TaskExtensions
{
    extension(Task task)
    {
        /// <summary>
        /// Creates a <see cref="Task"/> that completes when the specified <paramref name="task"/> completes or when the specified <paramref name="cancellationToken"/> is canceled, but neither event will fault the returned <see cref="Task"/>.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the wait.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous wait on <paramref name="task"/>.</returns>
        public Task WaitSafeAsync(CancellationToken cancellationToken = default)
        {
            if (task.IsCompleted)
                return Task.CompletedTask;

            return Wait(task, cancellationToken);
        }

        private static async Task Wait(Task t, CancellationToken cancellationToken)
        {
            if (cancellationToken.CanBeCanceled)
            {
                // Need some more work in this case
                var timeout = Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
                await ((Task)Task.WhenAny(t, timeout)).ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);
            }
            else
                await t.ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);
        }
    }

    extension<TResult>(Task<TResult> task) { }

    extension(ValueTask task) { }

    extension<TResult>(ValueTask<TResult> task) { }
}
