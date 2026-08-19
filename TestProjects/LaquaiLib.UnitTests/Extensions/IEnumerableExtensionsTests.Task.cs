using LaquaiLib.Extensions;

namespace LaquaiLib.UnitTests.Extensions;

public class IEnumerableExtensionsTaskTests
{
    [Fact]
    public async Task GetAwaiterWaitsForAllTasksToComplete()
    {
        var counter = 0;
        var tasks = new List<Task>();

        for (var i = 0; i < 3; i++)
            tasks.Add(Task.Run(() => Interlocked.Increment(ref counter)));

        await tasks;

        Assert.Equal(3, counter);
    }

    [Fact]
    public void StartInitiatesAllTasks()
    {
        var counter = 0;
        var tasks = new List<Task>();

        for (var i = 0; i < 3; i++)
            tasks.Add(new Task(() => Interlocked.Increment(ref counter)));

        tasks.Start();
        Task.WaitAll([.. tasks], TestContext.Current.CancellationToken);

        Assert.Equal(3, counter);
    }

    [Fact]
    public void WaitAllBlocksUntilAllTasksComplete()
    {
        var counter = 0;
        var tasks = new List<Task>();

        for (var i = 0; i < 3; i++)
            tasks.Add(Task.Run(async () =>
            {
                await Task.Delay(50);
                Interlocked.Increment(ref counter);
            }, TestContext.Current.CancellationToken));

        tasks.WaitAll(cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(3, counter);
    }

    [Fact]
    public void WaitAllRespectsCancellation()
    {
        var cts = new CancellationTokenSource();
        var tasks = new List<Task>
        {
            Task.Run(async () => await Task.Delay(10000), TestContext.Current.CancellationToken)
        };

        cts.CancelAfter(50);

        Assert.Throws<OperationCanceledException>(() =>
            tasks.WaitAll(cts.Token));
    }

    [Fact]
    public void WaitAnyReturnsFirstCompletedTask()
    {
        // the loser never completes at all, so which task wins is settled rather than raced
        var completed = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var pending = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        completed.SetResult();

        var tasks = new[] { pending.Task, completed.Task };
        var completedTask = tasks.WaitAny(cancellationToken: TestContext.Current.CancellationToken);

        Assert.Same(completed.Task, completedTask);
        Assert.True(completedTask.IsCompleted);
    }

    [Fact]
    public async Task WaitAnyRespectsCancellation()
    {
        var cts = new CancellationTokenSource();
        var tasks = new List<Task>
        {
            Task.Run(async () => await Task.Delay(10000), TestContext.Current.CancellationToken)
        };

        cts.CancelAfter(50);

        await Assert.ThrowsAsync<OperationCanceledException>(() => tasks.WaitAny(cts.Token));
    }

    [Fact]
    public async Task WhenAllCompletesWhenAllTasksComplete()
    {
        var counter = 0;
        var tasks = new List<Task>();

        for (var i = 0; i < 3; i++)
            tasks.Add(Task.Run(async () =>
            {
                await Task.Delay(50 * i);
                Interlocked.Increment(ref counter);
            }, TestContext.Current.CancellationToken));

        await tasks.WhenAll();

        Assert.Equal(3, counter);
    }

    [Fact]
    public async Task WhenAnyReturnsFirstCompletedTask()
    {
        // the loser never completes at all, so which task wins is settled rather than raced
        var completed = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var pending = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        completed.SetResult();

        var tasks = new[] { pending.Task, completed.Task };
        var completedTask = await tasks.WhenAny();

        Assert.Same(completed.Task, completedTask);
    }

    [Fact]
    public async Task WhenEachYieldsTasksAsTheyComplete()
    {
        // completion is driven explicitly rather than by Task.Delay: delays only guarantee a lower
        // bound, so under load the intended order is not the observed one
        var first = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var second = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var third = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        // deliberately not in completion order, so source order cannot pass by accident
        var tasks = new List<Task> { third.Task, first.Task, second.Task };

        var completionOrder = new List<Task>();
        await using var enumerator = tasks.WhenEach().GetAsyncEnumerator(TestContext.Current.CancellationToken);

        // exactly one task is newly complete at each step, so only one task can be yielded
        first.SetResult();
        Assert.True(await enumerator.MoveNextAsync());
        completionOrder.Add(enumerator.Current);

        second.SetResult();
        Assert.True(await enumerator.MoveNextAsync());
        completionOrder.Add(enumerator.Current);

        third.SetResult();
        Assert.True(await enumerator.MoveNextAsync());
        completionOrder.Add(enumerator.Current);

        Assert.False(await enumerator.MoveNextAsync());

        Assert.Equal(3, completionOrder.Count);
        Assert.Same(first.Task, completionOrder[0]);
        Assert.Same(second.Task, completionOrder[1]);
        Assert.Same(third.Task, completionOrder[2]);
    }
}

public class IEnumerableTaskTResultExtensionsTests
{
    [Fact]
    public void WaitAllBlocksUntilAllTasksComplete()
    {
        var tasks = new List<Task<int>>
        {
            Task.Run(static async () => { await Task.Delay(50); return 1; }),
            Task.Run(static async () => { await Task.Delay(100); return 2; }),
            Task.Run(static async () => { await Task.Delay(150); return 3; })
        };

        tasks.WaitAll(cancellationToken: TestContext.Current.CancellationToken);

        Assert.All(tasks, static task => Assert.True(task.IsCompleted));
    }

    [Fact]
    public void WaitAllRespectsCancellation()
    {
        var cts = new CancellationTokenSource();
        var tasks = new List<Task<int>>
        {
            Task.Run(async () => { await Task.Delay(10000); return 1; })
        };

        cts.CancelAfter(50);

        Assert.Throws<OperationCanceledException>(() =>
            tasks.WaitAll(cts.Token));
    }

    [Fact]
    public async Task WaitAnyReturnsFirstCompletedTask()
    {
        // the loser never completes at all, so which task wins is settled rather than raced
        var completed = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
        var pending = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
        completed.SetResult(1);

        var tasks = new[] { pending.Task, completed.Task };
        var completedTask = tasks.WaitAny(cancellationToken: TestContext.Current.CancellationToken);

        Assert.Same(completed.Task, completedTask);
        Assert.Equal(1, await completedTask);
    }

    [Fact]
    public async Task WaitAnyRespectsCancellation()
    {
        var cts = new CancellationTokenSource();
        var tasks = new List<Task<int>>
        {
            Task.Run(async () => { await Task.Delay(10000); return 1; })
        };

        cts.CancelAfter(50);

        await Assert.ThrowsAsync<OperationCanceledException>(() => tasks.WaitAny(cts.Token));
    }

    [Fact]
    public async Task WhenAllCompletesWithResultsFromAllTasks()
    {
        var tasks = new List<Task<int>>
        {
            Task.Run(static async () => { await Task.Delay(50); return 1; }),
            Task.Run(static async () => { await Task.Delay(100); return 2; }),
            Task.Run(static async () => { await Task.Delay(150); return 3; })
        };

        var results = await tasks.WhenAll();

        Assert.Equal([1, 2, 3], results);
    }

    [Fact]
    public async Task WhenAnyReturnsFirstCompletedTaskWithResult()
    {
        // the loser never completes at all, so which task wins is settled rather than raced
        var completed = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
        var pending = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
        completed.SetResult(1);

        var tasks = new[] { pending.Task, completed.Task };
        var completedTask = await tasks.WhenAny();

        Assert.Same(completed.Task, completedTask);
        Assert.Equal(1, await completedTask);
    }

    [Fact]
    public async Task WhenEachYieldsTasksAsTheyCompleteWithResults()
    {
        // completion is driven explicitly rather than by Task.Delay: delays only guarantee a lower
        // bound, so under load the intended order is not the observed one
        var first = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
        var second = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
        var third = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
        // deliberately not in completion order, so source order cannot pass by accident
        var tasks = new List<Task<int>> { third.Task, first.Task, second.Task };

        var completionResults = new List<int>();
        await using var enumerator = tasks.WhenEach().GetAsyncEnumerator(TestContext.Current.CancellationToken);

        // exactly one task is newly complete at each step, so only one task can be yielded
        first.SetResult(2);
        Assert.True(await enumerator.MoveNextAsync());
        completionResults.Add(await enumerator.Current);

        second.SetResult(3);
        Assert.True(await enumerator.MoveNextAsync());
        completionResults.Add(await enumerator.Current);

        third.SetResult(1);
        Assert.True(await enumerator.MoveNextAsync());
        completionResults.Add(await enumerator.Current);

        Assert.False(await enumerator.MoveNextAsync());

        Assert.Equal(3, completionResults.Count);
        Assert.Equal(new[] { 2, 3, 1 }, completionResults);
    }

    [Fact]
    public async Task WhenAllHandlesEmptyCollection()
    {
        var emptyTasks = Array.Empty<Task<int>>();
        var results = await emptyTasks.WhenAll();

        Assert.Empty(results);
    }

    [Fact]
    public async Task WhenAllAggregatesExceptions()
    {
        var tasks = new List<Task<int>>
        {
            Task.Run<int>(async () => throw new InvalidOperationException("Error 1")),
            Task.Run<int>(async () => throw new ArgumentException("Error 2"))
        };

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(tasks.WhenAll);

        Assert.IsType<InvalidOperationException>(exception);
    }
}
