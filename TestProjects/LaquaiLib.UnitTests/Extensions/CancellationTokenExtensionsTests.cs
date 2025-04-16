using LaquaiLib.Extensions;

namespace LaquaiLib.UnitTests.Extensions;

public class CancellationTokenExtensionsTests
{
    [Fact]
    public async Task WhenCancelledTokenCancelledTaskCompletes()
    {
        using var cts = new CancellationTokenSource();
        var completionTask = cts.Token.WhenCancelled();

        cts.Cancel();
        var completedTask = await Task.WhenAny(completionTask, Task.Delay(1000));

        Assert.Equal(completionTask, completedTask);
        Assert.True(completionTask.IsCompletedSuccessfully);
    }

    [Fact]
    public async Task WhenCancelledTokenAlreadyCancelledTaskCompletesImmediately()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var completionTask = cts.Token.WhenCancelled();

        Assert.True(completionTask.IsCompletedSuccessfully);
    }

    [Fact]
    public async Task WhenCancelledTokenNotCancelledTaskDoesNotComplete()
    {
        using var cts = new CancellationTokenSource();
        var completionTask = cts.Token.WhenCancelled();

        var timeoutTask = Task.Delay(100, TestContext.Current.CancellationToken);
        var completedTask = await Task.WhenAny(completionTask, timeoutTask);

        Assert.Equal(timeoutTask, completedTask);
        Assert.False(completionTask.IsCompleted);
    }

    [Fact]
    public async Task WhenCancelledTokenCancelledTaskCompletesWithoutException()
    {
        using var cts = new CancellationTokenSource();
        var completionTask = cts.Token.WhenCancelled();

        cts.Cancel();

        await completionTask.ConfigureAwait(false);
        Assert.True(completionTask.IsCompletedSuccessfully);
        Assert.False(completionTask.IsFaulted);
        Assert.False(completionTask.IsCanceled);
    }

    [Fact]
    public async Task WhenCancelledDisposedTokenSourceShouldNotThrow()
    {
        CancellationToken token;
        using (var cts = new CancellationTokenSource())
        {
            token = cts.Token;
        }

        var completionTask = token.WhenCancelled();

        Assert.NotNull(completionTask);

        var timeoutTask = Task.Delay(100, TestContext.Current.CancellationToken);
        var completedTask = await Task.WhenAny(completionTask, timeoutTask);
        Assert.Equal(timeoutTask, completedTask);
    }

    [Fact]
    public async Task WhenCancelledDefaultTokenTaskDoesNotComplete()
    {
        CancellationToken defaultToken = default;

        var completionTask = defaultToken.WhenCancelled();

        var timeoutTask = Task.Delay(100, TestContext.Current.CancellationToken);
        var completedTask = await Task.WhenAny(completionTask, timeoutTask);

        Assert.Equal(timeoutTask, completedTask);
        Assert.False(completionTask.IsCompleted);
    }

    [Fact]
    public async Task WhenCancelledLinkedTokenSourceTaskCompletes()
    {
        using var cts1 = new CancellationTokenSource();
        using var cts2 = new CancellationTokenSource();
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cts1.Token, cts2.Token);
        var completionTask = linkedCts.Token.WhenCancelled();

        cts1.Cancel();
        var timeoutTask = Task.Delay(100, TestContext.Current.CancellationToken);
        var completedTask = await Task.WhenAny(completionTask, timeoutTask);

        Assert.Equal(completionTask, completedTask);
        Assert.True(completionTask.IsCompletedSuccessfully);
    }
}