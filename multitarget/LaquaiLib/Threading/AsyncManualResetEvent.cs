namespace LaquaiLib.Threading;

/// <summary>
/// Custom implementation of <see cref="ManualResetEventSlim"/> that supports asynchronous waiting.
/// </summary>
/// <param name="signaled">Specifies whether the event is initially in the signaled state.</param>
public sealed class AsyncManualResetEvent(bool signaled = false)
{
    private volatile TaskCompletionSource<bool> _tcs = CreateTcs(signaled);
    private static TaskCompletionSource<bool> CreateTcs(bool set)
    {
        var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        if (set)
        {
            tcs.SetResult(true);
        }
        return tcs;
    }

    /// <summary>
    /// Gets a <see cref="Task"/> that represents the completion of the operation.
    /// </summary>
    /// <returns>The <see cref="Task"/> that completes when the event is set.</returns>
    public Task WaitAsync() => _tcs.Task;

    /// <summary>
    /// Signals completion of the event.
    /// Calls when the event is set has no effect.
    /// </summary>
    public void Set() => _tcs.TrySetResult(true);

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
    public bool IsSet => _tcs.Task.IsCompleted;
}
