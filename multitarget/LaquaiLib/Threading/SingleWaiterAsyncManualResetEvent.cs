using System.Threading.Tasks.Sources;

namespace LaquaiLib.Threading;

/// <summary>
/// An allocation-free variant of <see cref="AsyncManualResetEvent"/> backed by <see cref="ManualResetValueTaskSourceCore{TResult}"/>.
/// <para/>
/// Unlike <see cref="AsyncManualResetEvent"/>, this type reuses a single backing source across <see cref="Set"/>/<see cref="Reset"/> cycles instead of
/// allocating a fresh <see cref="TaskCompletionSource"/> and <see cref="Task"/> each time, which eliminates per-cycle allocations on hot paths.
/// <para/>
/// In exchange, it imposes a strict <b>single-waiter</b> contract that the caller is responsible for honoring:
/// <list type="bullet">
/// <item>The <see cref="ValueTask"/> returned by <see cref="WaitAsync"/> must be awaited (or otherwise consumed) <b>exactly once</b>.</item>
/// <item>At most one waiter may be outstanding at a time; registering a second continuation against the same wait throws.</item>
/// <item>A <see cref="WaitAsync"/> result must be consumed before the next <see cref="Reset"/>; a <see cref="Reset"/> invalidates any outstanding token.</item>
/// <item><see cref="Set"/> and <see cref="Reset"/> are not safe to invoke concurrently with each other. The intended pattern is a single
/// controller toggling the event for a single waiter.</item>
/// </list>
/// If you need multiple or repeated waiters, or cannot guarantee single-consumption, use <see cref="AsyncManualResetEvent"/> instead.
/// </summary>
public sealed class SingleWaiterAsyncManualResetEvent : IValueTaskSource
{
    private ManualResetValueTaskSourceCore<bool> _core = new() { RunContinuationsAsynchronously = true };
    private volatile bool _set;

    /// <summary>
    /// Initializes a new <see cref="SingleWaiterAsyncManualResetEvent"/>.
    /// </summary>
    /// <param name="signaled">Specifies whether the event is initially in the signaled state.</param>
    public SingleWaiterAsyncManualResetEvent(bool signaled = false)
    {
        if (signaled)
        {
            _core.SetResult(true);
            _set = true;
        }
    }

    /// <summary>
    /// Gets a <see cref="ValueTask"/> that completes when the event is set.
    /// The returned <see cref="ValueTask"/> must be awaited exactly once and before the next <see cref="Reset"/>; see the type-level remarks.
    /// </summary>
    /// <returns>The <see cref="ValueTask"/> that completes when the event is set.</returns>
    public ValueTask WaitAsync() => new(this, _core.Version);

    /// <summary>
    /// Signals the event.
    /// Calls when the event is already set have no effect.
    /// </summary>
    public void Set()
    {
        if (!_set)
        {
            _set = true;
            _core.SetResult(true);
        }
    }

    /// <summary>
    /// Resets the event to the unsignaled state and invalidates any token previously handed out by <see cref="WaitAsync"/>.
    /// Calls when the event is not signaled have no effect.
    /// </summary>
    public void Reset()
    {
        if (_set)
        {
            _set = false;
            _core.Reset();
        }
    }

    /// <summary>
    /// Gets whether the event is in the signaled state.
    /// </summary>
    public bool IsSet => _set;

    void IValueTaskSource.GetResult(short token) => _core.GetResult(token);
    ValueTaskSourceStatus IValueTaskSource.GetStatus(short token) => _core.GetStatus(token);
    void IValueTaskSource.OnCompleted(Action<object> continuation, object state, short token, ValueTaskSourceOnCompletedFlags flags)
        => _core.OnCompleted(continuation, state, token, flags);
}
