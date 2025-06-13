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
        {
            tasks.Add(Task.Run(() => Interlocked.Increment(ref counter)));
        }

        await tasks;

        Assert.Equal(3, counter);
    }

    [Fact]
    public void StartInitiatesAllTasks()
    {
        var counter = 0;
        var tasks = new List<Task>();

        for (var i = 0; i < 3; i++)
        {
            tasks.Add(new Task(() => Interlocked.Increment(ref counter)));
        }

        tasks.Start();
        Task.WaitAll(tasks.ToArray(), TestContext.Current.CancellationToken);

        Assert.Equal(3, counter);
    }

    [Fact]
    public void WaitAllBlocksUntilAllTasksComplete()
    {
        var counter = 0;
        var tasks = new List<Task>();

        for (var i = 0; i < 3; i++)
        {
            tasks.Add(Task.Run(async () =>
            {
                await Task.Delay(50);
                Interlocked.Increment(ref counter);
            }, TestContext.Current.CancellationToken));
        }

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
        var fastTask = Task.Run(static async () => await Task.Delay(50), TestContext.Current.CancellationToken);
        var slowTask = Task.Run(static async () => await Task.Delay(500), TestContext.Current.CancellationToken);

        var tasks = new[] { slowTask, fastTask };
        var completedTask = tasks.WaitAny(cancellationToken: TestContext.Current.CancellationToken);

        Assert.Same(fastTask, completedTask);
        Assert.True(fastTask.IsCompleted);
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
        {
            tasks.Add(Task.Run(async () =>
            {
                await Task.Delay(50 * i);
                Interlocked.Increment(ref counter);
            }, TestContext.Current.CancellationToken));
        }

        await tasks.WhenAll();

        Assert.Equal(3, counter);
    }

    [Fact]
    public async Task WhenAnyReturnsFirstCompletedTask()
    {
        var fastTask = Task.Run(static async () => await Task.Delay(50), TestContext.Current.CancellationToken);
        var slowTask = Task.Run(static async () => await Task.Delay(500), TestContext.Current.CancellationToken);

        var tasks = new[] { slowTask, fastTask };
        var completedTask = await tasks.WhenAny();

        Assert.Same(fastTask, completedTask);
    }

    [Fact]
    public async Task WhenEachYieldsTasksAsTheyComplete()
    {
        var tasks = new List<Task>
        {
            Task.Run(static async () => await Task.Delay(300), TestContext.Current.CancellationToken),
            Task.Run(static async () => await Task.Delay(100), TestContext.Current.CancellationToken),
            Task.Run(static async () => await Task.Delay(200), TestContext.Current.CancellationToken)
        };

        var completionOrder = new List<Task>();

        await foreach (var task in tasks.WhenEach())
        {
            completionOrder.Add(task);
        }

        Assert.Equal(3, completionOrder.Count);
        Assert.Same(tasks[1], completionOrder[0]); // 100ms delay completes first
        Assert.Same(tasks[2], completionOrder[1]); // 200ms delay completes second
        Assert.Same(tasks[0], completionOrder[2]); // 300ms delay completes last
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
        var fastTask = Task.Run(static async () => { await Task.Delay(50); return 1; });
        var slowTask = Task.Run(static async () => { await Task.Delay(500); return 2; });

        var tasks = new[] { slowTask, fastTask };
        var completedTask = tasks.WaitAny(cancellationToken: TestContext.Current.CancellationToken);

        Assert.Same(fastTask, completedTask);
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
        var fastTask = Task.Run(static async () => { await Task.Delay(50); return 1; });
        var slowTask = Task.Run(static async () => { await Task.Delay(500); return 2; });

        var tasks = new[] { slowTask, fastTask };
        var completedTask = await tasks.WhenAny();

        Assert.Same(fastTask, completedTask);
        Assert.Equal(1, await completedTask);
    }

    [Fact]
    public async Task WhenEachYieldsTasksAsTheyCompleteWithResults()
    {
        var tasks = new List<Task<int>>
        {
            Task.Run(static async () => { await Task.Delay(300); return 1; }),
            Task.Run(static async () => { await Task.Delay(100); return 2; }),
            Task.Run(static async () => { await Task.Delay(200); return 3; })
        };

        var completionResults = new List<int>();

        await foreach (var task in tasks.WhenEach())
        {
            completionResults.Add(await task);
        }

        Assert.Equal(3, completionResults.Count);
        Assert.Equal(2, completionResults[0]); // 100ms delay completes first with result 2
        Assert.Equal(3, completionResults[1]); // 200ms delay completes second with result 3
        Assert.Equal(1, completionResults[2]); // 300ms delay completes last with result 1
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
