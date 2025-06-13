using System.Runtime.CompilerServices;

using LaquaiLib.Extensions;

namespace LaquaiLib.UnitTests.Extensions;

public class IAsyncEnumerableExtensionsTests
{
    private static async IAsyncEnumerable<int> CreateSequence(int start, int count)
    {
        for (var i = 0; i < count; i++)
        {
            yield return start + i;
        }
    }

    private static async IAsyncEnumerable<int> CreateDelayedSequence(int start, int count, int delayMs)
    {
        for (var i = 0; i < count; i++)
        {
            await Task.Delay(delayMs);
            yield return start + i;
        }
    }

    private static async IAsyncEnumerable<int> CreateThrowingSequence(int start, int count, int throwAt)
    {
        for (var i = 0; i < count; i++)
        {
            if (i == throwAt)
            {
                throw new InvalidOperationException("Test exception");
            }
            yield return start + i;
        }
    }

    private static async IAsyncEnumerable<int> CreateCancellableSequence(int start, int count, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        for (var i = 0; i < count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return start + i;
        }
    }

    [Fact]
    public async Task ConcatArrayCreatesCorrectSequence()
    {
        var first = CreateSequence(1, 3);
        var second = CreateSequence(10, 2);
        var third = CreateSequence(100, 1);

        var combined = IAsyncEnumerableExtensions.Concat([first, second, third]);
        var result = await ConvertToList(combined);

        Assert.Equal([1, 2, 3, 10, 11, 100], result);
    }

    [Fact]
    public async Task ConcatEmptySequencesWorks()
    {
        var empty1 = CreateSequence(0, 0);
        var empty2 = CreateSequence(0, 0);
        var nonEmpty = CreateSequence(1, 3);

        var combined = IAsyncEnumerableExtensions.Concat([empty1, nonEmpty, empty2]);
        var result = await ConvertToList(combined);

        Assert.Equal([1, 2, 3], result);
    }

    [Fact]
    public async Task ConcatAllEmptySequencesWorks()
    {
        var empty1 = CreateSequence(0, 0);
        var empty2 = CreateSequence(0, 0);
        var empty3 = CreateSequence(0, 0);

        var combined = IAsyncEnumerableExtensions.Concat([empty1, empty2, empty3]);
        var result = await ConvertToList(combined);

        Assert.Empty(result);
    }

    [Fact]
    public async Task ConcatWithDelaysPreservesOrder()
    {
        var fast = CreateSequence(1, 3);
        var slow = CreateDelayedSequence(10, 2, 10);
        var fast2 = CreateSequence(100, 1);

        var combined = IAsyncEnumerableExtensions.Concat([fast, slow, fast2]);
        var result = await ConvertToList(combined);

        Assert.Equal([1, 2, 3, 10, 11, 100], result);
    }

    [Fact]
    public async Task ConcatHandlesExceptionsCorrectly()
    {
        var first = CreateSequence(1, 3);
        var second = CreateThrowingSequence(10, 3, 1);
        var third = CreateSequence(100, 1);

        var combined = IAsyncEnumerableExtensions.Concat([first, second, third]);
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(async () => await ConvertToList(combined));
        Assert.Equal("Test exception", ex.Message);
    }

    [Fact]
    public async Task ConcatHandlesCancellationCorrectly()
    {
        var cts = new CancellationTokenSource();
        var first = CreateSequence(1, 3);
        var second = CreateCancellableSequence(10, 100, cts.Token);

        var combined = IAsyncEnumerableExtensions.Concat([first, second]);

        var enumerator = combined.GetAsyncEnumerator(cts.Token);

        Assert.True(await enumerator.MoveNextAsync());
        Assert.Equal(1, enumerator.Current);
        Assert.True(await enumerator.MoveNextAsync());
        Assert.Equal(2, enumerator.Current);
        Assert.True(await enumerator.MoveNextAsync());
        Assert.Equal(3, enumerator.Current);

        Assert.True(await enumerator.MoveNextAsync());
        Assert.Equal(10, enumerator.Current);

        cts.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(async () => await enumerator.MoveNextAsync());
    }

    [Fact]
    public async Task ConcatWithSingleSequenceWorks()
    {
        var single = CreateSequence(1, 3);

        var combined = IAsyncEnumerableExtensions.Concat([single]);
        var result = await ConvertToList(combined);

        Assert.Equal([1, 2, 3], result);
    }

    [Fact]
    public async Task ConcatWithEmptyArrayWorks()
    {
        var combined = IAsyncEnumerableExtensions.Concat(Array.Empty<IAsyncEnumerable<int>>());
        var result = await ConvertToList(combined);

        Assert.Empty(result);
    }

    [Fact]
    public Task ConcatWithNullArrayThrowsArgumentNullException() => Assert.ThrowsAsync<ArgumentNullException>(static async () =>
    {
        var combined = IAsyncEnumerableExtensions.Concat<int>(toChain: null);
        await ConvertToList(combined);
    });

    [Fact]
    public async Task ConcatWithNullItemInArrayThrowsArgumentNullException()
    {
        var first = CreateSequence(1, 3);
        IAsyncEnumerable<int> second = null;

        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
        {
            var combined = IAsyncEnumerableExtensions.Concat([first, second]);
            await ConvertToList(combined);
        });
    }

    [Fact]
    public async Task IteratorDisposeIsCalledForAllIterators()
    {
        var disposeCheck1 = new DisposeTrackingAsyncEnumerable<int>(CreateSequence(1, 2));
        var disposeCheck2 = new DisposeTrackingAsyncEnumerable<int>(CreateSequence(3, 2));

        var combined = IAsyncEnumerableExtensions.Concat([disposeCheck1, disposeCheck2]);

        await ConvertToList(combined);

        Assert.True(disposeCheck1.IsDisposed);
        Assert.True(disposeCheck2.IsDisposed);
    }

    [Fact]
    public async Task SecondIteratorIsNotEnumeratedIfFirstThrows()
    {
        var first = CreateThrowingSequence(1, 3, 1);
        var second = new TrackingAsyncEnumerable<int>(CreateSequence(10, 2));

        var combined = IAsyncEnumerableExtensions.Concat([first, second]);

        await Assert.ThrowsAsync<InvalidOperationException>(async () => await ConvertToList(combined));

        Assert.False(second.WasEnumerated);
    }

    private class TrackingAsyncEnumerable<T>(IAsyncEnumerable<T> inner) : IAsyncEnumerable<T>
    {
        private readonly IAsyncEnumerable<T> _inner = inner;
        public bool WasEnumerated { get; private set; }

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            WasEnumerated = true;
            return _inner.GetAsyncEnumerator(cancellationToken);
        }
    }

    private class DisposeTrackingAsyncEnumerable<T>(IAsyncEnumerable<T> inner) : IAsyncEnumerable<T>
    {
        private readonly IAsyncEnumerable<T> _inner = inner;
        public bool IsDisposed { get; private set; }

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) => new DisposeTrackingEnumerator(this, _inner.GetAsyncEnumerator(cancellationToken));

        private class DisposeTrackingEnumerator(IAsyncEnumerableExtensionsTests.DisposeTrackingAsyncEnumerable<T> parent, IAsyncEnumerator<T> innerEnumerator) : IAsyncEnumerator<T>
        {
            private readonly DisposeTrackingAsyncEnumerable<T> _parent = parent;
            private readonly IAsyncEnumerator<T> _innerEnumerator = innerEnumerator;

            public T Current => _innerEnumerator.Current;

            public ValueTask DisposeAsync()
            {
                _parent.IsDisposed = true;
                return _innerEnumerator.DisposeAsync();
            }

            public ValueTask<bool> MoveNextAsync() => _innerEnumerator.MoveNextAsync();
        }
    }

    private static async Task<List<T>> ConvertToList<T>(IAsyncEnumerable<T> source)
    {
        var result = new List<T>();
        await foreach (var item in source)
        {
            result.Add(item);
        }
        return result;
    }
}
