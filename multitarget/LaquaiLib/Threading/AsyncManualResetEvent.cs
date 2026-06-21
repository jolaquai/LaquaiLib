namespace LaquaiLib.Threading;

/// <summary>
/// Custom implementation of <see cref="ManualResetEventSlim"/> that supports asynchronous waiting.
/// <para/>
/// Any number of waiters may concurrently await the <see cref="Task"/> returned by <see cref="WaitAsync()"/>, and that <see cref="Task"/> may be awaited any number of times.
/// For a single-waiter, allocation-free alternative, see <see cref="SingleWaiterAsyncManualResetEvent"/>.
/// </summary>
/// <param name="signaled">Specifies whether the event is initially in the signaled state.</param>
public sealed class AsyncManualResetEvent(bool signaled = false)
{
    private volatile TaskCompletionSource _tcs = CreateTcs(signaled);
    private static TaskCompletionSource CreateTcs(bool set)
    {
        var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        if (set)
        {
            tcs.SetResult();
        }
        return tcs;
    }

    /// <summary>
    /// Gets a <see cref="Task"/> that represents the completion of the operation.
    /// </summary>
    /// <returns>The <see cref="Task"/> that completes when the event is set.</returns>
    public Task WaitAsync() => _tcs.Task;

    /// <summary>
    /// Gets a <see cref="Task"/> that completes when the event is set or the specified <paramref name="cancellationToken"/> is cancelled.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> that, when cancelled, causes the returned <see cref="Task"/> to transition to the canceled state.</param>
    /// <returns>The <see cref="Task"/> that completes when the event is set, or that is cancelled when <paramref name="cancellationToken"/> is.</returns>
    public Task WaitAsync(CancellationToken cancellationToken) => _tcs.Task.WaitAsync(cancellationToken);

    /// <summary>
    /// Signals completion of the event.
    /// Calls when the event is set has no effect.
    /// </summary>
    public void Set() => _tcs.TrySetResult();

    /// <summary>
    /// Resets the event to the unsignaled state.
    /// Calls when the event is not signaled has no effect.
    /// </summary>
    public void Reset()
    {
        var currentTcs = _tcs;
        if (currentTcs.Task.IsCompleted)
        {
            Interlocked.CompareExchange(ref _tcs, CreateTcs(false), currentTcs);
        }
    }

    /// <summary>
    /// Gets whether the event is in the signaled state.
    /// </summary>
    public bool IsSet => _tcs.Task.IsCompletedSuccessfully;
}
