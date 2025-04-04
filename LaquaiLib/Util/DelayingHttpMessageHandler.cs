﻿using System.Runtime.CompilerServices;

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
    /// Do not use this to guard <see cref="Send(HttpRequestMessage, CancellationToken)"/> or <see cref="SendAsync(HttpRequestMessage, CancellationToken)"/> calls, since those employ waiting themselves.
    /// </summary>
    public DateTime NextCallAllowed => new DateTime(nextCallAllowed);

    // _semaphore is used when cancellation is possible, running is used when it is not since that's probably a bit cheaper
    private readonly SemaphoreSlim _semaphore;
    private int running;
    // Also allocate a static one if consumers request an instance that contributes to the global delayed handler pool
    private static readonly SemaphoreSlim _globalSemaphore = new SemaphoreSlim(1, 1);

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
    public DelayingHttpMessageHandler(TimeSpan minimumDelay, HttpMessageHandler innerHandler) : this(minimumDelay, innerHandler, false)
    {
    }
    /// <summary>
    /// Initializes a new <see cref="DelayingHttpMessageHandler"/> with the specified minimum delay between requests that uses the specified <paramref name="innerHandler"/> to delegate requests to.
    /// </summary>
    /// <param name="minimumDelay">The minimum delay between requests.</param>
    /// <param name="innerHandler">The inner handler to delegate sending requests to.</param>
    /// <param name="asGlobalHandler">Whether to observe a global semaphore for all instances created with this set to <see langword="true"/> or observe a local semaphore for this instance.</param>
    /// <remarks>
    /// This constructor is useful when many <see cref="HttpClient"/>s target the same server and should share the same delay between requests. Setting <paramref name="asGlobalHandler"/> to <see langword="true"/> synchronizes the delay between all instances created this way.
    /// </remarks>
    public DelayingHttpMessageHandler(TimeSpan minimumDelay, HttpMessageHandler innerHandler, bool asGlobalHandler) : base(innerHandler)
    {
        minDelay = minimumDelay.Ticks;
        // Use a global semaphore if requested, otherwise create a new one
        _semaphore = asGlobalHandler ? _globalSemaphore : new SemaphoreSlim(1, 1);
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
        if (cancellationToken.CanBeCanceled)
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
        }
        else
        {
            if (Interlocked.Exchange(ref running, 1) == 1)
            {
                try
                {
                    Wait();
                    SetNextAllowedTime(cancellationToken);
                }
                finally
                {
                    Interlocked.Exchange(ref running, 0);
                }
            }
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
        if (cancellationToken.CanBeCanceled)
        {
            await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                await WaitAsync(cancellationToken).ConfigureAwait(false);
                cancellationToken.ThrowIfCancellationRequested();
                SetNextAllowedTime(cancellationToken);
            }
            finally
            {
                _semaphore.Release();
            }
        }
        else
        {
            if (Interlocked.Exchange(ref running, 1) == 1)
            {
                try
                {
                    await WaitAsync(cancellationToken).ConfigureAwait(false);
                    cancellationToken.ThrowIfCancellationRequested();
                    SetNextAllowedTime(cancellationToken);
                }
                finally
                {
                    Interlocked.Exchange(ref running, 0);
                }
            }
        }

        return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
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
    private void SetNextAllowedTime(CancellationToken cancellationToken = default)
    {
        long original, newValue;
        do
        {
            cancellationToken.ThrowIfCancellationRequested();
            original = nextCallAllowed;
            newValue = original + minDelay;
        } while (Interlocked.CompareExchange(ref nextCallAllowed, newValue, original) != original);
    }
}