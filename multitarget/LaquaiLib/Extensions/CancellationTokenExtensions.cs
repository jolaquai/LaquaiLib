namespace LaquaiLib.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="CancellationToken"/> Type.
/// </summary>
public static class CancellationTokenExtensions
{
    extension(CancellationToken cancellationToken)
    {
        /// <summary>
        /// Creates a <see cref="Task"/> that completes successfully (that is, without throwing an exception) when the specified <paramref name="cancellationToken"/> is cancelled.
        /// </summary>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
        /// <returns>The created <see cref="Task"/>.</returns>
        public Task WhenCancelled()
        {
            var tcs = new TaskCompletionSource();
            _ = cancellationToken.Register(tcs.SetResult);
            return tcs.Task;
        }
    }
}
