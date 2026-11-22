using LaquaiLib.Threading;

namespace LaquaiLib.UnitTests.Threading;

public enum CompletionKind
{
    Result,
    Exception,
    Canceled
}

public abstract class ReusableTaskCompletionSourceTestBase
{
    protected static readonly TimeSpan WaitTimeout = TimeSpan.FromSeconds(10);
    protected static CancellationToken Ct => TestContext.Current.CancellationToken;
    protected static readonly CancellationToken Canceled = new(true);

    public static TheoryData<CompletionKind> Kinds => [CompletionKind.Result, CompletionKind.Exception, CompletionKind.Canceled];

    protected static void AssertState<T>(ReusableTaskCompletionSourceBase<T> tcs, CompletionKind? kind)
    {
        Assert.Equal(kind is not null, tcs.IsCompleted);
        Assert.Equal(kind == CompletionKind.Result, tcs.IsCompletedSuccessfully);
        Assert.Equal(kind == CompletionKind.Exception, tcs.IsFaulted);
        Assert.Equal(kind == CompletionKind.Canceled, tcs.IsCanceled);
    }

    protected static void Complete(ReusableTaskCompletionSource tcs, CompletionKind kind)
    {
        switch (kind)
        {
            case CompletionKind.Result:
                tcs.SetResult();
                break;
            case CompletionKind.Exception:
                tcs.SetException(new FormatException());
                break;
            default:
                tcs.SetCanceled();
                break;
        }
    }

    protected static void Complete<T>(ReusableTaskCompletionSource<T> tcs, CompletionKind kind, T result = default)
    {
        switch (kind)
        {
            case CompletionKind.Result:
                tcs.SetResult(result);
                break;
            case CompletionKind.Exception:
                tcs.SetException(new FormatException());
                break;
            default:
                tcs.SetCanceled();
                break;
        }
    }

    protected static Task CompletedTaskOf(CompletionKind kind) => kind switch
    {
        CompletionKind.Result => Task.CompletedTask,
        CompletionKind.Exception => Task.FromException(new FormatException()),
        _ => Task.FromCanceled(new CancellationToken(true))
    };

    protected static Task<T> CompletedTaskOf<T>(CompletionKind kind, T result = default) => kind switch
    {
        CompletionKind.Result => Task.FromResult(result),
        CompletionKind.Exception => Task.FromException<T>(new FormatException()),
        _ => Task.FromCanceled<T>(new CancellationToken(true))
    };

    protected static long AllocatedBytes(Action action)
    {
        for (var i = 0; i < 1000; i++)
            action();
        var before = GC.GetAllocatedBytesForCurrentThread();
        for (var i = 0; i < 1000; i++)
            action();
        return GC.GetAllocatedBytesForCurrentThread() - before;
    }

    protected static int ContinuationThreadId(Action<Action> register, Action complete)
    {
        using var ran = new ManualResetEventSlim();
        var continuationThread = 0;
        register(() =>
        {
            continuationThread = Environment.CurrentManagedThreadId;
            ran.Set();
        });
        complete();
        Assert.True(ran.Wait(WaitTimeout, Ct));
        return continuationThread;
    }

    protected static async Task Await(ValueTask valueTask) => await valueTask;
    protected static async Task<T> Await<T>(ValueTask<T> valueTask) => await valueTask;
}

public class ReusableTaskCompletionSourceTests : ReusableTaskCompletionSourceTestBase
{
    private static ValueTask Consume(ReusableTaskCompletionSource tcs, bool asTask) => asTask ? new ValueTask(tcs.ValueTask.AsTask()) : tcs.ValueTask;
    private static void CycleThroughValueTask(ReusableTaskCompletionSource tcs)
    {
        tcs.SetResult();
        tcs.ValueTask.GetAwaiter().GetResult();
        tcs.Reset();
    }

    [Fact]
    public void NewInstanceIsPending()
    {
        var tcs = new ReusableTaskCompletionSource();
        AssertState(tcs, null);
        Assert.False(tcs.ValueTask.IsCompleted);
    }

    [Fact]
    public void SetResultCompletesSuccessfully()
    {
        var tcs = new ReusableTaskCompletionSource();
        tcs.SetResult();
        AssertState(tcs, CompletionKind.Result);
        Assert.True(tcs.ValueTask.IsCompletedSuccessfully);
    }

    [Fact]
    public async Task SetResultResumesPendingValueTaskAwaiter()
    {
        var tcs = new ReusableTaskCompletionSource();
        var pending = Await(tcs.ValueTask);
        Assert.False(pending.IsCompleted);
        tcs.SetResult();
        await pending.WaitAsync(WaitTimeout, Ct);
    }

    [Theory, MemberData(nameof(Kinds))]
    public void SetResultAfterCompletionThrows(CompletionKind kind)
    {
        var tcs = new ReusableTaskCompletionSource();
        Complete(tcs, kind);
        Assert.Throws<InvalidOperationException>(tcs.SetResult);
        AssertState(tcs, kind);
    }

    [Theory, MemberData(nameof(Kinds))]
    public void SetExceptionAfterCompletionThrows(CompletionKind kind)
    {
        var tcs = new ReusableTaskCompletionSource();
        Complete(tcs, kind);
        Assert.Throws<InvalidOperationException>(() => tcs.SetException(new InvalidCastException()));
        AssertState(tcs, kind);
    }

    [Theory, MemberData(nameof(Kinds))]
    public void SetCanceledAfterCompletionThrows(CompletionKind kind)
    {
        var tcs = new ReusableTaskCompletionSource();
        Complete(tcs, kind);
        Assert.Throws<InvalidOperationException>(() => tcs.SetCanceled(Canceled));
        AssertState(tcs, kind);
    }

    [Fact]
    public void TrySetResultOnPendingReturnsTrue()
    {
        var tcs = new ReusableTaskCompletionSource();
        Assert.True(tcs.TrySetResult());
        AssertState(tcs, CompletionKind.Result);
    }

    [Theory, MemberData(nameof(Kinds))]
    public void TrySetResultAfterCompletionReturnsFalse(CompletionKind kind)
    {
        var tcs = new ReusableTaskCompletionSource();
        Complete(tcs, kind);
        Assert.False(tcs.TrySetResult());
        AssertState(tcs, kind);
    }

    [Fact]
    public void TrySetExceptionCompletesPending()
    {
        var tcs = new ReusableTaskCompletionSource();
        Assert.True(tcs.TrySetException(new FormatException()));
        AssertState(tcs, CompletionKind.Exception);
    }

    [Theory, MemberData(nameof(Kinds))]
    public void TrySetExceptionAfterCompletionReturnsFalse(CompletionKind kind)
    {
        var tcs = new ReusableTaskCompletionSource();
        Complete(tcs, kind);
        Assert.False(tcs.TrySetException(new InvalidCastException()));
        AssertState(tcs, kind);
    }

    [Fact]
    public void TrySetCanceledCompletesPending()
    {
        var tcs = new ReusableTaskCompletionSource();
        Assert.True(tcs.TrySetCanceled(Canceled));
        AssertState(tcs, CompletionKind.Canceled);
    }

    [Theory, MemberData(nameof(Kinds))]
    public void TrySetCanceledAfterCompletionReturnsFalse(CompletionKind kind)
    {
        var tcs = new ReusableTaskCompletionSource();
        Complete(tcs, kind);
        Assert.False(tcs.TrySetCanceled(Canceled));
        AssertState(tcs, kind);
    }

    [Fact]
    public async Task SetExceptionFaultsValueTaskWithSameException()
    {
        var tcs = new ReusableTaskCompletionSource();
        var ex = new FormatException();
        tcs.SetException(ex);
        AssertState(tcs, CompletionKind.Exception);
        Assert.True(tcs.ValueTask.IsFaulted);
        Assert.Same(ex, await Assert.ThrowsAsync<FormatException>(async () => await tcs.ValueTask));
    }

    [Fact]
    public void SetExceptionWithNullThrowsAndLeavesPending()
    {
        var tcs = new ReusableTaskCompletionSource();
        Assert.Throws<ArgumentNullException>(() => tcs.SetException(null));
        AssertState(tcs, null);
    }

    [Fact]
    public void TrySetExceptionWithNullThrowsAndLeavesPending()
    {
        var tcs = new ReusableTaskCompletionSource();
        Assert.Throws<ArgumentNullException>(() => tcs.TrySetException(null));
        AssertState(tcs, null);
    }

    [Fact]
    public async Task SetCanceledCancelsValueTaskWithToken()
    {
        using var cts = new CancellationTokenSource();
        var tcs = new ReusableTaskCompletionSource();
        tcs.SetCanceled(cts.Token);
        AssertState(tcs, CompletionKind.Canceled);
        Assert.True(tcs.ValueTask.IsCanceled);
        var oce = await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await tcs.ValueTask);
        Assert.Equal(cts.Token, oce.CancellationToken);
    }

    [Fact]
    public async Task SetCanceledWithoutTokenUsesNoneToken()
    {
        var tcs = new ReusableTaskCompletionSource();
        Complete(tcs, CompletionKind.Canceled);
        var oce = await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await tcs.ValueTask);
        Assert.Equal(CancellationToken.None, oce.CancellationToken);
    }

    [Theory, MemberData(nameof(Kinds))]
    public void ResetAfterCompletionReturnsToPending(CompletionKind kind)
    {
        var tcs = new ReusableTaskCompletionSource();
        Complete(tcs, kind);
        tcs.Reset();
        AssertState(tcs, null);
        Assert.False(tcs.ValueTask.IsCompleted);
    }

    [Fact]
    public void ResetWhilePendingThrows()
    {
        var tcs = new ReusableTaskCompletionSource();
        Assert.Throws<InvalidOperationException>(tcs.Reset);
        AssertState(tcs, null);
    }

    [Fact]
    public void TryResetWhilePendingReturnsFalse()
    {
        var tcs = new ReusableTaskCompletionSource();
        Assert.False(tcs.TryReset());
        AssertState(tcs, null);
    }

    [Theory, MemberData(nameof(Kinds))]
    public void TryResetAfterCompletionReturnsTrue(CompletionKind kind)
    {
        var tcs = new ReusableTaskCompletionSource();
        Complete(tcs, kind);
        Assert.True(tcs.TryReset());
        AssertState(tcs, null);
    }

    [Fact]
    public async Task StaleValueTaskThrowsAfterReset()
    {
        var tcs = new ReusableTaskCompletionSource();
        var stale = tcs.ValueTask;
        tcs.SetResult();
        tcs.Reset();
        await Assert.ThrowsAsync<InvalidOperationException>(async () => await stale);
    }

    [Fact]
    public void ValueTaskContinuationRunsAsynchronously()
    {
        var tcs = new ReusableTaskCompletionSource();
        var continuationThread = ContinuationThreadId(c => tcs.ValueTask.ConfigureAwait(false).GetAwaiter().OnCompleted(c), tcs.SetResult);
        Assert.NotEqual(Environment.CurrentManagedThreadId, continuationThread);
    }

    [Theory, InlineData(false), InlineData(true)]
    public async Task PingPongAcrossThreadsCompletesEveryCycle(bool consumeAsTask)
    {
        const int cycles = 2000;
        var ping = new ReusableTaskCompletionSource();
        var pong = new ReusableTaskCompletionSource();
        var echoed = 0;

        async Task Echo()
        {
            for (var i = 0; i < cycles; i++)
            {
                await Consume(ping, consumeAsTask);
                ping.Reset();
                echoed++;
                pong.SetResult();
            }
        }
        async Task Drive()
        {
            for (var i = 0; i < cycles; i++)
            {
                ping.SetResult();
                await Consume(pong, consumeAsTask);
                pong.Reset();
            }
        }

        await Task.WhenAll(Task.Run(Echo, Ct), Task.Run(Drive, Ct)).WaitAsync(WaitTimeout, Ct);
        Assert.Equal(cycles, echoed);
    }

    [Fact]
    public void StateRemainsConsistentAcrossVersionWrapAround()
    {
        var tcs = new ReusableTaskCompletionSource();
        for (var i = 0; i <= ushort.MaxValue + 1; i++)
        {
            Assert.False(tcs.IsCompleted);
            Assert.False(tcs.ValueTask.IsCompleted);
            tcs.SetResult();
            Assert.True(tcs.ValueTask.IsCompletedSuccessfully);
            tcs.Reset();
        }
    }

    [Fact]
    public void ValueTaskCycleDoesNotAllocate()
    {
        var tcs = new ReusableTaskCompletionSource();
        Assert.Equal(0, AllocatedBytes(() => CycleThroughValueTask(tcs)));
    }

    [Fact]
    public void SetFromTaskCopiesSuccess()
    {
        var tcs = new ReusableTaskCompletionSource();
        tcs.SetFromTask(Task.CompletedTask);
        AssertState(tcs, CompletionKind.Result);
    }

    [Fact]
    public async Task SetFromTaskCopiesOriginalException()
    {
        var tcs = new ReusableTaskCompletionSource();
        var ex = new FormatException();
        tcs.SetFromTask(Task.FromException(ex));
        AssertState(tcs, CompletionKind.Exception);
        Assert.Same(ex, await Assert.ThrowsAsync<FormatException>(async () => await tcs.ValueTask));
    }

    [Fact]
    public async Task SetFromTaskCopiesCancellationToken()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        var tcs = new ReusableTaskCompletionSource();
        tcs.SetFromTask(Task.FromCanceled(cts.Token));
        AssertState(tcs, CompletionKind.Canceled);
        var oce = await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await tcs.ValueTask);
        Assert.Equal(cts.Token, oce.CancellationToken);
    }

    [Fact]
    public void SetFromTaskWithPendingTaskThrowsAndLeavesPending()
    {
        var tcs = new ReusableTaskCompletionSource();
        Assert.Throws<InvalidOperationException>(() => tcs.SetFromTask(new TaskCompletionSource().Task));
        AssertState(tcs, null);
    }

    [Fact]
    public void TrySetFromTaskWithPendingTaskThrowsAndLeavesPending()
    {
        var tcs = new ReusableTaskCompletionSource();
        Assert.Throws<InvalidOperationException>(() => tcs.TrySetFromTask(new TaskCompletionSource().Task));
        AssertState(tcs, null);
    }

    [Theory, MemberData(nameof(Kinds))]
    public void TrySetFromTaskOnPendingReturnsTrueAndCopiesState(CompletionKind kind)
    {
        var tcs = new ReusableTaskCompletionSource();
        Assert.True(tcs.TrySetFromTask(CompletedTaskOf(kind)));
        AssertState(tcs, kind);
    }

    [Theory, MemberData(nameof(Kinds))]
    public void SetFromTaskAfterCompletionThrowsAndLeavesStateUnchanged(CompletionKind kind)
    {
        var tcs = new ReusableTaskCompletionSource();
        tcs.SetResult();
        Assert.Throws<InvalidOperationException>(() => tcs.SetFromTask(CompletedTaskOf(kind)));
        AssertState(tcs, CompletionKind.Result);
    }

    [Theory, MemberData(nameof(Kinds))]
    public void TrySetFromTaskAfterCompletionReturnsFalseAndLeavesStateUnchanged(CompletionKind kind)
    {
        var tcs = new ReusableTaskCompletionSource();
        tcs.SetResult();
        Assert.False(tcs.TrySetFromTask(CompletedTaskOf(kind)));
        AssertState(tcs, CompletionKind.Result);
    }
}

public class ReusableTaskCompletionSourceOfTTests : ReusableTaskCompletionSourceTestBase
{
    private static ValueTask<T> Consume<T>(ReusableTaskCompletionSource<T> tcs, bool asTask) => asTask ? new ValueTask<T>(tcs.ValueTask.AsTask()) : tcs.ValueTask;
    private static T ResultOf<T>(ReusableTaskCompletionSource<T> tcs) => tcs.ValueTask.GetAwaiter().GetResult();
    private static void CycleThroughValueTask(ReusableTaskCompletionSource<int> tcs, int result)
    {
        tcs.SetResult(result);
        tcs.ValueTask.GetAwaiter().GetResult();
        tcs.Reset();
    }

    [Fact]
    public void NewInstanceIsPending()
    {
        var tcs = new ReusableTaskCompletionSource<int>();
        AssertState(tcs, null);
        Assert.False(tcs.ValueTask.IsCompleted);
    }

    [Fact]
    public async Task SetResultIsObservableThroughValueTask()
    {
        var tcs = new ReusableTaskCompletionSource<int>();
        tcs.SetResult(42);
        AssertState(tcs, CompletionKind.Result);
        Assert.True(tcs.ValueTask.IsCompletedSuccessfully);
        Assert.Equal(42, await tcs.ValueTask);
    }

    [Fact]
    public async Task SetResultWithNullReferenceIsObservable()
    {
        var tcs = new ReusableTaskCompletionSource<string>();
        tcs.SetResult(null);
        AssertState(tcs, CompletionKind.Result);
        Assert.Null(await tcs.ValueTask);
    }

    [Fact]
    public async Task SetResultResumesPendingValueTaskAwaiterWithResult()
    {
        var tcs = new ReusableTaskCompletionSource<int>();
        var pending = Await(tcs.ValueTask);
        Assert.False(pending.IsCompleted);
        tcs.SetResult(7);
        Assert.Equal(7, await pending.WaitAsync(WaitTimeout, Ct));
    }

    [Fact]
    public async Task ValueTaskAsTaskObtainedWhilePendingReceivesResult()
    {
        var tcs = new ReusableTaskCompletionSource<int>();
        var task = tcs.ValueTask.AsTask();
        Assert.False(task.IsCompleted);
        tcs.SetResult(7);
        Assert.Equal(7, await task.WaitAsync(WaitTimeout, Ct));
    }

    [Theory, MemberData(nameof(Kinds))]
    public void SetResultAfterCompletionThrowsAndKeepsState(CompletionKind kind)
    {
        var tcs = new ReusableTaskCompletionSource<int>();
        Complete(tcs, kind, 1);
        Assert.Throws<InvalidOperationException>(() => tcs.SetResult(2));
        AssertState(tcs, kind);
        if (kind == CompletionKind.Result)
            Assert.Equal(1, ResultOf(tcs));
    }

    [Theory, MemberData(nameof(Kinds))]
    public void SetExceptionAfterCompletionThrows(CompletionKind kind)
    {
        var tcs = new ReusableTaskCompletionSource<int>();
        Complete(tcs, kind, 1);
        Assert.Throws<InvalidOperationException>(() => tcs.SetException(new InvalidCastException()));
        AssertState(tcs, kind);
    }

    [Theory, MemberData(nameof(Kinds))]
    public void SetCanceledAfterCompletionThrows(CompletionKind kind)
    {
        var tcs = new ReusableTaskCompletionSource<int>();
        Complete(tcs, kind, 1);
        Assert.Throws<InvalidOperationException>(() => tcs.SetCanceled(Canceled));
        AssertState(tcs, kind);
    }

    [Fact]
    public void TrySetResultOnPendingReturnsTrue()
    {
        var tcs = new ReusableTaskCompletionSource<int>();
        Assert.True(tcs.TrySetResult(7));
        AssertState(tcs, CompletionKind.Result);
        Assert.Equal(7, ResultOf(tcs));
    }

    [Theory, MemberData(nameof(Kinds))]
    public void TrySetResultAfterCompletionReturnsFalseAndKeepsState(CompletionKind kind)
    {
        var tcs = new ReusableTaskCompletionSource<int>();
        Complete(tcs, kind, 1);
        Assert.False(tcs.TrySetResult(2));
        AssertState(tcs, kind);
        if (kind == CompletionKind.Result)
            Assert.Equal(1, ResultOf(tcs));
    }

    [Theory, MemberData(nameof(Kinds))]
    public void TrySetExceptionAfterCompletionReturnsFalse(CompletionKind kind)
    {
        var tcs = new ReusableTaskCompletionSource<int>();
        Complete(tcs, kind, 1);
        Assert.False(tcs.TrySetException(new InvalidCastException()));
        AssertState(tcs, kind);
    }

    [Theory, MemberData(nameof(Kinds))]
    public void TrySetCanceledAfterCompletionReturnsFalse(CompletionKind kind)
    {
        var tcs = new ReusableTaskCompletionSource<int>();
        Complete(tcs, kind, 1);
        Assert.False(tcs.TrySetCanceled(Canceled));
        AssertState(tcs, kind);
    }

    [Fact]
    public async Task SetExceptionFaultsValueTaskWithSameException()
    {
        var tcs = new ReusableTaskCompletionSource<int>();
        var ex = new FormatException();
        tcs.SetException(ex);
        AssertState(tcs, CompletionKind.Exception);
        Assert.Same(ex, await Assert.ThrowsAsync<FormatException>(async () => await tcs.ValueTask));
    }

    [Fact]
    public async Task SetCanceledCancelsValueTaskWithToken()
    {
        using var cts = new CancellationTokenSource();
        var tcs = new ReusableTaskCompletionSource<int>();
        tcs.SetCanceled(cts.Token);
        AssertState(tcs, CompletionKind.Canceled);
        Assert.Equal(cts.Token, (await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await tcs.ValueTask)).CancellationToken);
    }

    [Theory, MemberData(nameof(Kinds))]
    public void ResetAfterCompletionReturnsToPending(CompletionKind kind)
    {
        var tcs = new ReusableTaskCompletionSource<int>();
        Complete(tcs, kind, 1);
        tcs.Reset();
        AssertState(tcs, null);
        Assert.False(tcs.ValueTask.IsCompleted);
    }

    [Fact]
    public void ResetWhilePendingThrows()
    {
        var tcs = new ReusableTaskCompletionSource<int>();
        Assert.Throws<InvalidOperationException>(tcs.Reset);
        AssertState(tcs, null);
    }

    [Fact]
    public void TryResetWhilePendingReturnsFalse()
    {
        var tcs = new ReusableTaskCompletionSource<int>();
        Assert.False(tcs.TryReset());
        AssertState(tcs, null);
    }

    [Theory, MemberData(nameof(Kinds))]
    public void TryResetAfterCompletionReturnsTrue(CompletionKind kind)
    {
        var tcs = new ReusableTaskCompletionSource<int>();
        Complete(tcs, kind, 1);
        Assert.True(tcs.TryReset());
        AssertState(tcs, null);
    }

    [Fact]
    public async Task StaleValueTaskThrowsAfterReset()
    {
        var tcs = new ReusableTaskCompletionSource<int>();
        var stale = tcs.ValueTask;
        tcs.SetResult(1);
        tcs.Reset();
        tcs.SetResult(2);
        await Assert.ThrowsAsync<InvalidOperationException>(async () => await stale);
    }

    [Fact]
    public void ValueTaskContinuationRunsAsynchronously()
    {
        var tcs = new ReusableTaskCompletionSource<int>();
        var continuationThread = ContinuationThreadId(c => tcs.ValueTask.ConfigureAwait(false).GetAwaiter().OnCompleted(c), () => tcs.SetResult(1));
        Assert.NotEqual(Environment.CurrentManagedThreadId, continuationThread);
    }

    [Theory, InlineData(false), InlineData(true)]
    public async Task PingPongAcrossThreadsDeliversEveryResult(bool consumeAsTask)
    {
        const int cycles = 2000;
        var ping = new ReusableTaskCompletionSource<int>();
        var pong = new ReusableTaskCompletionSource<int>();
        var mismatches = 0;

        async Task Echo()
        {
            for (var i = 0; i < cycles; i++)
            {
                var value = await Consume(ping, consumeAsTask);
                ping.Reset();
                pong.SetResult(value * 2);
            }
        }
        async Task Drive()
        {
            for (var i = 0; i < cycles; i++)
            {
                ping.SetResult(i);
                if (await Consume(pong, consumeAsTask) != i * 2)
                    mismatches++;
                pong.Reset();
            }
        }

        await Task.WhenAll(Task.Run(Echo, Ct), Task.Run(Drive, Ct)).WaitAsync(WaitTimeout, Ct);
        Assert.Equal(0, mismatches);
    }

    [Fact]
    public void StateRemainsConsistentAcrossVersionWrapAround()
    {
        var tcs = new ReusableTaskCompletionSource<int>();
        for (var i = 0; i <= ushort.MaxValue + 1; i++)
        {
            Assert.False(tcs.IsCompleted);
            Assert.False(tcs.ValueTask.IsCompleted);
            tcs.SetResult(i);
            Assert.Equal(i, ResultOf(tcs));
            tcs.Reset();
        }
    }

    [Fact]
    public void ValueTaskCycleDoesNotAllocate()
    {
        var tcs = new ReusableTaskCompletionSource<int>();
        var i = 0;
        Assert.Equal(0, AllocatedBytes(() => CycleThroughValueTask(tcs, i++)));
    }

    [Fact]
    public void SetFromTaskCopiesResult()
    {
        var tcs = new ReusableTaskCompletionSource<int>();
        tcs.SetFromTask(Task.FromResult(9));
        AssertState(tcs, CompletionKind.Result);
        Assert.Equal(9, ResultOf(tcs));
    }

    [Fact]
    public async Task SetFromTaskCopiesOriginalException()
    {
        var tcs = new ReusableTaskCompletionSource<int>();
        var ex = new FormatException();
        tcs.SetFromTask(Task.FromException<int>(ex));
        AssertState(tcs, CompletionKind.Exception);
        Assert.Same(ex, await Assert.ThrowsAsync<FormatException>(async () => await tcs.ValueTask));
    }

    [Fact]
    public async Task SetFromTaskCopiesCancellationToken()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        var tcs = new ReusableTaskCompletionSource<int>();
        tcs.SetFromTask(Task.FromCanceled<int>(cts.Token));
        AssertState(tcs, CompletionKind.Canceled);
        var oce = await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await tcs.ValueTask);
        Assert.Equal(cts.Token, oce.CancellationToken);
    }

    [Fact]
    public void SetFromTaskWithPendingTaskThrowsAndLeavesPending()
    {
        var tcs = new ReusableTaskCompletionSource<int>();
        Assert.Throws<InvalidOperationException>(() => tcs.SetFromTask(new TaskCompletionSource<int>().Task));
        AssertState(tcs, null);
    }

    [Fact]
    public void TrySetFromTaskWithPendingTaskThrowsAndLeavesPending()
    {
        var tcs = new ReusableTaskCompletionSource<int>();
        Assert.Throws<InvalidOperationException>(() => tcs.TrySetFromTask(new TaskCompletionSource<int>().Task));
        AssertState(tcs, null);
    }

    [Theory, MemberData(nameof(Kinds))]
    public void TrySetFromTaskOnPendingReturnsTrueAndCopiesState(CompletionKind kind)
    {
        var tcs = new ReusableTaskCompletionSource<int>();
        Assert.True(tcs.TrySetFromTask(CompletedTaskOf(kind, 9)));
        AssertState(tcs, kind);
        if (kind == CompletionKind.Result)
            Assert.Equal(9, ResultOf(tcs));
    }

    [Theory, MemberData(nameof(Kinds))]
    public void SetFromTaskAfterCompletionThrowsAndLeavesStateUnchanged(CompletionKind kind)
    {
        var tcs = new ReusableTaskCompletionSource<int>();
        tcs.SetResult(1);
        Assert.Throws<InvalidOperationException>(() => tcs.SetFromTask(CompletedTaskOf(kind, 2)));
        AssertState(tcs, CompletionKind.Result);
        Assert.Equal(1, ResultOf(tcs));
    }

    [Theory, MemberData(nameof(Kinds))]
    public void TrySetFromTaskAfterCompletionReturnsFalseAndLeavesStateUnchanged(CompletionKind kind)
    {
        var tcs = new ReusableTaskCompletionSource<int>();
        tcs.SetResult(1);
        Assert.False(tcs.TrySetFromTask(CompletedTaskOf(kind, 2)));
        AssertState(tcs, CompletionKind.Result);
        Assert.Equal(1, ResultOf(tcs));
    }
}

public class ReusableTaskCompletionSourceBaseTests
{
    [Fact]
    public void TokenAdvancesOnReset()
    {
        var tcs = new ReusableTaskCompletionSource<int>();
        var initial = tcs.Token;
        tcs.SetResult(1);
        Assert.Equal(initial, tcs.Token);
        tcs.Reset();
        Assert.Equal((short)(initial + 1), tcs.Token);
    }

    [Fact]
    public void IsCurrentMatchesOnlyCurrentToken()
    {
        var tcs = new ReusableTaskCompletionSource<int>();
        var initial = tcs.Token;
        Assert.True(tcs.IsCurrent(initial));
        tcs.SetResult(1);
        tcs.Reset();
        Assert.False(tcs.IsCurrent(initial));
        Assert.True(tcs.IsCurrent(tcs.Token));
    }

    [Fact]
    public void HasResultTracksCompletionOfCurrentToken()
    {
        var tcs = new ReusableTaskCompletionSource<int>();
        var token = tcs.Token;
        Assert.False(tcs.HasResult(null));
        Assert.False(tcs.HasResult(token));
        tcs.SetResult(1);
        Assert.True(tcs.HasResult(null));
        Assert.True(tcs.HasResult(token));
    }

    [Fact]
    public void HasResultWithStaleTokenThrows()
    {
        var tcs = new ReusableTaskCompletionSource<int>();
        var stale = tcs.Token;
        tcs.SetResult(1);
        tcs.Reset();
        Assert.Throws<InvalidOperationException>(() => tcs.HasResult(stale));
    }

    [Fact]
    public void ThrowIfTokenHasResultThrowsOnlyAfterCompletion()
    {
        var tcs = new ReusableTaskCompletionSource<int>();
        var token = tcs.Token;
        tcs.ThrowIfTokenHasResult(token);
        tcs.SetResult(1);
        Assert.Throws<InvalidOperationException>(() => tcs.ThrowIfTokenHasResult(token));
    }

    [Fact]
    public void ThrowIfTokenHasNoResultThrowsOnlyWhilePending()
    {
        var tcs = new ReusableTaskCompletionSource<int>();
        var token = tcs.Token;
        Assert.Throws<InvalidOperationException>(() => tcs.ThrowIfTokenHasNoResult(token));
        tcs.SetResult(1);
        tcs.ThrowIfTokenHasNoResult(token);
    }

    [Fact]
    public void ThrowCannotCopyResultStateFromUncompletedTaskThrows() => Assert.Throws<InvalidOperationException>(ReusableTaskCompletionSourceBase<int>.ThrowCannotCopyResultStateFromUncompletedTask);
}

public class NothingTests
{
    [Fact]
    public void AllInstancesAreEqual()
    {
        Assert.Equal(default, new Nothing());
        Assert.True(default(Nothing).Equals(new Nothing()));
        Assert.Equal(default(Nothing).GetHashCode(), new Nothing().GetHashCode());
    }
}
