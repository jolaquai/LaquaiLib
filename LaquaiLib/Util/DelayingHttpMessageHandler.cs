using System.Runtime.CompilerServices;
using System.Threading;

namespace LaquaiLib.Util;

/// <summary>
/// Implements a <see cref="HttpMessageHandler"/> that delays requests by a minimum amount of time. Useful for preventing rate limiting.
/// </summary>
/// <remarks>
/// The delay specified is applied immediately before <i>sending</i> the request, not after receiving the response. As such, the delay will never be exact and the contacted server may disagree. Plan for this according to application load, for example using a higher delay than strictly necessary or when running into 429 or 503 responses.
/// </remarks>
public class DelayingHttpMessageHandler : DelegatingHandler
{
    // I know this is ugly but this is easier than just using TimeSpan and DateTime because of CompareExchange
    private long minDelay;
    /// <summary>
    /// The minimum delay between requests.
    /// </summary>
    public TimeSpan MinimumDelay
    {
        get => TimeSpan.FromTicks(minDelay);
        set => minDelay = value.Ticks;
    }

    private long nextCallAllowed = DateTime.Now.Ticks;
    /// <summary>
    /// Gets a <see cref="DateTime"/> representing the next time a request is allowed.
    /// Note that this should be used purely informatively and not to guard <see cref="Send(HttpRequestMessage, CancellationToken)"/> or <see cref="SendAsync(HttpRequestMessage, CancellationToken)"/> calls, since those employ waiting themselves.
    /// </summary>
    public DateTime NextCallAllowed => new DateTime(nextCallAllowed);

    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

    /// <summary>
    /// Initializes a new <see cref="DelayingHttpMessageHandler"/> with the specified minimum delay between requests that uses a default <see cref="HttpClientHandler"/> to delegate requests to.
    /// </summary>
    /// <param name="minimumDelay">The minimum delay between requests.</param>
    public DelayingHttpMessageHandler(TimeSpan minimumDelay) : this(minimumDelay, new HttpClientHandler())
    {
    }
    /// <summary>
    /// Initializes a new <see cref="DelayingHttpMessageHandler"/> with the specified minimum delay between requests that uses the specified <paramref name="innerHandler"/> to delegate requests to.
    /// </summary>
    /// <param name="minimumDelay">The minimum delay between requests.</param>
    /// <param name="innerHandler">The inner handler to delegate sending requests to.</param>
    public DelayingHttpMessageHandler(TimeSpan minimumDelay, HttpMessageHandler innerHandler) : base(innerHandler)
    {
        minDelay = minimumDelay.Ticks;
    }

    /// <summary>
    /// Sleeps the current thread until the minimum time since the last request has passed, then sends the request.
    /// Note that the <paramref name="cancellationToken"/> is capable of cancelling the request itself, but not the thread sleep. If it is cancelled during the sleep, the request will not be made and the next request is allowed immediately.
    /// </summary>
    /// <param name="request">The request to send.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe for cancellation requests.</param>
    /// <returns>The response to the request.</returns>
    protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        _semaphore.Wait(cancellationToken);
        try
        {
            Wait();
            SetNextAllowedTime(cancellationToken);
        }
        finally
        {
            _semaphore.Release();
        }

        return base.Send(request, cancellationToken);
    }
    /// <summary>
    /// Asynchronously waits until the minimum time since the last request has passed, then sends the request.
    /// </summary>
    /// <param name="request">The request to send.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe for cancellation requests.</param>
    /// <returns>A <see cref="Task{TResult}"/> that resolves to the response to the request.</returns>
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            await WaitAsync(cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            SetNextAllowedTime(cancellationToken);
        }
        finally
        {
            _semaphore.Release();
        }

        return await base.SendAsync(request, cancellationToken);
    }

    /// <summary>
    /// Sleeps the current thread until the minimum time since the last request has passed.
    /// Consumers should never guard <see cref="Send(HttpRequestMessage, CancellationToken)"/> calls using this method, since that method would then reevaluate the wait unnecessarily.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Wait()
    {
        var now = DateTime.Now.Ticks;
        if (now < nextCallAllowed)
        {
            Thread.Sleep(TimeSpan.FromTicks(nextCallAllowed - now));
        }
    }
    /// <summary>
    /// Creates a <see cref="Task"/> that completes when the minimum time since the last request has passed.
    /// Consumers should never guard <see cref="SendAsync(HttpRequestMessage, CancellationToken)"/> calls using this method, since that method would then reevaluate the wait unnecessarily.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe for cancellation requests.</param>
    /// <returns>The created <see cref="Task"/> or <see cref="Task.CompletedTask"/> if the delay has already passed.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task WaitAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.Now.Ticks;
        if (now < nextCallAllowed)
        {
            return Task.Delay(TimeSpan.FromTicks(nextCallAllowed - now), cancellationToken);
        }
        return Task.CompletedTask;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void SetNextAllowedTime(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        long original, newValue;
        do
        {
            original = nextCallAllowed;
            newValue = original + minDelay;
        } while (Interlocked.CompareExchange(ref nextCallAllowed, newValue, original) != original);
    }
}