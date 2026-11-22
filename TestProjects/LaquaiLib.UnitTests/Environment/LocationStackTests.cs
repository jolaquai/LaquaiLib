using LaquaiLib.Env;

namespace LaquaiLib.UnitTests.Env;

[CollectionDefinition("LocationStack", DisableParallelization = true)]
public class LocationStackCollection;

[Collection("LocationStack")]
public class LocationStackTests : IDisposable
{
    private static readonly string Seed = System.Environment.CurrentDirectory;
    private readonly List<string> _dirs = [];

    public LocationStackTests()
    {
        System.Environment.CurrentDirectory = Seed;
        DrainStack();
    }

    public void Dispose()
    {
        DrainStack();
        System.Environment.CurrentDirectory = Seed;
        foreach (var dir in _dirs)
        {
            try
            {
                Directory.Delete(dir, true);
            }
            catch (IOException)
            {
            }
        }
    }

    private static void DrainStack()
    {
        for (var i = 0; i < 1000 && LocationStack.Pop() != Seed; i++)
        {
        }
    }

    private string NewDir()
    {
        var dir = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "LocationStackTests_" + Guid.NewGuid().ToString("N")));
        Directory.CreateDirectory(dir);
        _dirs.Add(dir);
        return dir;
    }

    private static string Cwd => System.Environment.CurrentDirectory;

    [Fact]
    public void PushSetsCurrentDirectory()
    {
        var a = NewDir();
        LocationStack.Push(a);
        Assert.Equal(a, Cwd);
    }

    [Fact]
    public void PopReturnsAndRestoresPreviousLocation()
    {
        var a = NewDir();
        LocationStack.Push(a);
        var result = LocationStack.Pop();
        Assert.Equal(Seed, result);
        Assert.Equal(Seed, Cwd);
    }

    [Fact]
    public void NestedPushesUnwindInOrder()
    {
        var a = NewDir();
        var b = NewDir();
        var c = NewDir();
        LocationStack.Push(a);
        LocationStack.Push(b);
        LocationStack.Push(c);
        Assert.Equal(c, Cwd);
        Assert.Equal(b, LocationStack.Pop());
        Assert.Equal(b, Cwd);
        Assert.Equal(a, LocationStack.Pop());
        Assert.Equal(Seed, LocationStack.Pop());
        Assert.Equal(Seed, Cwd);
    }

    [Fact]
    public void PopAtSeedReturnsSeedRepeatedly()
    {
        Assert.Equal(Seed, LocationStack.Pop());
        Assert.Equal(Seed, LocationStack.Pop());
        Assert.Equal(Seed, Cwd);
    }

    [Fact]
    public void PopAtSeedReappliesSeedAfterExternalChange()
    {
        var a = NewDir();
        System.Environment.CurrentDirectory = a;
        Assert.Equal(Seed, LocationStack.Pop());
        Assert.Equal(Seed, Cwd);
    }

    [Fact]
    public void PushResolvesRelativePathAgainstCurrentDirectory()
    {
        var a = NewDir();
        var child = Path.Combine(a, "child");
        Directory.CreateDirectory(child);
        LocationStack.Push(a);
        LocationStack.Push("child");
        Assert.Equal(child, Cwd);
        Assert.Equal(a, LocationStack.Pop());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void PushRejectsNullEmptyAndWhitespace(string location)
    {
        Assert.ThrowsAny<ArgumentException>(() => LocationStack.Push(location));
        Assert.Equal(Seed, Cwd);
    }

    [Fact]
    public void PushRejectsNonexistentDirectoryAndLeavesStateUntouched()
    {
        var missing = Path.Combine(NewDir(), "missing");
        Assert.Throws<DirectoryNotFoundException>(() => LocationStack.Push(missing));
        Assert.Equal(Seed, Cwd);
        Assert.Equal(Seed, LocationStack.Pop());
    }

    [Fact]
    public void PushRejectsFilePath()
    {
        var file = Path.Combine(NewDir(), "f.txt");
        File.WriteAllText(file, "x");
        Assert.Throws<DirectoryNotFoundException>(() => LocationStack.Push(file));
    }

    [Fact]
    public async Task InRunsInLocationAndRestores()
    {
        var a = NewDir();
        string inside = null;
        await LocationStack.In(a, () =>
        {
            inside = Cwd;
            return ValueTask.CompletedTask;
        });
        Assert.Equal(a, inside);
        Assert.Equal(Seed, Cwd);
    }

    [Fact]
    public async Task InRestoresToPushedLocationNotSeed()
    {
        var a = NewDir();
        var b = NewDir();
        LocationStack.Push(a);
        await LocationStack.In(b, () => ValueTask.CompletedTask);
        Assert.Equal(a, Cwd);
    }

    [Fact]
    public async Task InGenericReturnsResultAndRestores()
    {
        var a = NewDir();
        var result = await LocationStack.In(a, async () =>
        {
            await Task.Yield();
            return Cwd;
        });
        Assert.Equal(a, result);
        Assert.Equal(Seed, Cwd);
    }

    [Fact]
    public async Task InRestoresAfterAsyncSuspension()
    {
        var a = NewDir();
        await LocationStack.In(a, async () =>
        {
            await Task.Delay(10);
            Assert.Equal(a, Cwd);
        });
        Assert.Equal(Seed, Cwd);
    }

    [Fact]
    public async Task InRestoresAndRethrowsOnException()
    {
        var a = NewDir();
        await Assert.ThrowsAsync<InvalidOperationException>(() => LocationStack.In(a, () => throw new InvalidOperationException()).AsTask());
        Assert.Equal(Seed, Cwd);
    }

    [Fact]
    public async Task InGenericRestoresAndRethrowsOnException()
    {
        var a = NewDir();
        await Assert.ThrowsAsync<InvalidOperationException>(() => LocationStack.In<int>(a, async () =>
        {
            await Task.Yield();
            throw new InvalidOperationException();
        }).AsTask());
        Assert.Equal(Seed, Cwd);
    }

    [Fact]
    public async Task InRestoresOnCancellationFromAction()
    {
        var a = NewDir();
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => LocationStack.In(a, async () =>
        {
            await Task.Delay(Timeout.Infinite, cts.Token);
        }).AsTask());
        Assert.Equal(Seed, Cwd);
    }

    [Fact]
    public async Task InGenericRestoresOnCancellationFromAction()
    {
        var a = NewDir();
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => LocationStack.In<int>(a, async () =>
        {
            await Task.Delay(Timeout.Infinite, cts.Token);
            return 1;
        }).AsTask());
        Assert.Equal(Seed, Cwd);
    }

    [Fact]
    public async Task InCancelsWhileWaitingForAccess()
    {
        var a = NewDir();
        var gate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var entered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var holder = LocationStack.In(a, async () =>
        {
            entered.SetResult();
            await gate.Task;
        }).AsTask();
        await entered.Task;

        using var cts = new CancellationTokenSource();
        var ran = false;
        var waiter = LocationStack.In(a, () =>
        {
            ran = true;
            return ValueTask.CompletedTask;
        }, cts.Token).AsTask();
        await cts.CancelAsync();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => waiter);
        gate.SetResult();
        await holder;

        Assert.False(ran);
        Assert.Equal(Seed, Cwd);
    }

    [Fact]
    public async Task InRejectsInvalidArguments()
    {
        var a = NewDir();
        await Assert.ThrowsAnyAsync<ArgumentException>(() => LocationStack.In(null, () => ValueTask.CompletedTask).AsTask());
        await Assert.ThrowsAnyAsync<ArgumentException>(() => LocationStack.In("", () => ValueTask.CompletedTask).AsTask());
        await Assert.ThrowsAsync<ArgumentNullException>(() => LocationStack.In(a, null).AsTask());
        await Assert.ThrowsAsync<ArgumentNullException>(() => LocationStack.In<int>(a, null).AsTask());
    }

    [Fact]
    public async Task InWithNonexistentDirectoryThrowsWithoutRunningActionAndReleasesStack()
    {
        var missing = Path.Combine(NewDir(), "missing");
        var ran = false;
        await Assert.ThrowsAsync<DirectoryNotFoundException>(() => LocationStack.In(missing, () =>
        {
            ran = true;
            return ValueTask.CompletedTask;
        }).AsTask());
        Assert.False(ran);
        Assert.Equal(Seed, Cwd);

        var a = NewDir();
        await LocationStack.In(a, () => ValueTask.CompletedTask);
        Assert.Equal(Seed, Cwd);
    }

    [Fact]
    public async Task PushAndPopInsideInThrowInsteadOfDeadlocking()
    {
        var a = NewDir();
        var b = NewDir();
        await LocationStack.In(a, () =>
        {
            Assert.Throws<InvalidOperationException>(() => LocationStack.Push(b));
            Assert.Throws<InvalidOperationException>(() => LocationStack.Pop());
            Assert.Equal(a, Cwd);
            return ValueTask.CompletedTask;
        });
        Assert.Equal(Seed, Cwd);
    }

    [Fact]
    public async Task NestedInThrowsInsteadOfDeadlocking()
    {
        var a = NewDir();
        var b = NewDir();
        await LocationStack.In(a, async () =>
        {
            await Assert.ThrowsAsync<InvalidOperationException>(() => LocationStack.In(b, () => ValueTask.CompletedTask).AsTask());
            await Assert.ThrowsAsync<InvalidOperationException>(() => LocationStack.In<int>(b, () => ValueTask.FromResult(1)).AsTask());
        });
        Assert.Equal(Seed, Cwd);
    }

    [Fact]
    public async Task PushWorksAgainAfterInCompletes()
    {
        var a = NewDir();
        await LocationStack.In(a, () => ValueTask.CompletedTask);
        LocationStack.Push(a);
        Assert.Equal(a, Cwd);
    }

    [Fact]
    public async Task ConcurrentInCallsAreSerialized()
    {
        const int count = 16;
        var dirs = Enumerable.Range(0, count).Select(_ => NewDir()).ToArray();
        var active = 0;
        var maxActive = 0;
        var mismatches = 0;

        var tasks = dirs.Select(dir => Task.Run(() => LocationStack.In(dir, async () =>
        {
            var now = Interlocked.Increment(ref active);
            InterlockedMax(ref maxActive, now);
            await Task.Delay(5);
            if (Cwd != dir)
                Interlocked.Increment(ref mismatches);
            Interlocked.Decrement(ref active);
        }).AsTask())).ToArray();
        await Task.WhenAll(tasks);

        Assert.Equal(1, maxActive);
        Assert.Equal(0, mismatches);
        Assert.Equal(Seed, Cwd);
    }

    [Fact]
    public async Task ConcurrentInGenericCallsReturnTheirOwnLocation()
    {
        var dirs = Enumerable.Range(0, 16).Select(_ => NewDir()).ToArray();
        var tasks = dirs.Select(dir => Task.Run(() => LocationStack.In(dir, async () =>
        {
            await Task.Yield();
            return Cwd;
        }).AsTask())).ToArray();
        var results = await Task.WhenAll(tasks);

        Assert.Equal(dirs, results);
        Assert.Equal(Seed, Cwd);
    }

    [Fact]
    public async Task PushBlocksWhileInScopeIsActiveOnAnotherFlow()
    {
        var a = NewDir();
        var b = NewDir();
        var gate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var entered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var holder = LocationStack.In(a, async () =>
        {
            entered.SetResult();
            await gate.Task;
        }).AsTask();
        await entered.Task;

        var pusher = Task.Run(() => LocationStack.Push(b));
        await Task.Delay(50);
        Assert.False(pusher.IsCompleted);
        gate.SetResult();
        await holder;
        await pusher;

        Assert.Equal(b, Cwd);
    }

    private static void InterlockedMax(ref int target, int value)
    {
        int current;
        while (value > (current = Volatile.Read(ref target)))
        {
            if (Interlocked.CompareExchange(ref target, value, current) == current)
                break;
        }
    }
}
