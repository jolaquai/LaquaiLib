using System.Globalization;

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

    protected static void AssertTaskState(Task task, CompletionKind kind)
    {
        Assert.Equal(kind == CompletionKind.Result, task.IsCompletedSuccessfully);
        Assert.Equal(kind == CompletionKind.Exception, task.IsFaulted);
        Assert.Equal(kind == CompletionKind.Canceled, task.IsCanceled);
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
    private static ValueTask Consume(ReusableTaskCompletionSource tcs, bool asTask) => asTask ? new ValueTask(tcs.Task) : tcs.ValueTask;
    private static void CycleThroughValueTask(ReusableTaskCompletionSource tcs)
    {
        tcs.SetResult();
        tcs.ValueTask.GetAwaiter().GetResult();
        tcs.Reset();
    }
    private static void CycleThroughTask(ReusableTaskCompletionSource tcs)
    {
        tcs.SetResult();
        tcs.Task.GetAwaiter().GetResult();
        tcs.Reset();
    }

    [Fact]
    public void NewInstanceIsPending()
    {
        var tcs = new ReusableTaskCompletionSource();
        AssertState(tcs, null);
        Assert.False(tcs.ValueTask.IsCompleted);
        Assert.False(tcs.Task.IsCompleted);
    }

    [Fact]
    public void SetResultCompletesSuccessfully()
    {
        var tcs = new ReusableTaskCompletionSource();
        tcs.SetResult();
        AssertState(tcs, CompletionKind.Result);
        Assert.True(tcs.ValueTask.IsCompletedSuccessfully);
        Assert.True(tcs.Task.IsCompletedSuccessfully);
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

    [Fact]
    public async Task SetResultCompletesTaskObtainedWhilePending()
    {
        var tcs = new ReusableTaskCompletionSource();
        var task = tcs.Task;
        Assert.False(task.IsCompleted);
        tcs.SetResult();
        await task.WaitAsync(WaitTimeout, Ct);
        Assert.True(task.IsCompletedSuccessfully);
    }

    [Theory, MemberData(nameof(Kinds))]
    public async Task ResetRightAfterCompletionPreservesTaskObtainedWhilePending(CompletionKind kind)
    {
        var tcs = new ReusableTaskCompletionSource();
        var task = tcs.Task;
        Complete(tcs, kind);
        tcs.Reset();
        await Task.WhenAny(task).WaitAsync(WaitTimeout, Ct);
        AssertTaskState(task, kind);
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
    public async Task SetExceptionFaultsTaskWithSameException()
    {
        var tcs = new ReusableTaskCompletionSource();
        var ex = new FormatException();
        tcs.SetException(ex);
        var task = tcs.Task;
        Assert.True(task.IsFaulted);
        Assert.Same(ex, task.Exception.InnerException);
        Assert.Same(ex, await Assert.ThrowsAsync<FormatException>(() => task));
    }

    [Fact]
    public async Task SetExceptionFaultsTaskObtainedWhilePending()
    {
        var tcs = new ReusableTaskCompletionSource();
        var ex = new FormatException();
        var task = tcs.Task;
        tcs.SetException(ex);
        Assert.Same(ex, await Assert.ThrowsAsync<FormatException>(() => task.WaitAsync(WaitTimeout, Ct)));
        Assert.True(task.IsFaulted);
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
    public async Task SetCanceledCancelsTaskWithToken()
    {
        using var cts = new CancellationTokenSource();
        var tcs = new ReusableTaskCompletionSource();
        tcs.SetCanceled(cts.Token);
        var task = tcs.Task;
        Assert.True(task.IsCanceled);
        var oce = await Assert.ThrowsAnyAsync<OperationCanceledException>(() => task);
        Assert.Equal(cts.Token, oce.CancellationToken);
    }

    [Fact]
    public async Task SetCanceledCancelsTaskObtainedWhilePending()
    {
        using var cts = new CancellationTokenSource();
        var tcs = new ReusableTaskCompletionSource();
        var task = tcs.Task;
        tcs.SetCanceled(cts.Token);
        var oce = await Assert.ThrowsAnyAsync<OperationCanceledException>(() => task.WaitAsync(WaitTimeout, Ct));
        Assert.True(task.IsCanceled);
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
        Assert.False(tcs.Task.IsCompleted);
    }

    [Theory, MemberData(nameof(Kinds))]
    public void TaskAfterResetIsPendingWhenPreviousCycleTaskWasObtained(CompletionKind kind)
    {
        var tcs = new ReusableTaskCompletionSource();
        Complete(tcs, kind);
        var previous = tcs.Task;
        tcs.Reset();
        var current = tcs.Task;
        Assert.NotSame(previous, current);
        Assert.False(current.IsCompleted);
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
    public void TaskFromPreviousCycleKeepsItsState()
    {
        var tcs = new ReusableTaskCompletionSource();
        var ex = new FormatException();
        tcs.SetException(ex);
        var previous = tcs.Task;
        tcs.Reset();
        tcs.SetResult();
        Assert.True(previous.IsFaulted);
        Assert.Same(ex, previous.Exception.InnerException);
        Assert.NotSame(previous, tcs.Task);
        Assert.True(tcs.Task.IsCompletedSuccessfully);
    }

    [Fact]
    public void TaskIsSameInstanceWithinCycle()
    {
        var tcs = new ReusableTaskCompletionSource();
        var pending = tcs.Task;
        Assert.Same(pending, tcs.Task);
        tcs.SetResult();
        Assert.Same(pending, tcs.Task);
    }

    [Fact]
    public void TaskIsReusedAcrossCyclesWhenObtainedAfterCompletion()
    {
        var tcs = new ReusableTaskCompletionSource();
        tcs.SetResult();
        var first = tcs.Task;
        tcs.Reset();
        tcs.SetResult();
        Assert.Same(first, tcs.Task);
    }

    [Fact]
    public async Task MultipleAwaitersOfPendingTaskAllComplete()
    {
        var tcs = new ReusableTaskCompletionSource();
        var task = tcs.Task;
        var awaiters = Enumerable.Range(0, 4).Select(async _ => await task).ToArray();
        Assert.All(awaiters, static a => Assert.False(a.IsCompleted));
        tcs.SetResult();
        await Task.WhenAll(awaiters).WaitAsync(WaitTimeout, Ct);
    }

    [Fact]
    public void ValueTaskContinuationRunsAsynchronously()
    {
        var tcs = new ReusableTaskCompletionSource();
        var continuationThread = ContinuationThreadId(c => tcs.ValueTask.ConfigureAwait(false).GetAwaiter().OnCompleted(c), tcs.SetResult);
        Assert.NotEqual(Environment.CurrentManagedThreadId, continuationThread);
    }

    [Fact]
    public void TaskContinuationRunsAsynchronously()
    {
        var tcs = new ReusableTaskCompletionSource();
        var continuationThread = ContinuationThreadId(c => tcs.Task.ConfigureAwait(false).GetAwaiter().OnCompleted(c), tcs.SetResult);
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

    [Theory, InlineData(false), InlineData(true)]
    public void TaskIsPendingAfterVersionWrapsAround(bool obtainTaskEachCycle)
    {
        var tcs = new ReusableTaskCompletionSource();
        for (var i = 0; i < ushort.MaxValue; i++)
        {
            tcs.SetResult();
            if (obtainTaskEachCycle)
                _ = tcs.Task;
            tcs.Reset();
        }
        var task = tcs.Task;
        Assert.NotNull(task);
        Assert.False(task.IsCompleted);
    }

    [Fact]
    public void ValueTaskCycleDoesNotAllocate()
    {
        var tcs = new ReusableTaskCompletionSource();
        Assert.Equal(0, AllocatedBytes(() => CycleThroughValueTask(tcs)));
    }

    [Fact]
    public void TaskCycleDoesNotAllocateWhenObtainedAfterCompletion()
    {
        var tcs = new ReusableTaskCompletionSource();
        Assert.Equal(0, AllocatedBytes(() => CycleThroughTask(tcs)));
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
    private static ValueTask<T> Consume<T>(ReusableTaskCompletionSource<T> tcs, bool asTask) => asTask ? new ValueTask<T>(tcs.Task) : tcs.ValueTask;
    private static T ResultOf<T>(ReusableTaskCompletionSource<T> tcs) => tcs.ValueTask.GetAwaiter().GetResult();
    private static T TaskResult<T>(Task<T> task) => task.GetAwaiter().GetResult();
    private static bool WaitFor(Task task) => task.Wait(WaitTimeout);

    private sealed class GatedSynchronizationContext : SynchronizationContext, IDisposable
    {
        public ManualResetEventSlim Entered { get; } = new();
        public ManualResetEventSlim Release { get; } = new();

        public override void Post(SendOrPostCallback d, object state)
        {
            Entered.Set();
            Release.Wait();
            d(state);
        }

        public void Dispose()
        {
            Entered.Dispose();
            Release.Dispose();
        }
    }
    private static void RegisterNoOpContinuation<T>(ReusableTaskCompletionSource<T> tcs) => tcs.ValueTask.ConfigureAwait(false).GetAwaiter().OnCompleted(static () => { });
    private static void CycleThroughValueTask(ReusableTaskCompletionSource<int> tcs, int result)
    {
        tcs.SetResult(result);
        tcs.ValueTask.GetAwaiter().GetResult();
        tcs.Reset();
    }
    private static void CycleThroughTask(ReusableTaskCompletionSource<int> tcs, int result)
    {
        tcs.SetResult(result);
        tcs.Task.GetAwaiter().GetResult();
        tcs.Reset();
    }

    [Fact]
    public void NewInstanceIsPending()
    {
        var tcs = new ReusableTaskCompletionSource<int>();
        AssertState(tcs, null);
        Assert.False(tcs.ValueTask.IsCompleted);
        Assert.False(tcs.Task.IsCompleted);
    }

    [Fact]
    public async Task SetResultIsObservableThroughValueTaskAndTask()
    {
        var tcs = new ReusableTaskCompletionSource<int>();
        tcs.SetResult(42);
        AssertState(tcs, CompletionKind.Result);
        Assert.True(tcs.ValueTask.IsCompletedSuccessfully);
        Assert.True(tcs.Task.IsCompletedSuccessfully);
        Assert.Equal(42, await tcs.ValueTask);
        Assert.Equal(42, await tcs.Task);
    }

    [Fact]
    public async Task SetResultWithNullReferenceIsObservable()
    {
        var tcs = new ReusableTaskCompletionSource<string>();
        tcs.SetResult(null);
        AssertState(tcs, CompletionKind.Result);
        Assert.Null(await tcs.ValueTask);
        Assert.Null(await tcs.Task);
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
    public async Task SetResultCompletesTaskObtainedWhilePendingWithResult()
    {
        var tcs = new ReusableTaskCompletionSource<int>();
        var task = tcs.Task;
        Assert.False(task.IsCompleted);
        tcs.SetResult(7);
        Assert.Equal(7, await task.WaitAsync(WaitTimeout, Ct));
    }

    [Theory, MemberData(nameof(Kinds))]
    public async Task ResetRightAfterCompletionPreservesTaskObtainedWhilePending(CompletionKind kind)
    {
        var tcs = new ReusableTaskCompletionSource<int>();
        var task = tcs.Task;
        Complete(tcs, kind, 7);
        tcs.Reset();
        await Task.WhenAny(task).WaitAsync(WaitTimeout, Ct);
        AssertTaskState(task, kind);
        if (kind == CompletionKind.Result)
            Assert.Equal(7, await task);
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
    public async Task SetExceptionFaultsValueTaskAndTaskWithSameException()
    {
        var tcs = new ReusableTaskCompletionSource<int>();
        var ex = new FormatException();
        tcs.SetException(ex);
        AssertState(tcs, CompletionKind.Exception);
        Assert.Same(ex, await Assert.ThrowsAsync<FormatException>(async () => await tcs.ValueTask));
        Assert.True(tcs.Task.IsFaulted);
        Assert.Same(ex, await Assert.ThrowsAsync<FormatException>(() => tcs.Task));
    }

    [Fact]
    public async Task SetCanceledCancelsValueTaskAndTaskWithToken()
    {
        using var cts = new CancellationTokenSource();
        var tcs = new ReusableTaskCompletionSource<int>();
        tcs.SetCanceled(cts.Token);
        AssertState(tcs, CompletionKind.Canceled);
        Assert.Equal(cts.Token, (await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await tcs.ValueTask)).CancellationToken);
        Assert.True(tcs.Task.IsCanceled);
        Assert.Equal(cts.Token, (await Assert.ThrowsAnyAsync<OperationCanceledException>(() => tcs.Task)).CancellationToken);
    }

    [Theory, MemberData(nameof(Kinds))]
    public void ResetAfterCompletionReturnsToPending(CompletionKind kind)
    {
        var tcs = new ReusableTaskCompletionSource<int>();
        Complete(tcs, kind, 1);
        tcs.Reset();
        AssertState(tcs, null);
        Assert.False(tcs.ValueTask.IsCompleted);
        Assert.False(tcs.Task.IsCompleted);
    }

    [Theory, MemberData(nameof(Kinds))]
    public void TaskAfterResetIsPendingWhenPreviousCycleTaskWasObtained(CompletionKind kind)
    {
        var tcs = new ReusableTaskCompletionSource<int>();
        Complete(tcs, kind, 1);
        var previous = tcs.Task;
        tcs.Reset();
        var current = tcs.Task;
        Assert.NotSame(previous, current);
        Assert.False(current.IsCompleted);
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
    public async Task TaskFromPreviousCycleKeepsItsResult()
    {
        var tcs = new ReusableTaskCompletionSource<int>();
        tcs.SetResult(1);
        var previous = tcs.Task;
        tcs.Reset();
        tcs.SetResult(2);
        Assert.NotSame(previous, tcs.Task);
        Assert.Equal(1, await previous);
        Assert.Equal(2, await tcs.Task);
    }

    [Fact]
    public async Task TaskObtainedWhilePendingKeepsItsResultAfterLaterCycles()
    {
        var tcs = new ReusableTaskCompletionSource<int>();
        var previous = tcs.Task;
        tcs.SetResult(1);
        Assert.Equal(1, await previous.WaitAsync(WaitTimeout, Ct));
        tcs.Reset();
        tcs.SetResult(2);
        Assert.Equal(1, await previous);
        Assert.Equal(2, await tcs.Task);
    }

    [Fact]
    public void TaskIsSameInstanceWithinCycle()
    {
        var tcs = new ReusableTaskCompletionSource<int>();
        var pending = tcs.Task;
        Assert.Same(pending, tcs.Task);
        tcs.SetResult(1);
        Assert.Same(pending, tcs.Task);
    }

    [Fact]
    public void TaskIsReusedAcrossCyclesForEqualValueTypeResult()
    {
        var tcs = new ReusableTaskCompletionSource<int>();
        tcs.SetResult(1000);
        var first = tcs.Task;
        tcs.Reset();
        tcs.SetResult(1000);
        Assert.Same(first, tcs.Task);
    }

    [Fact]
    public async Task TaskIsReplacedAcrossCyclesForDifferentResult()
    {
        var tcs = new ReusableTaskCompletionSource<int>();
        tcs.SetResult(1000);
        var first = tcs.Task;
        tcs.Reset();
        tcs.SetResult(2000);
        Assert.NotSame(first, tcs.Task);
        Assert.Equal(1000, await first);
        Assert.Equal(2000, await tcs.Task);
    }

    [Fact]
    public async Task TaskCarriesCurrentReferenceTypeInstanceForEqualResult()
    {
        var first = new string('x', 3);
        var second = new string('x', 3);
        var tcs = new ReusableTaskCompletionSource<string>();
        tcs.SetResult(first);
        Assert.Same(first, await tcs.Task);
        tcs.Reset();
        tcs.SetResult(second);
        Assert.Same(second, await tcs.Task);
    }

    [Fact]
    public async Task TaskPreservesNegativeZeroAfterPositiveZero()
    {
        var tcs = new ReusableTaskCompletionSource<double>();
        tcs.SetResult(0.0);
        _ = tcs.Task;
        tcs.Reset();
        tcs.SetResult(-0.0);
        Assert.True(double.IsNegative(await tcs.ValueTask));
        Assert.True(double.IsNegative(await tcs.Task));
    }

    [Fact]
    public async Task TaskPreservesDecimalScaleAfterEqualDecimal()
    {
        var tcs = new ReusableTaskCompletionSource<decimal>();
        tcs.SetResult(1.0m);
        _ = tcs.Task;
        tcs.Reset();
        tcs.SetResult(1.00m);
        Assert.Equal("1.00", (await tcs.ValueTask).ToString(CultureInfo.InvariantCulture));
        Assert.Equal("1.00", (await tcs.Task).ToString(CultureInfo.InvariantCulture));
    }

    [Fact]
    public async Task TaskAfterFaultedCycleCarriesNewResult()
    {
        var tcs = new ReusableTaskCompletionSource<int>();
        tcs.SetResult(1000);
        _ = tcs.Task;
        tcs.Reset();
        tcs.SetException(new FormatException());
        var faulted = tcs.Task;
        tcs.Reset();
        tcs.SetResult(1000);
        Assert.True(faulted.IsFaulted);
        Assert.True(tcs.Task.IsCompletedSuccessfully);
        Assert.Equal(1000, await tcs.Task);
    }

    [Fact]
    public async Task MultipleAwaitersOfPendingTaskAllObserveResult()
    {
        var tcs = new ReusableTaskCompletionSource<int>();
        var task = tcs.Task;
        var awaiters = Enumerable.Range(0, 4).Select(async _ => await task).ToArray();
        tcs.SetResult(5);
        Assert.All(await Task.WhenAll(awaiters).WaitAsync(WaitTimeout, Ct), static r => Assert.Equal(5, r));
    }

    [Fact]
    public void ValueTaskContinuationRunsAsynchronously()
    {
        var tcs = new ReusableTaskCompletionSource<int>();
        var continuationThread = ContinuationThreadId(c => tcs.ValueTask.ConfigureAwait(false).GetAwaiter().OnCompleted(c), () => tcs.SetResult(1));
        Assert.NotEqual(Environment.CurrentManagedThreadId, continuationThread);
    }

    [Fact]
    public void TaskContinuationRunsAsynchronously()
    {
        var tcs = new ReusableTaskCompletionSource<int>();
        var continuationThread = ContinuationThreadId(c => tcs.Task.ConfigureAwait(false).GetAwaiter().OnCompleted(c), () => tcs.SetResult(1));
        Assert.NotEqual(Environment.CurrentManagedThreadId, continuationThread);
    }

    [Fact]
    public void ConcurrentTrySetResultHasExactlyOneWinner()
    {
        const int iterations = 50000;
        var tcs = new ReusableTaskCompletionSource<int>();
        using var barrier = new Barrier(3);
        var wins = 0;
        var throws = 0;
        var stop = false;

        void Contend(int value)
        {
            while (true)
            {
                barrier.SignalAndWait();
                if (Volatile.Read(ref stop))
                    return;
                try
                {
                    if (tcs.TrySetResult(value))
                        Interlocked.Increment(ref wins);
                }
                catch (InvalidOperationException)
                {
                    Interlocked.Increment(ref throws);
                }
                barrier.SignalAndWait();
            }
        }

        Thread[] threads = [new(() => Contend(1)) { IsBackground = true }, new(() => Contend(2)) { IsBackground = true }];
        foreach (var thread in threads)
            thread.Start();

        var failures = 0;
        for (var i = 0; i < iterations && failures == 0; i++)
        {
            RegisterNoOpContinuation(tcs);
            barrier.SignalAndWait(Ct);
            barrier.SignalAndWait(Ct);
            if (Interlocked.Exchange(ref wins, 0) != 1 | Interlocked.Exchange(ref throws, 0) != 0)
                failures++;
            tcs.TryReset();
        }

        Volatile.Write(ref stop, true);
        barrier.SignalAndWait(Ct);
        foreach (var thread in threads)
            thread.Join();
        Assert.Equal(0, failures);
    }

    [Fact]
    public void TaskObtainedConcurrentlyWithCompletionReceivesResult()
    {
        const int iterations = 50000;
        var tcs = new ReusableTaskCompletionSource<int>();
        using var barrier = new Barrier(2);
        var value = 0;
        var stop = false;

        var completer = new Thread(() =>
        {
            while (true)
            {
                barrier.SignalAndWait();
                if (Volatile.Read(ref stop))
                    return;
                tcs.SetResult(Volatile.Read(ref value));
                barrier.SignalAndWait();
            }
        })
        { IsBackground = true };
        completer.Start();

        var failures = 0;
        for (var i = 0; i < iterations && failures == 0; i++)
        {
            Volatile.Write(ref value, i);
            barrier.SignalAndWait(Ct);
            var task = tcs.Task;
            barrier.SignalAndWait(Ct);
            if (!WaitFor(task) || TaskResult(task) != i)
                failures++;
            tcs.Reset();
        }

        Volatile.Write(ref stop, true);
        barrier.SignalAndWait(Ct);
        completer.Join();
        Assert.Equal(0, failures);
    }

    [Fact]
    public void CompleterStalledAfterSignalingNeverCompletesNextCycleTask()
    {
        var tcs = new ReusableTaskCompletionSource<int>();
        using var context = new GatedSynchronizationContext();
        var previousContext = SynchronizationContext.Current;
        SynchronizationContext.SetSynchronizationContext(context);
        try
        {
            tcs.ValueTask.GetAwaiter().UnsafeOnCompleted(static () => { });
        }
        finally
        {
            SynchronizationContext.SetSynchronizationContext(previousContext);
        }
        var completer = new Thread(() => tcs.SetResult(1)) { IsBackground = true };
        completer.Start();
        Task<int> next;
        try
        {
            Assert.True(context.Entered.Wait(WaitTimeout, Ct));
            tcs.Reset();
            next = tcs.Task;
        }
        finally
        {
            context.Release.Set();
        }
        Assert.True(completer.Join(WaitTimeout));

        Assert.False(next.IsCompleted);
        tcs.SetResult(2);
        Assert.True(WaitFor(next));
        Assert.Equal(2, TaskResult(next));
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

    [Theory, InlineData(false), InlineData(true)]
    public void TaskIsPendingAfterVersionWrapsAround(bool obtainTaskEachCycle)
    {
        var tcs = new ReusableTaskCompletionSource<int>();
        for (var i = 0; i < ushort.MaxValue; i++)
        {
            tcs.SetResult(i);
            if (obtainTaskEachCycle)
                _ = tcs.Task;
            tcs.Reset();
        }
        var task = tcs.Task;
        Assert.NotNull(task);
        Assert.False(task.IsCompleted);
    }

    [Fact]
    public void ValueTaskCycleDoesNotAllocate()
    {
        var tcs = new ReusableTaskCompletionSource<int>();
        var i = 0;
        Assert.Equal(0, AllocatedBytes(() => CycleThroughValueTask(tcs, i++)));
    }

    [Fact]
    public void TaskCycleWithRepeatedResultDoesNotAllocate()
    {
        var tcs = new ReusableTaskCompletionSource<int>();
        Assert.Equal(0, AllocatedBytes(() => CycleThroughTask(tcs, 1000)));
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
