namespace LaquaiLib.Threading;

/// <summary>
/// Custom implementation of a <see cref="CountdownEvent"/> that supports asynchronous waiting.
/// </summary>
/// <param name="count">The initial count for the countdown event.</param>
public sealed class AsyncCountdownEvent(int count)
{
    private readonly Lock _lock = new();
    private readonly int _initialCount = count;
    private TaskCompletionSource _tcs = new TaskCompletionSource();
    private int remaining = count;

    /// <summary>
    /// Decreases the countdown by one and signals completion if the countdown reaches zero.
    /// Calls after the countdown has already reached zero will throw an <see cref="InvalidOperationException"/> to prevent misuse.
    /// </summary>
    public void Signal()
    {
        lock (_lock)
        {
            if (remaining <= 0)
                throw new InvalidOperationException("Signal called after countdown reached zero.");
            if (--remaining == 0)
                _tcs.SetResult();
        }
    }
    /// <summary>
    /// Gets a <see cref="Task"/> that completes when the countdown reaches zero.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task WaitAsync() => _tcs.Task;
    /// <summary>
    /// Signals the countdown event and waits asynchronously until the countdown reaches zero.
    /// Calls after the countdown has already reached zero will throw an <see cref="InvalidOperationException"/>.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task SignalAndWaitAsync()
    {
        Signal();
        return WaitAsync();
    }
    /// <summary>
    /// Cancels pendings waits by faulting the underlying <see cref="TaskCompletionSource"/> using an <see cref="OperationCanceledException"/>, then resets the state of the event to its initial count.
    /// </summary>
    public void Reset()
    {
        lock (_lock)
        {
            _tcs.TrySetCanceled();
            _tcs = new TaskCompletionSource();
            remaining = _initialCount;
        }
    }
}