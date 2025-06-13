using System.Collections.Concurrent;

using LaquaiLib.Threading;

namespace LaquaiLib.UnitTests.Threading;

public class AsyncTimerTests
{
    [Fact]
    public async Task ConstructorWithIntervalInitializesProperties()
    {
        var interval = TimeSpan.FromMilliseconds(100);
        await using var timer = new AsyncTimer(interval);

        Assert.Equal(interval, timer.Period);
        Assert.Null(timer.State);
    }

    [Fact]
    public async Task ConstructorWithIntervalAndCallbackInitializesProperties()
    {
        var interval = TimeSpan.FromMilliseconds(100);
        static Task callback(object _, CancellationToken ct) => Task.CompletedTask;

        await using var timer = new AsyncTimer(interval, callback);

        Assert.Equal(interval, timer.Period);
        Assert.Null(timer.State);
    }

    [Fact]
    public async Task ConstructorWithIntervalStateAndCallbacksInitializesProperties()
    {
        var interval = TimeSpan.FromMilliseconds(100);
        var state = new object();
        static Task callback1(object _, CancellationToken ct) => Task.CompletedTask;
        static Task callback2(object _, CancellationToken ct) => Task.CompletedTask;

        await using var timer = new AsyncTimer(interval, state, callback1, callback2);

        Assert.Equal(interval, timer.Period);
        Assert.Same(state, timer.State);
    }

    [Fact]
    public async Task PeriodPropertyCanBeSet()
    {
        var initialInterval = TimeSpan.FromMilliseconds(100);
        await using var timer = new AsyncTimer(initialInterval);
        Assert.Equal(initialInterval, timer.Period);

        var newInterval = TimeSpan.FromMilliseconds(200);
        timer.Period = newInterval;
        Assert.Equal(newInterval, timer.Period);
    }

    [Fact]
    public async Task StatePropertyCanBeSet()
    {
        await using var timer = new AsyncTimer(TimeSpan.FromMilliseconds(100));
        Assert.Null(timer.State);

        var state = new object();
        timer.State = state;
        Assert.Same(state, timer.State);

        timer.State = null;
        Assert.Null(timer.State);
    }

    [Fact]
    public async Task CallbackEventCanBeRegisteredAndUnregistered()
    {
        await using var timer = new AsyncTimer(TimeSpan.FromMilliseconds(100));

        var callback1Invoked = false;
        var callback2Invoked = false;

        Task callback1(object _, CancellationToken ct)
        {
            callback1Invoked = true;
            return Task.CompletedTask;
        }

        Task callback2(object _, CancellationToken ct)
        {
            callback2Invoked = true;
            return Task.CompletedTask;
        }

        timer.Callback += callback1;
        timer.Callback += callback2;

        await timer.StartAsync();
        await Task.Delay(200, TestContext.Current.CancellationToken);
        await timer.StopAsync();

        Assert.True(callback1Invoked);
        Assert.True(callback2Invoked);

        callback1Invoked = false;
        callback2Invoked = false;

        timer.Callback -= callback1;

        await timer.StartAsync();
        await Task.Delay(200, TestContext.Current.CancellationToken);
        await timer.StopAsync();

        Assert.False(callback1Invoked);
        Assert.True(callback2Invoked);
    }

    [Fact]
    public async Task CallbackIsInvokedAfterStart()
    {
        var callbackInvoked = new TaskCompletionSource<bool>();

        Task callback(object _, CancellationToken ct)
        {
            callbackInvoked.SetResult(true);
            return Task.CompletedTask;
        }

        await using var timer = new AsyncTimer(TimeSpan.FromMilliseconds(50), callback);
        await timer.StartAsync();

        var result = await Task.WhenAny(callbackInvoked.Task, Task.Delay(500, TestContext.Current.CancellationToken));
        await timer.StopAsync();

        Assert.Same(callbackInvoked.Task, result);
        Assert.True(await callbackInvoked.Task);
    }

    [Fact]
    public async Task CallbackIsNotInvokedAfterStop()
    {
        var callCount = 0;
        var callbackInvoked = new TaskCompletionSource<bool>();

        Task callback(object _, CancellationToken ct)
        {
            if (Interlocked.Increment(ref callCount) == 1)
            {
                callbackInvoked.SetResult(true);
            }
            return Task.CompletedTask;
        }

        await using var timer = new AsyncTimer(TimeSpan.FromMilliseconds(100), callback);
        await timer.StartAsync();

        await callbackInvoked.Task;
        await timer.StopAsync(TestContext.Current.CancellationToken);

        var initialCount = callCount;
        await Task.Delay(300, TestContext.Current.CancellationToken);

        Assert.Equal(initialCount, callCount);
    }

    [Fact]
    public async Task StateIsPassedToCallback()
    {
        var testState = new object();
        object receivedState = null;
        var stateReceived = new TaskCompletionSource<bool>();

        Task callback(object state, CancellationToken cancellationToken)
        {
            receivedState = state;
            stateReceived.SetResult(true);
            return Task.CompletedTask;
        }

        await using var timer = new AsyncTimer(TimeSpan.FromMilliseconds(50));
        timer.State = testState;
        timer.Callback += callback;
        await timer.StartAsync();

        var result = await Task.WhenAny(stateReceived.Task, Task.Delay(500, TestContext.Current.CancellationToken));
        await timer.StopAsync();

        Assert.Same(stateReceived.Task, result);
        Assert.True(await stateReceived.Task);
        Assert.Same(testState, receivedState);
    }

    [Fact]
    public async Task ChangingStateAffectsSubsequentCallbackInvocations()
    {
        var originalState = new object();
        var newState = new object();
        var receivedStates = new ConcurrentQueue<object>();
        var callbackInvokedTwice = new TaskCompletionSource<bool>();
        var callCount = 0;

        Task callback(object state, CancellationToken cancellationToken)
        {
            receivedStates.Enqueue(state);
            if (Interlocked.Increment(ref callCount) >= 2)
            {
                callbackInvokedTwice.SetResult(true);
            }
            return Task.CompletedTask;
        }

        await using var timer = new AsyncTimer(TimeSpan.FromMilliseconds(50));
        timer.State = originalState;
        timer.Callback += callback;
        await timer.StartAsync();

        await Task.Delay(70, TestContext.Current.CancellationToken);
        timer.State = newState;

        var result = await Task.WhenAny(callbackInvokedTwice.Task, Task.Delay(500, TestContext.Current.CancellationToken));
        await timer.StopAsync();

        Assert.Same(callbackInvokedTwice.Task, result);
        Assert.True(await callbackInvokedTwice.Task);
        Assert.Equal(2, receivedStates.Count);
        Assert.Same(originalState, receivedStates.ToArray()[0]);
        Assert.Same(newState, receivedStates.ToArray()[1]);
    }

    [Fact]
    public async Task MultipleCallbacksExecuteInParallel()
    {
        var startTcs = new TaskCompletionSource<bool>();
        var callback1Started = new TaskCompletionSource<bool>();
        var callback2Started = new TaskCompletionSource<bool>();
        var bothCallbacksRunning = new TaskCompletionSource<bool>();

        var callback1Executing = false;
        var callback2Executing = false;

        async Task callback1(object _, CancellationToken ct)
        {
            callback1Executing = true;
            callback1Started.SetResult(true);
            await startTcs.Task;
            callback1Executing = false;
        }

        async Task callback2(object _, CancellationToken ct)
        {
            callback2Executing = true;
            callback2Started.SetResult(true);

            if (callback1Executing)
            {
                bothCallbacksRunning.SetResult(true);
            }

            await startTcs.Task;
            callback2Executing = false;
        }

        await using var timer = new AsyncTimer(TimeSpan.FromMilliseconds(50));
        timer.Callback += callback1;
        timer.Callback += callback2;
        await timer.StartAsync();

        await Task.WhenAll(callback1Started.Task, callback2Started.Task);
        var result = await Task.WhenAny(bothCallbacksRunning.Task, Task.Delay(100, TestContext.Current.CancellationToken));

        startTcs.SetResult(true);
        await timer.StopAsync();

        Assert.Same(bothCallbacksRunning.Task, result);
        Assert.True(await bothCallbacksRunning.Task);
    }

    [Fact]
    public async Task LongRunningCallbacksDoNotOverlap()
    {
        var callTimes = new ConcurrentQueue<DateTime>();
        var callCount = 0;
        var minimumCallsReceived = new TaskCompletionSource<bool>();

        async Task callback(object _, CancellationToken ct)
        {
            callTimes.Enqueue(DateTime.UtcNow);

            if (Interlocked.Increment(ref callCount) >= 3)
            {
                minimumCallsReceived.SetResult(true);
            }

            await Task.Delay(150, TestContext.Current.CancellationToken);
        }

        await using var timer = new AsyncTimer(TimeSpan.FromMilliseconds(50), callback);
        await timer.StartAsync();

        var result = await Task.WhenAny(minimumCallsReceived.Task, Task.Delay(1000, TestContext.Current.CancellationToken));
        await timer.StopAsync();

        Assert.Same(minimumCallsReceived.Task, result);
        Assert.True(await minimumCallsReceived.Task);

        var timeArray = callTimes.ToArray();
        for (var i = 1; i < timeArray.Length; i++)
        {
            var timeDiff = timeArray[i] - timeArray[i - 1];
            Assert.True(timeDiff >= TimeSpan.FromMilliseconds(150 - 20));
        }
    }

    [Fact]
    public async Task ChangingPeriodAffectsSubsequentInvocations()
    {
        var callTimes = new ConcurrentQueue<DateTime>();
        var firstCallReceived = new TaskCompletionSource<bool>();
        var minimumCallsReceived = new TaskCompletionSource<bool>();
        var callCount = 0;

        Task callback(object _, CancellationToken ct)
        {
            callTimes.Enqueue(DateTime.UtcNow);
            var count = Interlocked.Increment(ref callCount);

            if (count == 1)
            {
                firstCallReceived.SetResult(true);
            }
            else if (count >= 3)
            {
                minimumCallsReceived.SetResult(true);
            }

            return Task.CompletedTask;
        }

        var initialPeriod = TimeSpan.FromMilliseconds(100);
        var newPeriod = TimeSpan.FromMilliseconds(50);

        await using var timer = new AsyncTimer(initialPeriod, callback);
        await timer.StartAsync();

        await firstCallReceived.Task;
        timer.Period = newPeriod;

        var result = await Task.WhenAny(minimumCallsReceived.Task, Task.Delay(500, TestContext.Current.CancellationToken));
        await timer.StopAsync();

        Assert.Same(minimumCallsReceived.Task, result);
        Assert.True(await minimumCallsReceived.Task);

        var timeArray = callTimes.ToArray();
        var timeDiff = timeArray[2] - timeArray[1];

        Assert.True(timeArray.Length >= 3);
        Assert.True(timeDiff >= newPeriod.Subtract(TimeSpan.FromMilliseconds(20)));
        Assert.True(timeDiff <= newPeriod.Add(TimeSpan.FromMilliseconds(50)));
    }

    [Fact]
    public async Task StartMethodStartsTimer()
    {
        var callbackInvoked = new TaskCompletionSource<bool>();

        Task callback(object _, CancellationToken ct)
        {
            callbackInvoked.SetResult(true);
            return Task.CompletedTask;
        }

        await using var timer = new AsyncTimer(TimeSpan.FromMilliseconds(50), callback);
        await timer.StartAsync();

        var result = await Task.WhenAny(callbackInvoked.Task, Task.Delay(500, TestContext.Current.CancellationToken));
        await timer.StopAsync();

        Assert.Same(callbackInvoked.Task, result);
        Assert.True(await callbackInvoked.Task);
    }

    [Fact]
    public async Task StopMethodStopsTimer()
    {
        var callCount = 0;
        var callbackInvoked = new TaskCompletionSource<bool>();

        Task callback(object _, CancellationToken ct)
        {
            Interlocked.Increment(ref callCount);
            callbackInvoked.SetResult(true);
            return Task.CompletedTask;
        }

        await using var timer = new AsyncTimer(TimeSpan.FromMilliseconds(500), callback);
        await timer.StartAsync();

        await callbackInvoked.Task;
        await timer.StopAsync();

        var initialCount = callCount;
        await Task.Delay(200, TestContext.Current.CancellationToken);

        Assert.Equal(initialCount, callCount);
    }

    [Fact]
    public async Task MultipleStartCallsDoNotCauseIssues()
    {
        var callbackInvoked = new TaskCompletionSource<bool>();

        Task callback(object _, CancellationToken ct)
        {
            callbackInvoked.SetResult(true);
            return Task.CompletedTask;
        }

        await using var timer = new AsyncTimer(TimeSpan.FromMilliseconds(50), callback);
        await timer.StartAsync();
        await timer.StartAsync();
        await timer.StartAsync();

        var result = Task.WhenAny(callbackInvoked.Task, Task.Delay(200, TestContext.Current.CancellationToken)).Result;
        await timer.StopAsync();

        Assert.Same(callbackInvoked.Task, result);
        Assert.True(callbackInvoked.Task.Result);
    }

    [Fact]
    public async Task MultipleStopCallsDoNotCauseIssues()
    {
        var callbackInvoked = new TaskCompletionSource<bool>();

        Task callback(object _, CancellationToken ct)
        {
            callbackInvoked.SetResult(true);
            return Task.CompletedTask;
        }

        await using var timer = new AsyncTimer(TimeSpan.FromMilliseconds(50), callback);
        await timer.StartAsync();

        await callbackInvoked.Task;

        await timer.StopAsync();
        await timer.StopAsync();
        await timer.StopAsync();

        Assert.True(callbackInvoked.Task.Result);
    }

    [Fact]
    public async Task StaticStartWithIntervalCreatesAndStartsTimer()
    {
        var callbackInvoked = new TaskCompletionSource<bool>();

        await using var timer = AsyncTimer.Start(TimeSpan.FromMilliseconds(50));
        timer.Callback += (_, _) => {
            callbackInvoked.SetResult(true);
            return Task.CompletedTask;
        };

        var result = await Task.WhenAny(callbackInvoked.Task, Task.Delay(200, TestContext.Current.CancellationToken));

        Assert.Same(callbackInvoked.Task, result);
        Assert.True(await callbackInvoked.Task);
    }

    [Fact]
    public async Task StaticStartWithIntervalAndCallbackCreatesAndStartsTimer()
    {
        var callbackInvoked = new TaskCompletionSource<bool>();

        Task callback(object _, CancellationToken ct)
        {
            callbackInvoked.SetResult(true);
            return Task.CompletedTask;
        }

        await using var timer = AsyncTimer.Start(TimeSpan.FromMilliseconds(50), callback);

        var result = await Task.WhenAny(callbackInvoked.Task, Task.Delay(200, TestContext.Current.CancellationToken));

        Assert.Same(callbackInvoked.Task, result);
        Assert.True(await callbackInvoked.Task);
    }

    [Fact]
    public async Task StaticStartWithIntervalStateAndCallbacksCreatesAndStartsTimer()
    {
        var callbackInvoked1 = new TaskCompletionSource<bool>();
        var callbackInvoked2 = new TaskCompletionSource<bool>();
        var state = new object();

        Task callback1(object s, CancellationToken cancellationToken)
        {
            Assert.Same(state, s);
            callbackInvoked1.SetResult(true);
            return Task.CompletedTask;
        }

        Task callback2(object s, CancellationToken cancellationToken)
        {
            Assert.Same(state, s);
            callbackInvoked2.SetResult(true);
            return Task.CompletedTask;
        }

        await using var timer = AsyncTimer.Start(TimeSpan.FromMilliseconds(50), state, callback1, callback2);

        var result1 = await Task.WhenAny(callbackInvoked1.Task, Task.Delay(200, TestContext.Current.CancellationToken));
        var result2 = await Task.WhenAny(callbackInvoked2.Task, Task.Delay(200, TestContext.Current.CancellationToken));

        Assert.Same(callbackInvoked1.Task, result1);
        Assert.Same(callbackInvoked2.Task, result2);
        Assert.True(await callbackInvoked1.Task);
        Assert.True(await callbackInvoked2.Task);
    }

    [Fact]
    public async Task DisposeStopsTimerAndReleasesResources()
    {
        var callCount = 0;
        var callbackInvoked = new TaskCompletionSource<bool>();

        Task callback(object _, CancellationToken ct)
        {
            Interlocked.Increment(ref callCount);
            callbackInvoked.SetResult(true);
            return Task.CompletedTask;
        }

        var timer = new AsyncTimer(TimeSpan.FromMilliseconds(50), callback);
        await timer.StartAsync();

        await callbackInvoked.Task;
        var initialCount = callCount;

        await timer.DisposeAsync();

        await Task.Delay(200, TestContext.Current.CancellationToken);
        Assert.Equal(initialCount, callCount);
    }

    [Fact]
    public async Task StartStopStartWorksCorrectly()
    {
        var callCount = 0;
        var callbackInvoked = new TaskCompletionSource<bool>();
        var secondCallbackInvoked = new TaskCompletionSource<bool>();

        Task callback(object _, CancellationToken ct)
        {
            var count = Interlocked.Increment(ref callCount);
            if (count == 1)
            {
                callbackInvoked.SetResult(true);
            }
            else if (count == 2)
            {
                secondCallbackInvoked.SetResult(true);
            }
            return Task.CompletedTask;
        }

        await using var timer = new AsyncTimer(TimeSpan.FromMilliseconds(50), callback);
        await timer.StartAsync();

        await callbackInvoked.Task;
        await timer.StopAsync();

        var initialCount = callCount;
        await timer.StartAsync();

        var result = await Task.WhenAny(secondCallbackInvoked.Task, Task.Delay(200, TestContext.Current.CancellationToken));
        await timer.StopAsync();

        Assert.Equal(1, initialCount);
        Assert.Same(secondCallbackInvoked.Task, result);
        Assert.True(await secondCallbackInvoked.Task);
    }
}