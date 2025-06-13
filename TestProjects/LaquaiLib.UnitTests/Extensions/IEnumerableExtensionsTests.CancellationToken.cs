using LaquaiLib.Extensions;

namespace LaquaiLib.UnitTests.Extensions;

public class IEnumerableExtensionsCancellationTokenTests
{
    [Fact]
    public async Task WhenAnyCancelledCompletesWhenAnyTokenIsCancelled()
    {
        var cts1 = new CancellationTokenSource();
        var cts2 = new CancellationTokenSource();
        var cts3 = new CancellationTokenSource();

        var tokens = new[] { cts1.Token, cts2.Token, cts3.Token };

        var task = tokens.WhenAnyCancelled();

        Assert.False(task.IsCompleted);

        cts2.Cancel();

        var cancelledToken = await task;

        Assert.True(cancelledToken.IsCancellationRequested);
        Assert.Equal(cts2.Token, cancelledToken);
    }

    [Fact]
    public async Task WhenAnyCancelledReturnsAlreadyCancelledToken()
    {
        var cts1 = new CancellationTokenSource();
        var cts2 = new CancellationTokenSource();
        cts2.Cancel();
        var cts3 = new CancellationTokenSource();

        var tokens = new[] { cts1.Token, cts2.Token, cts3.Token };

        var task = tokens.WhenAnyCancelled();

        Assert.True(task.IsCompleted);

        var cancelledToken = await task;

        Assert.Equal(cts2.Token, cancelledToken);
    }

    [Fact]
    public async Task WhenAnyCancelledHandlesMultipleCancellations()
    {
        var cts1 = new CancellationTokenSource();
        var cts2 = new CancellationTokenSource();
        var cts3 = new CancellationTokenSource();

        var tokens = new[] { cts1.Token, cts2.Token, cts3.Token };

        var task = tokens.WhenAnyCancelled();

        cts1.Cancel();
        cts3.Cancel();

        var cancelledToken = await task;

        Assert.True(cancelledToken.IsCancellationRequested);
        Assert.Equal(cts1.Token, cancelledToken);
    }

    [Fact]
    public async Task WhenAnyCancelledWithEmptyCollectionDoesNotComplete()
    {
        var tokens = Array.Empty<CancellationToken>();

        var task = tokens.WhenAnyCancelled();

        var timeoutTask = Task.Delay(100, TestContext.Current.CancellationToken);
        var completedTask = await Task.WhenAny(task, timeoutTask);

        Assert.Equal(timeoutTask, completedTask);
        Assert.False(task.IsCompleted);
    }

    [Fact]
    public async Task WhenAllCancelledCompletesWhenAllTokensAreCancelled()
    {
        var cts1 = new CancellationTokenSource();
        var cts2 = new CancellationTokenSource();
        var cts3 = new CancellationTokenSource();

        var tokens = new[] { cts1.Token, cts2.Token, cts3.Token };

        var task = tokens.WhenAllCancelled();

        Assert.False(task.IsCompleted);

        cts1.Cancel();
        cts2.Cancel();

        Assert.False(task.IsCompleted);

        cts3.Cancel();

        await task;

        Assert.True(task.IsCompleted);
    }

    [Fact]
    public async Task WhenAllCancelledCompletesImmediatelyWhenAllTokensAreAlreadyCancelled()
    {
        var cts1 = new CancellationTokenSource();
        var cts2 = new CancellationTokenSource();
        var cts3 = new CancellationTokenSource();

        cts1.Cancel();
        cts2.Cancel();
        cts3.Cancel();

        var tokens = new[] { cts1.Token, cts2.Token, cts3.Token };

        var task = tokens.WhenAllCancelled();

        Assert.True(task.IsCompleted);
        await task;
    }

    [Fact]
    public async Task WhenAllCancelledHandlesEmptyCollection()
    {
        var tokens = Array.Empty<CancellationToken>();

        var task = tokens.WhenAllCancelled();

        Assert.True(task.IsCompleted);
        await task;
    }

    [Fact]
    public void ThrowIfAnyCancelledThrowsWhenAnyTokenIsCancelled()
    {
        var cts1 = new CancellationTokenSource();
        var cts2 = new CancellationTokenSource();
        var cts3 = new CancellationTokenSource();

        cts2.Cancel();

        var tokens = new[] { cts1.Token, cts2.Token, cts3.Token };

        Assert.Throws<OperationCanceledException>(tokens.ThrowIfAnyCancelled);
    }

    [Fact]
    public void ThrowIfAnyCancelledDoesNotThrowWhenNoTokensAreCancelled()
    {
        var cts1 = new CancellationTokenSource();
        var cts2 = new CancellationTokenSource();
        var cts3 = new CancellationTokenSource();

        var tokens = new[] { cts1.Token, cts2.Token, cts3.Token };

        tokens.ThrowIfAnyCancelled();
    }

    [Fact]
    public void ThrowIfAnyCancelledHandlesEmptyCollection()
    {
        var tokens = Array.Empty<CancellationToken>();

        tokens.ThrowIfAnyCancelled();
    }

    [Fact]
    public void ThrowIfAllCancelledThrowsWhenAllTokensAreCancelled()
    {
        var cts1 = new CancellationTokenSource();
        var cts2 = new CancellationTokenSource();
        var cts3 = new CancellationTokenSource();

        cts1.Cancel();
        cts2.Cancel();
        cts3.Cancel();

        var tokens = new[] { cts1.Token, cts2.Token, cts3.Token };

        Assert.Throws<OperationCanceledException>(tokens.ThrowIfAllCancelled);
    }

    [Fact]
    public void ThrowIfAllCancelledDoesNotThrowWhenNotAllTokensAreCancelled()
    {
        var cts1 = new CancellationTokenSource();
        var cts2 = new CancellationTokenSource();
        var cts3 = new CancellationTokenSource();

        cts1.Cancel();
        cts2.Cancel();

        var tokens = new[] { cts1.Token, cts2.Token, cts3.Token };

        tokens.ThrowIfAllCancelled();
    }

    [Fact]
    public void ThrowIfAllCancelledHandlesEmptyCollection()
    {
        var tokens = Array.Empty<CancellationToken>();

        tokens.ThrowIfAllCancelled();
    }

    [Fact]
    public void ThrowIfAllCancelledDoesNotThrowWhenNoneAreCancelled()
    {
        var cts1 = new CancellationTokenSource();
        var cts2 = new CancellationTokenSource();
        var cts3 = new CancellationTokenSource();

        var tokens = new[] { cts1.Token, cts2.Token, cts3.Token };

        tokens.ThrowIfAllCancelled();
    }

    [Fact]
    public async Task WhenAnyCancelledHandlesCustomCollection()
    {
        var cts1 = new CancellationTokenSource();
        var cts2 = new CancellationTokenSource();

        var customCollection = new CustomTokenCollection([cts1.Token, cts2.Token]);

        var task = customCollection.WhenAnyCancelled();

        cts1.Cancel();

        var cancelledToken = await task;

        Assert.Equal(cts1.Token, cancelledToken);
    }

    [Fact]
    public async Task WhenAllCancelledHandlesCustomCollection()
    {
        var cts1 = new CancellationTokenSource();
        var cts2 = new CancellationTokenSource();

        var customCollection = new CustomTokenCollection([cts1.Token, cts2.Token]);

        var task = customCollection.WhenAllCancelled();

        cts1.Cancel();
        cts2.Cancel();

        await task;
        Assert.True(task.IsCompleted);
    }

    [Fact]
    public async Task WhenAllCancelledWithSomeTokensAlreadyCancelled()
    {
        var cts1 = new CancellationTokenSource();
        var cts2 = new CancellationTokenSource();
        var cts3 = new CancellationTokenSource();

        cts1.Cancel();

        var tokens = new[] { cts1.Token, cts2.Token, cts3.Token };

        var task = tokens.WhenAllCancelled();

        Assert.False(task.IsCompleted);

        cts2.Cancel();
        cts3.Cancel();

        await task;
        Assert.True(task.IsCompleted);
    }

    [Fact]
    public void ThrowIfAnyCancelledWithMixedTokenStates()
    {
        var cts1 = new CancellationTokenSource();
        var cts2 = new CancellationTokenSource();
        var cts3 = new CancellationTokenSource();

        cts3.Cancel();

        var tokens = new[] { cts1.Token, cts2.Token, cts3.Token };

        Assert.Throws<OperationCanceledException>(tokens.ThrowIfAnyCancelled);
    }

    private class CustomTokenCollection(CancellationToken[] tokens) : IEnumerable<CancellationToken>
    {
        private readonly CancellationToken[] _tokens = tokens;

        public IEnumerator<CancellationToken> GetEnumerator() => ((IEnumerable<CancellationToken>)_tokens).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _tokens.GetEnumerator();
    }
}