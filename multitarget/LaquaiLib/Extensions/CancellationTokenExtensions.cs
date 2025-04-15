namespace LaquaiLib.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="CancellationToken"/> Type.
/// </summary>
public static class CancellationTokenExtensions
{
    /// <summary>
    /// Creates a <see cref="Task"/> that completes successfully (that is, without throwing an exception) when the specified <paramref name="cancellationToken"/> is cancelled.
    /// </summary>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
    /// <returns>The created <see cref="Task"/>.</returns>
    public static Task WhenCancelled(this CancellationToken cancellationToken)
    {
        var tcs = new TaskCompletionSource();
        cancellationToken.Register(tcs.SetResult);
        return tcs.Task;
    }
}
