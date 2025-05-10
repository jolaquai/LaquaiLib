namespace LaquaiLib.Extensions;

/// <summary>
/// Provides extension methods for <see cref="Task"/>, <see cref="Task{TResult}"/>, <see cref="ValueTask"/> and <see cref="ValueTask{TResult}"/>.
/// </summary>
public static class TaskExtensions
{
    extension(Task task)
    {
        /// <summary>
        /// Creates a <see cref="Task"/> that completes when the specified <paramref name="task"/> completes or when the specified <paramref name="cancellationToken"/> is canceled, but neither event will cause an exception to be thrown.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the wait.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous wait on <paramref name="task"/>.</returns>
        public async Task WaitSafeAsync(CancellationToken cancellationToken = default)
        {
            if (cancellationToken.CanBeCanceled)
            {
                // Need some more work in this case
                var timeout = Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
                _ = await Task.WhenAny(task, timeout).ConfigureAwait(false);
            }
            else
            {
                _ = await Task.WhenAny(task).ConfigureAwait(false);
            }
        }
    }

    extension<TResult>(Task<TResult> task) { }

    extension(ValueTask task) { }

    extension<TResult>(ValueTask<TResult> task) { }
}
