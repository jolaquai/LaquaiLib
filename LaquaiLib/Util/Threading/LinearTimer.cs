namespace LaquaiLib.Util.Threading;

/// <summary>
/// Implements a timer that periodically invokes a callback while guaranteeing that no two invocations of that callback overlap.
/// </summary>
public class LinearTimer
{
    /// <summary>
    /// Gets or sets whether the callback should be queued for immediate invocation if the timer ticks while the callback is still running.
    /// If <see langword="false"/>, the callback will only be invoked after the previous invocation has completed and the timer ticks again. Invocations then occur only at multiples of <see cref="Interval"/>. The default is <see langword="false"/>.
    /// </summary>
    public bool QueueCallback { get; set; }
    /// <summary>
    /// Gets or sets the state object that is passed to the callback when it is invoked.
    /// May be freely changed, even while the timer is running, with changes being reflected upon the next invocation of the callback.
    /// </summary>
    public object State { get; set; }

    /// <summary>
    /// Gets or sets the minimal interval between invocations of the callback.
    /// Assigning this will stop the timer.
    /// </summary>
    public TimeSpan Interval
    {
        get;
        set
        {
            field = value;
            timer ??= new System.Threading.Timer(Execute);
            timer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
        }
    }

    /// <summary>
    /// Gets or sets the delay before the first invocation of the callback when the timer is started.
    /// Assigning this will stop the timer.
    /// </summary>
    public TimeSpan WaitOnStart
    {
        get;
        set
        {
            field = value;
            timer ??= new System.Threading.Timer(Execute);
            timer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
        }
    }

    /// <summary>
    /// Gets or sets the callback method to invoke periodically.
    /// Assigning this will stop the timer.
    /// </summary>
    public Action<object> Callback
    {
        get;
        set
        {
            field = value;
            timer ??= new System.Threading.Timer(Execute);
            timer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
        }
    }

    private System.Threading.Timer timer;
    private Task execution;

    /// <summary>
    /// Initializes a new <see cref="LinearTimer"/> with the specified interval and an initial delay equal to that interval.
    /// </summary>
    /// <param name="interval">A <see cref="TimeSpan"/> instance that represents the interval between invocations of the callback.</param>
    public LinearTimer(TimeSpan interval) : this(interval, interval) { }
    /// <summary>
    /// Initializes a new <see cref="LinearTimer"/> with the specified interval and initial delay.
    /// </summary>
    /// <param name="interval">A <see cref="TimeSpan"/> instance that represents the interval between invocations of the callback.</param>
    /// <param name="waitOnStart">A <see cref="TimeSpan"/> instance that represents the delay before the first invocation of the callback.</param>
    public LinearTimer(TimeSpan interval, TimeSpan waitOnStart)
    {
        Interval = interval;
        WaitOnStart = waitOnStart;
    }
    /// <summary>
    /// Initializes a new <see cref="LinearTimer"/> with the specified interval and an initial delay equal to that interval.
    /// </summary>
    /// <param name="milliseconds">The number of milliseconds that represents the interval between invocations of the callback.</param>
    public LinearTimer(long milliseconds) : this(TimeSpan.FromMilliseconds(milliseconds), TimeSpan.FromMilliseconds(milliseconds)) { }
    /// <summary>
    /// Initializes a new <see cref="LinearTimer"/> with the specified interval and initial delay.
    /// </summary>
    /// <param name="milliseconds">The number of milliseconds that represents the interval between invocations of the callback.</param>
    /// <param name="waitOnStart">The number of milliseconds that represents the delay before the first invocation of the callback.</param>
    public LinearTimer(long milliseconds, long waitOnStart) : this(TimeSpan.FromMilliseconds(milliseconds), TimeSpan.FromMilliseconds(waitOnStart)) { }
    /// <summary>
    /// Initializes a new <see cref="LinearTimer"/> with the specified callback method, interval and an initial delay equal to that interval.
    /// </summary>
    /// <param name="callback">The callback method to invoke periodically.</param>
    /// <param name="interval">A <see cref="TimeSpan"/> instance that represents the interval between invocations of the callback.</param>
    public LinearTimer(Action<object> callback, TimeSpan interval) : this(callback, interval, interval) { }
    /// <summary>
    /// Initializes a new <see cref="LinearTimer"/> with the specified interval and initial delay.
    /// </summary>
    /// <param name="callback">The callback method to invoke periodically.</param>
    /// <param name="interval">A <see cref="TimeSpan"/> instance that represents the interval between invocations of the callback.</param>
    /// <param name="waitOnStart">A <see cref="TimeSpan"/> instance that represents the delay before the first invocation of the callback.</param>
    public LinearTimer(Action<object> callback, TimeSpan interval, TimeSpan waitOnStart)
    {
        Callback = callback;
        Interval = interval;
        WaitOnStart = waitOnStart;
    }
    /// <summary>
    /// Initializes a new <see cref="LinearTimer"/> with the specified interval and an initial delay equal to that interval.
    /// </summary>
    /// <param name="callback">The callback method to invoke periodically.</param>
    /// <param name="milliseconds">The number of milliseconds that represents the interval between invocations of the callback.</param>
    public LinearTimer(Action<object> callback, long milliseconds) : this(callback, TimeSpan.FromMilliseconds(milliseconds), TimeSpan.FromMilliseconds(milliseconds)) { }
    /// <summary>
    /// Initializes a new <see cref="LinearTimer"/> with the specified interval and initial delay.
    /// </summary>
    /// <param name="callback">The callback method to invoke periodically.</param>
    /// <param name="milliseconds">The number of milliseconds that represents the interval between invocations of the callback.</param>
    /// <param name="waitOnStart">The number of milliseconds that represents the delay before the first invocation of the callback.</param>
    public LinearTimer(Action<object> callback, long milliseconds, long waitOnStart) : this(callback, TimeSpan.FromMilliseconds(milliseconds), TimeSpan.FromMilliseconds(waitOnStart)) { }

    private int lastAssignedContinuationId = -1;
    /// <summary>
    /// Begins invoking the callback periodically.
    /// </summary>
    public void Start() => timer.Change(WaitOnStart, Interval);
    private void Execute(object state)
    {
        _ = state;

        if (execution?.IsCompleted == false && !QueueCallback)
        {
            return;
        }

        if (execution?.IsCompleted != false)
        {
            execution = Task.Run(() => Callback?.Invoke(State));
        }
        else if (QueueCallback && execution.Id != lastAssignedContinuationId)
        {
            // Don't assign to execution here, otherwise we'll end up with infinite continuations
            // Would defeat the purpose of the timer, wunnit now
            execution.ContinueWith(_ => Execute(null), TaskContinuationOptions.ExecuteSynchronously);
            lastAssignedContinuationId = execution.Id;
        }
    }
    /// <summary>
    /// Stops invoking the callback periodically.
    /// </summary>
    public void Stop() => timer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
}
