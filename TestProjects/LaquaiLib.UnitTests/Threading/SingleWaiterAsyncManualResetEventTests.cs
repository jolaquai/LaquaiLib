using LaquaiLib.Threading;

namespace LaquaiLib.UnitTests.Threading;

public class SingleWaiterAsyncManualResetEventTests
{
    [Fact]
    public void InitialUnsignaledIsSetFalse()
    {
        var e = new SingleWaiterAsyncManualResetEvent();
        Assert.False(e.IsSet);
        Assert.False(e.WaitAsync().IsCompleted);
    }

    [Fact]
    public async Task InitialSignaledIsSetTrueAndCompletesImmediately()
    {
        var e = new SingleWaiterAsyncManualResetEvent(true);
        Assert.True(e.IsSet);
        var wait = e.WaitAsync();
        Assert.True(wait.IsCompleted);
        await wait;
    }

    [Fact]
    public async Task SetCompletesPendingWaiter()
    {
        var e = new SingleWaiterAsyncManualResetEvent();
        var wait = e.WaitAsync();
        Assert.False(wait.IsCompleted);

        e.Set();

        await wait.AsTask().WaitAsync(TimeSpan.FromSeconds(1), TestContext.Current.CancellationToken);
        Assert.True(e.IsSet);
    }

    [Fact]
    public async Task WaitAsyncAfterSetCompletesImmediately()
    {
        var e = new SingleWaiterAsyncManualResetEvent();
        e.Set();
        var wait = e.WaitAsync();
        Assert.True(wait.IsCompleted);
        await wait;
    }

    [Fact]
    public void SetWhenAlreadySetHasNoEffect()
    {
        var e = new SingleWaiterAsyncManualResetEvent();
        e.Set();
        e.Set();
        Assert.True(e.IsSet);
        Assert.True(e.WaitAsync().IsCompleted);
    }

    [Fact]
    public void ResetReturnsToUnsignaled()
    {
        var e = new SingleWaiterAsyncManualResetEvent(true);
        e.Reset();
        Assert.False(e.IsSet);
        Assert.False(e.WaitAsync().IsCompleted);
    }

    [Fact]
    public void ResetWhenUnsignaledHasNoEffect()
    {
        var e = new SingleWaiterAsyncManualResetEvent();
        e.Reset();
        Assert.False(e.IsSet);
    }

    [Fact]
    public async Task SetResetCycleProducesFreshWaiter()
    {
        var e = new SingleWaiterAsyncManualResetEvent();
        e.Set();
        await e.WaitAsync();

        e.Reset();
        Assert.False(e.IsSet);
        var second = e.WaitAsync();
        Assert.False(second.IsCompleted);

        e.Set();
        await second.AsTask().WaitAsync(TimeSpan.FromSeconds(1), TestContext.Current.CancellationToken);
        Assert.True(e.IsSet);
    }

    [Fact]
    public async Task ResetInvalidatesOutstandingToken()
    {
        var e = new SingleWaiterAsyncManualResetEvent();
        e.Set();
        var wait = e.WaitAsync();
        e.Reset();

        await Assert.ThrowsAnyAsync<InvalidOperationException>(async () => await wait);
    }
}
