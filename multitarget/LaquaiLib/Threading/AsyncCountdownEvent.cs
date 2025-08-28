namespace LaquaiLib.Threading;

/// <summary>
/// Custom implementation of a <see cref="CountdownEvent"/> that supports asynchronous waiting.
/// </summary>
/// <param name="count">The initial count for the countdown event.</param>
public sealed class AsyncCountdownEvent(int count)
{
    private readonly TaskCompletionSource _tcs = new TaskCompletionSource();
    private volatile int remaining = count;

    /// <summary>
    /// Decreases the countdown by one and signals completion if the countdown reaches zero.
    /// Calls after the countdown has already reached zero will throw an <see cref="InvalidOperationException"/>.
    /// </summary>
    public void Signal()
    {
        if (Interlocked.Decrement(ref remaining) <= 0)
        {
            _tcs.SetResult();
        }
    }
    /// <summary>
    /// Gets a <see cref="Task"/> that completes when the countdown reaches zero.
    /// </summary>
    public Task WaitAsync() => _tcs.Task;
    /// <summary>
    /// Signals the countdown event and waits asynchronously until the countdown reaches zero.
    /// Calls after the countdown has already reached zero will throw an <see cref="InvalidOperationException"/>.
    /// </summary>
    public Task SignalAndWaitAsync()
    {
        Signal();
        return WaitAsync();
    }
}