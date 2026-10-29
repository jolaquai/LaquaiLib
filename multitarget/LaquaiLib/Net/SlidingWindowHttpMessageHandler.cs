namespace LaquaiLib.Util;

/// <summary>
/// Implements a <see cref="DelegatingHandler"/> that enforces a sliding window rate limit on requests.
/// </summary>
/// <remarks>
/// At most <c>maximumRequestCount</c> requests are dispatched within any rolling <c>window</c>-length time span.
/// When the limit is reached, sending is delayed until the oldest request within the current window expires, at which point the new request proceeds.
/// The delay is applied immediately before <i>sending</i> the request, so the contacted server may still enforce its own rate limits. Plan for this by setting window parameters conservatively.
/// </remarks>
/// <param name="innerHandler">The inner handler to delegate sending requests to.</param>
/// <param name="window">The duration of the sliding window.</param>
/// <param name="maximumRequestCount">The maximum number of requests allowed within <paramref name="window"/>.</param>
public sealed class SlidingWindowHttpMessageHandler(HttpMessageHandler innerHandler, TimeSpan window, int maximumRequestCount) : DelegatingHandler(innerHandler)
{
    private readonly Queue<long> _timestamps = new Queue<long>(maximumRequestCount > 0 ? maximumRequestCount : throw new ArgumentOutOfRangeException(nameof(maximumRequestCount), "Maximum request count must be positive."));
    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

    private long _windowTicks = window > TimeSpan.Zero ? window.Ticks : throw new ArgumentOutOfRangeException(nameof(window), "Window duration must be positive.");
    private volatile int _maxRequestCount = maximumRequestCount;
    private volatile bool _isReadOnly;

    /// <summary>
    /// Gets a value indicating whether this instance has been made read-only via <see cref="MakeReadOnly"/>.
    /// </summary>
    public bool IsReadOnly => _isReadOnly;

    /// <summary>
    /// Permanently locks the current <see cref="Window"/> and <see cref="MaximumRequestCount"/> settings for this instance.
    /// Subsequent attempts to set either property throw <see cref="InvalidOperationException"/>.
    /// This operation is irreversible.
    /// </summary>
    public void MakeReadOnly() => _isReadOnly = true;

    /// <summary>
    /// Gets or sets the duration of the sliding window.
    /// </summary>
    /// <remarks>
    /// Changes take effect on the next loop iteration of a waiting caller.
    /// </remarks>
    /// <exception cref="InvalidOperationException">Thrown if <see cref="MakeReadOnly"/> has been called on this instance.</exception>
    public TimeSpan Window
    {
        get => TimeSpan.FromTicks(Interlocked.Read(ref _windowTicks));
        set
        {
            if (_isReadOnly)
                throw new InvalidOperationException($"This {nameof(SlidingWindowHttpMessageHandler)} instance is read-only.");
            Interlocked.Exchange(ref _windowTicks, value > TimeSpan.Zero ? value.Ticks : throw new ArgumentOutOfRangeException(nameof(value), "Window duration must be positive."));
        }
    }
    /// <summary>
    /// Gets or sets the maximum number of requests allowed within <see cref="Window"/>.
    /// </summary>
    /// <remarks>
    /// If lowered below the current number of in-window requests, existing entries are allowed to expire naturally. No requests are drained from the history, so the server-side rate limit invariant is preserved.
    /// Changes take effect on the next loop iteration of a waiting caller.
    /// </remarks>
    /// <exception cref="InvalidOperationException">Thrown if <see cref="MakeReadOnly"/> has been called on this instance.</exception>
    public int MaximumRequestCount
    {
        get => _maxRequestCount;
        set
        {
            if (_isReadOnly)
                throw new InvalidOperationException($"This {nameof(SlidingWindowHttpMessageHandler)} instance is read-only.");
            _maxRequestCount = value > 0 ? value : throw new ArgumentOutOfRangeException(nameof(value), "Maximum request count must be positive.");
        }
    }

    /// <summary>
    /// Initializes a new <see cref="SlidingWindowHttpMessageHandler"/> with the specified window and maximum request count that uses a default <see cref="HttpClientHandler"/> to delegate requests to.
    /// </summary>
    /// <param name="window">The duration of the sliding window.</param>
    /// <param name="maximumRequestCount">The maximum number of requests allowed within <paramref name="window"/>.</param>
    public SlidingWindowHttpMessageHandler(TimeSpan window, int maximumRequestCount)
        : this(new HttpClientHandler(), window, maximumRequestCount)
    {
    }

    /// <summary>
    /// Waits until a request slot is available within the sliding window, then sends the request.
    /// </summary>
    /// <param name="request">The request to send.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe for cancellation requests.</param>
    /// <returns>The response to the request.</returns>
    protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        WaitForSlot(cancellationToken);
        return base.Send(request, cancellationToken);
    }
    /// <summary>
    /// Asynchronously waits until a request slot is available within the sliding window, then sends the request.
    /// </summary>
    /// <param name="request">The request to send.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe for cancellation requests.</param>
    /// <returns>A <see cref="Task{TResult}"/> that resolves to the response to the request.</returns>
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        await WaitForSlotAsync(cancellationToken).ConfigureAwait(false);
        return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
    }

    private void WaitForSlot(CancellationToken cancellationToken)
    {
        while (true)
        {
            _semaphore.Wait(cancellationToken);
            var semaphoreHeld = true;
            try
            {
                var now = DateTime.UtcNow.Ticks;
                var windowTicks = Interlocked.Read(ref _windowTicks);
                PurgeExpired(now, windowTicks);
                if (_timestamps.Count < _maxRequestCount)
                {
                    _timestamps.Enqueue(now);
                    return;
                }
                var waitFor = _timestamps.Peek() + windowTicks - now;
                _ = _semaphore.Release();
                semaphoreHeld = false;
                if (waitFor > 0)
                    if (cancellationToken.WaitHandle.WaitOne(TimeSpan.FromTicks(waitFor)))
                        cancellationToken.ThrowIfCancellationRequested();
            }
            finally
            {
                if (semaphoreHeld)
                    _ = _semaphore.Release();
            }
        }
    }
    private async Task WaitForSlotAsync(CancellationToken cancellationToken)
    {
        while (true)
        {
            await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            var semaphoreHeld = true;
            try
            {
                var now = DateTime.UtcNow.Ticks;
                var windowTicks = Interlocked.Read(ref _windowTicks);
                PurgeExpired(now, windowTicks);
                if (_timestamps.Count < _maxRequestCount)
                {
                    _timestamps.Enqueue(now);
                    return;
                }
                var waitFor = _timestamps.Peek() + windowTicks - now;
                _ = _semaphore.Release();
                semaphoreHeld = false;
                if (waitFor > 0)
                    await Task.Delay(TimeSpan.FromTicks(waitFor), cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                if (semaphoreHeld)
                    _ = _semaphore.Release();
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void PurgeExpired(long now, long windowTicks)
    {
        var cutoff = now - windowTicks;
        while (_timestamps.Count > 0 && _timestamps.Peek() <= cutoff)
            _ = _timestamps.Dequeue();
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
            _semaphore.Dispose();
        base.Dispose(disposing);
    }
}
