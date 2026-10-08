using LaquaiLib.Threading;

namespace LaquaiLib.UnitTests.Threading;

public class AsyncManualResetEventTests
{
    [Fact]
    public void InitialUnsignaledIsSetFalse()
    {
        var e = new AsyncManualResetEvent();
        Assert.False(e.IsSet);
        Assert.False(e.WaitAsync().IsCompleted);
    }

    [Fact]
    public async Task InitialSignaledIsSetTrueAndCompletesImmediately()
    {
        var e = new AsyncManualResetEvent(true);
        Assert.True(e.IsSet);
        Assert.True(e.WaitAsync().IsCompletedSuccessfully);
        await e.WaitAsync();
    }

    [Fact]
    public async Task SetCompletesPendingWaiter()
    {
        var e = new AsyncManualResetEvent();
        var wait = e.WaitAsync();
        Assert.False(wait.IsCompleted);

        e.Set();

        var completed = await Task.WhenAny(wait, Task.Delay(1000, TestContext.Current.CancellationToken));
        Assert.Equal(wait, completed);
        Assert.True(e.IsSet);
    }

    [Fact]
    public async Task WaitAsyncAfterSetCompletesImmediately()
    {
        var e = new AsyncManualResetEvent();
        e.Set();
        Assert.True(e.WaitAsync().IsCompletedSuccessfully);
        await e.WaitAsync();
    }

    [Fact]
    public void SetWhenAlreadySetHasNoEffect()
    {
        var e = new AsyncManualResetEvent();
        e.Set();
        var first = e.WaitAsync();
        e.Set();
        Assert.Same(first, e.WaitAsync());
        Assert.True(e.IsSet);
    }

    [Fact]
    public void ResetReturnsToUnsignaled()
    {
        var e = new AsyncManualResetEvent(true);
        e.Reset();
        Assert.False(e.IsSet);
        Assert.False(e.WaitAsync().IsCompleted);
    }

    [Fact]
    public void ResetWhenUnsignaledHasNoEffect()
    {
        var e = new AsyncManualResetEvent();
        var first = e.WaitAsync();
        e.Reset();
        Assert.Same(first, e.WaitAsync());
        Assert.False(e.IsSet);
    }

    [Fact]
    public async Task SetResetCycleProducesFreshWaiter()
    {
        var e = new AsyncManualResetEvent();
        e.Set();
        await e.WaitAsync();

        e.Reset();
        var second = e.WaitAsync();
        Assert.False(second.IsCompleted);

        e.Set();
        var completed = await Task.WhenAny(second, Task.Delay(1000, TestContext.Current.CancellationToken));
        Assert.Equal(second, completed);
    }

    [Fact]
    public async Task MultipleWaitersAllCompleteOnSet()
    {
        var e = new AsyncManualResetEvent();
        var waiters = new[] { e.WaitAsync(), e.WaitAsync(), e.WaitAsync() };
        Assert.All(waiters, w => Assert.False(w.IsCompleted));

        e.Set();

        var all = Task.WhenAll(waiters);
        var completed = await Task.WhenAny(all, Task.Delay(1000, TestContext.Current.CancellationToken));
        Assert.Equal(all, completed);
    }

    [Fact]
    public async Task WaitAsyncCancellationCancelsPendingWait()
    {
        var e = new AsyncManualResetEvent();
        using var cts = new CancellationTokenSource();
        var wait = e.WaitAsync(cts.Token);

        await cts.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await wait);
        Assert.False(e.IsSet);
    }

    [Fact]
    public async Task WaitAsyncCancellationDoesNotAffectUnderlyingEvent()
    {
        var e = new AsyncManualResetEvent();
        using var cts = new CancellationTokenSource();
        var cancellable = e.WaitAsync(cts.Token);
        var plain = e.WaitAsync();

        await cts.CancelAsync();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await cancellable);

        e.Set();
        var completed = await Task.WhenAny(plain, Task.Delay(1000, TestContext.Current.CancellationToken));
        Assert.Equal(plain, completed);
    }

    [Fact]
    public async Task WaitAsyncWithAlreadyCancelledTokenIgnoresTokenWhenSet()
    {
        var e = new AsyncManualResetEvent(true);
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        await e.WaitAsync(cts.Token);
    }
}
