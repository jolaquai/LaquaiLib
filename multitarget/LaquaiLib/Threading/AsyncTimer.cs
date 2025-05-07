using LaquaiLib.Extensions;

namespace LaquaiLib.Threading;

/// <summary>
/// Implements a timer that periodically invokes one or more callbacks asynchronously.
/// Callbacks that take longer to return that the configured period will cause ticks to be combined.
/// </summary>
/// <remarks>
/// This implementation uses <see cref="PeriodicTimer"/> for signaling and a <see cref="Task"/> for efficient ticking. It is important the instance be disposed when no longer needed to prevent resource leaks.
/// </remarks>
public class AsyncTimer : IAsyncDisposable
{
    private TaskCompletionSource _tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
    private CancellationTokenSource cts = new CancellationTokenSource();
    private volatile short invoke = 0;
    private readonly PeriodicTimer timer;
    private Task run;

    /// <summary>
    /// Gets or sets the interval between invocations of the callback.
    /// </summary>
    public TimeSpan Period
    {
        get;
        set
        {
            if (field != value)
            {
                field = value;
                if (timer is not null)
                {
                    timer.Period = value;
                }
            }
        }
    }
    /// <summary>
    /// Gets or sets the state object passed to the callback on each invocation.
    /// </summary>
    public object State { get; set; }

    /// <summary>
    /// Allows registering or unregistering a callback that is invoked periodically.
    /// </summary>
    public event Func<object, CancellationToken, Task> Callback;

    /// <summary>
    /// Initializes a new <see cref="AsyncTimer"/> with the specified interval.
    /// </summary>
    /// <param name="interval">The interval between invocations of the callback.</param>
    public AsyncTimer(TimeSpan interval) : this(interval, null, [], false)
    {
    }
    /// <summary>
    /// Initializes a new <see cref="AsyncTimer"/> with the specified interval and state.
    /// </summary>
    /// <param name="interval">The interval between invocations of the callback.</param>
    /// <param name="callback">The state object passed to the callback on each invocation.</param>
    public AsyncTimer(TimeSpan interval, Func<object, CancellationToken, Task> callback) : this(interval, null, [callback], false)
    {
    }
    /// <summary>
    /// Initializes a new <see cref="AsyncTimer"/> with the specified interval and state and registers the specified callbacks for invocation.
    /// </summary>
    /// <param name="interval">The interval between invocations of the callback.</param>
    /// <param name="state">The state object passed to the callback on each invocation.</param>
    /// <param name="callbacks">The callbacks to invoke periodically.</param>
    public AsyncTimer(TimeSpan interval, object state, params ReadOnlySpan<Func<object, CancellationToken, Task>> callbacks) : this(interval, state, callbacks, false)
    {
    }
    private AsyncTimer(TimeSpan interval, object state, ReadOnlySpan<Func<object, CancellationToken, Task>> callbacks, bool started)
    {
        Period = interval;
        State = state;
        for (var i = 0; i < callbacks.Length; i++)
        {
            Callback += callbacks[i];
        }

        if (started)
        {
            _tcs.SetResult();
        }

        timer = new PeriodicTimer(interval);
        new Thread(async () =>
        {
            while (!disposed)
            {
                // While not signaled to run, await that signal to conserve resources
                await _tcs.Task.WaitSafeAsync(cts.Token).ConfigureAwait(false);
                if (!_tcs.Task.IsCompletedSuccessfully || cts.IsCancellationRequested || disposed) continue;

                if (await timer.WaitForNextTickAsync().ConfigureAwait(false))
                {
                    if (!_tcs.Task.IsCompletedSuccessfully || cts.IsCancellationRequested || disposed) continue;
                    run ??= Task.Run(async () =>
                    {
                        var callbacks = Callback.GetTypedInvocationList();
                        var tasks = new Task[callbacks.Length];
                        for (var i = 0; i < callbacks.Length; i++)
                        {
                            tasks[i] = callbacks[i](State, cts.Token);
                        }
                        await Task.WhenAll(tasks).ConfigureAwait(false);
                        run = null;
                    });
                }
            }
        }).Start();
    }

    /// <summary>
    /// Creates and starts a new <see cref="AsyncTimer"/> with the specified interval.
    /// </summary>
    /// <param name="interval">The interval between invocations of the callback.</param>
    public static AsyncTimer Start(TimeSpan interval) => new AsyncTimer(interval, null, [], true);
    /// <summary>
    /// Creates and starts a new <see cref="AsyncTimer"/> with the specified interval and callback.
    /// </summary>
    /// <param name="interval">The interval between invocations of the callback.</param>
    /// <param name="callback">The state object passed to the callback on each invocation.</param>
    public static AsyncTimer Start(TimeSpan interval, Func<object, CancellationToken, Task> callback) => new AsyncTimer(interval, null, [callback], true);
    /// <summary>
    /// Creates and starts a new <see cref="AsyncTimer"/> with the specified interval and state and registers the specified callbacks for invocation.
    /// </summary>
    /// <param name="interval">The interval between invocations of the callback.</param>
    /// <param name="state">The state object passed to the callback on each invocation.</param>
    /// <param name="callbacks">The callbacks to invoke periodically.</param>
    public static AsyncTimer Start(TimeSpan interval, object state, params ReadOnlySpan<Func<object, CancellationToken, Task>> callbacks) => new AsyncTimer(interval, state, callbacks, true);

    private bool disposed;
    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        if (disposed)
        {
            return;
        }

        disposed = true;
        GC.SuppressFinalize(this);

        await StopAsync().ConfigureAwait(false);
        Callback = null;

        cts.Dispose();
        timer.Dispose();
        run?.Dispose();
    }

    /// <summary>
    /// Starts invoking the callback(s) periodically.
    /// </summary>
    public async ValueTask StartAsync()
    {
        _ = (_tcs?.TrySetResult());

        // Reset the cancellation token source
        cts?.Dispose();
        cts = new CancellationTokenSource();
    }
    /// <summary>
    /// Stops invoking the callback(s) periodically and signals cancellation any callbacks that are still running.
    /// The method blocks asynchronously until all callbacks complete or respond to cancellation, or <paramref name="cancellationToken"/> is canceled.
    /// </summary>
    public async ValueTask StopAsync(CancellationToken cancellationToken = default)
    {
        // Cancel current callbacks
        cts?.Cancel();
        // Stop invoking
        _ = (_tcs?.TrySetCanceled(cancellationToken));
        _tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        if (run is not null)
        {
            await run.WaitAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}