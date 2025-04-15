using LaquaiLib.Extensions;

namespace LaquaiLib.UnitTests.Extensions;

public class IAsyncEnumeratorExtensionsTests
{
    [Fact]
    public async Task ChainMultipleEnumeratorsWorks()
    {
        var first = CreateEnumerator(1, 3);
        var second = CreateEnumerator(10, 2);
        var third = CreateEnumerator(100, 1);

        var combined = IAsyncEnumeratorExtensions.Chain([first, second, third]);
        var result = await CollectResults(combined);

        Assert.Equal([1, 2, 3, 10, 11, 100], result);
    }

    [Fact]
    public async Task ChainWithEmptyEnumeratorsSkipsThem()
    {
        var empty1 = CreateEnumerator(0, 0);
        var empty2 = CreateEnumerator(0, 0);
        var nonEmpty = CreateEnumerator(1, 3);

        var combined = IAsyncEnumeratorExtensions.Chain([empty1, nonEmpty, empty2]);
        var result = await CollectResults(combined);

        Assert.Equal([1, 2, 3], result);
    }

    [Fact]
    public async Task ChainWithAllEmptyEnumeratorsReturnsEmpty()
    {
        var empty1 = CreateEnumerator(0, 0);
        var empty2 = CreateEnumerator(0, 0);

        var combined = IAsyncEnumeratorExtensions.Chain([empty1, empty2]);
        var result = await CollectResults(combined);

        Assert.Empty(result);
    }

    [Fact]
    public async Task ChainWithEmptyArrayReturnsEmptyEnumerator()
    {
        var combined = IAsyncEnumeratorExtensions.Chain(Array.Empty<IAsyncEnumerator<int>>());
        var result = await CollectResults(combined);

        Assert.Empty(result);
    }

    [Fact]
    public async Task ChainWithNullArrayThrowsArgumentNullException() => await Assert.ThrowsAsync<ArgumentNullException>(async () =>
    {
        var combined = IAsyncEnumeratorExtensions.Chain<int>(toChain: null);
        await CollectResults(combined);
    });

    [Fact]
    public async Task ChainWithNullItemInArrayThrowsArgumentNullException()
    {
        var first = CreateEnumerator(1, 3);
        IAsyncEnumerator<int> second = null;

        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
        {
            var combined = IAsyncEnumeratorExtensions.Chain([first, second]);
            await CollectResults(combined);
        });
    }

    [Fact]
    public async Task ChainExtensionWithMultipleEnumeratorsWorks()
    {
        var first = CreateEnumerator(1, 3);
        var second = CreateEnumerator(10, 2);
        var third = CreateEnumerator(100, 1);

        var combined = first.Chain(second, third);
        var result = await CollectResults(combined);

        Assert.Equal([1, 2, 3, 10, 11, 100], result);
    }

    [Fact]
    public async Task ChainExtensionWithSourceAsNullThrowsArgumentNullException()
    {
        IAsyncEnumerator<int> source = null;
        var with = CreateEnumerator(1, 3);

        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
        {
            var combined = source.Chain(with);
            await CollectResults(combined);
        });
    }

    [Fact]
    public async Task ChainExtensionWithNullItemInWithThrowsArgumentNullException()
    {
        var source = CreateEnumerator(1, 3);
        IAsyncEnumerator<int> with = null;

        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
        {
            var combined = source.Chain(with);
            await CollectResults(combined);
        });
    }

    [Fact]
    public async Task ChainExtensionWithSourceAsCombinerReusesIt()
    {
        var first = CreateEnumerator(1, 3);
        var second = CreateEnumerator(10, 2);
        var third = CreateEnumerator(100, 1);

        var initialCombined = IAsyncEnumeratorExtensions.Chain([first, second]);
        var finalCombined = initialCombined.Chain(third);

        var result = await CollectResults(finalCombined);

        Assert.Equal([1, 2, 3, 10, 11, 100], result);
    }

    [Fact]
    public async Task AsyncEnumeratorCombinerCurrentPropertyWorks()
    {
        var first = CreateEnumerator(42, 1);
        var combined = IAsyncEnumeratorExtensions.Chain([first]);

        await combined.MoveNextAsync();
        Assert.Equal(42, combined.Current);
    }

    [Fact]
    public async Task AsyncEnumeratorCombinerDisposeAsyncDisposesAllEnumerators()
    {
        var tracker1 = new DisposeTrackingEnumerator<int>(CreateEnumerator(1, 2));
        var tracker2 = new DisposeTrackingEnumerator<int>(CreateEnumerator(3, 2));

        var combined = IAsyncEnumeratorExtensions.Chain([tracker1, tracker2]);
        await combined.DisposeAsync();

        Assert.True(tracker1.IsDisposed);
        Assert.True(tracker2.IsDisposed);
    }

    [Fact]
    public async Task AsyncEnumeratorCombinerDisposeAsyncDisposesAllEnumeratorsEvenIfSomeThrow()
    {
        var tracker1 = new DisposeTrackingEnumerator<int>(CreateEnumerator(1, 2));
        var tracker2 = new ThrowOnDisposeEnumerator<int>(CreateEnumerator(3, 2));
        var tracker3 = new DisposeTrackingEnumerator<int>(CreateEnumerator(5, 2));

        var combined = IAsyncEnumeratorExtensions.Chain([tracker1, tracker2, tracker3]);

        await Assert.ThrowsAsync<AggregateException>(async () => await combined.DisposeAsync());

        Assert.True(tracker1.IsDisposed);
        Assert.True(tracker3.IsDisposed);
    }

    [Fact]
    public async Task EnumeratorIsExhaustedBeforeMovingToNext()
    {
        var first = CreateEnumerator(1, 2);
        var second = CreateEnumerator(10, 2);

        var combined = IAsyncEnumeratorExtensions.Chain([first, second]);

        Assert.True(await combined.MoveNextAsync());
        Assert.Equal(1, combined.Current);

        Assert.True(await combined.MoveNextAsync());
        Assert.Equal(2, combined.Current);

        Assert.True(await combined.MoveNextAsync());
        Assert.Equal(10, combined.Current);

        Assert.True(await combined.MoveNextAsync());
        Assert.Equal(11, combined.Current);

        Assert.False(await combined.MoveNextAsync());
    }

    [Fact]
    public async Task AddIteratorsWorksCorrectly()
    {
        var first = CreateEnumerator(1, 2);

        var second = CreateEnumerator(10, 1);
        var third = CreateEnumerator(100, 1);

        var combined = first.Chain(second).Chain(third);

        var result = await CollectResults(combined);
        Assert.Equal([1, 2, 10, 100], result);
    }

    private static async Task<List<T>> CollectResults<T>(IAsyncEnumerator<T> enumerator)
    {
        var results = new List<T>();
        try
        {
            while (await enumerator.MoveNextAsync())
            {
                results.Add(enumerator.Current);
            }
        }
        finally
        {
            await enumerator.DisposeAsync();
        }
        return results;
    }

    private static IAsyncEnumerator<int> CreateEnumerator(int start, int count) => new TestAsyncEnumerator<int>(Enumerable.Range(start, count));

    private class TestAsyncEnumerator<T>(IEnumerable<T> source) : IAsyncEnumerator<T>
    {
        private readonly IEnumerator<T> _enumerator = source.GetEnumerator();

        public T Current => _enumerator.Current;

        public ValueTask DisposeAsync()
        {
            _enumerator.Dispose();
            return ValueTask.CompletedTask;
        }

        public ValueTask<bool> MoveNextAsync() => new ValueTask<bool>(_enumerator.MoveNext());
    }

    private class DisposeTrackingEnumerator<T>(IAsyncEnumerator<T> inner) : IAsyncEnumerator<T>
    {
        private readonly IAsyncEnumerator<T> _inner = inner;
        public bool IsDisposed { get; private set; }

        public T Current => _inner.Current;

        public ValueTask DisposeAsync()
        {
            IsDisposed = true;
            return _inner.DisposeAsync();
        }

        public ValueTask<bool> MoveNextAsync() => _inner.MoveNextAsync();
    }

    private class ThrowOnDisposeEnumerator<T>(IAsyncEnumerator<T> inner) : IAsyncEnumerator<T>
    {
        private readonly IAsyncEnumerator<T> _inner = inner;

        public T Current => _inner.Current;

        public ValueTask DisposeAsync()
        {
            _inner.DisposeAsync();
            throw new InvalidOperationException("Test exception on dispose");
        }

        public ValueTask<bool> MoveNextAsync() => _inner.MoveNextAsync();
    }
}
