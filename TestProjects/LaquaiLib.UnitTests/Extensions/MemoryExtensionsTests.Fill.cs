using LaquaiLib.Collections.Enumeration;
using LaquaiLib.Extensions;

namespace LaquaiLib.UnitTests.Extensions;

public class MemoryExtensionsFillTests
{
    private static T[] Flatten<T>(Array array) => array.Cast<T>().ToArray();

    [Fact]
    public void SpanZeroMemoryClearsAllElements()
    {
        var array = new[] { 1, 2, 3, 4, 5 };

        array.AsSpan().ZeroMemory();

        Assert.Equal(new int[5], array);
    }

    [Fact]
    public void SpanZeroMemoryNullsReferences()
    {
        var array = new[] { "a", "b", "c" };

        array.AsSpan().ZeroMemory();

        Assert.All(array, static s => Assert.Null(s));
    }

    [Fact]
    public void SpanZeroMemoryOnEmptySpanDoesNothing()
    {
        var array = Array.Empty<int>();

        array.AsSpan().ZeroMemory();

        Assert.Empty(array);
    }

    [Fact]
    public void SpanZeroMemoryOnSliceLeavesRemainderIntact()
    {
        var array = new[] { 1, 2, 3, 4, 5 };

        array.AsSpan(1, 3).ZeroMemory();

        Assert.Equal([1, 0, 0, 0, 5], array);
    }

    [Fact]
    public void SpanFillDefaultResetsValueTypes()
    {
        var array = new[] { 1L, 2L, 3L };

        array.AsSpan().Fill();

        Assert.Equal([0L, 0L, 0L], array);
    }

    [Fact]
    public void SpanFillDefaultNullsReferenceTypes()
    {
        var array = new[] { "a", "b" };

        array.AsSpan().Fill();

        Assert.All(array, static s => Assert.Null(s));
    }

    [Fact]
    public void SpanFillDefaultResetsStructsWithNonZeroFields()
    {
        var array = new[] { new KeyValuePair<int, string>(1, "a"), new KeyValuePair<int, string>(2, "b") };

        array.AsSpan().Fill();

        Assert.All(array, static kvp =>
        {
            Assert.Equal(0, kvp.Key);
            Assert.Null(kvp.Value);
        });
    }

    [Fact]
    public void SpanFillDefaultOnEmptySpanDoesNothing()
    {
        var array = Array.Empty<long>();

        array.AsSpan().Fill();

        Assert.Empty(array);
    }

    [Fact]
    public void SpanFillDefaultOnSliceLeavesRemainderIntact()
    {
        var array = new[] { 1L, 2L, 3L, 4L };

        array.AsSpan(2).Fill();

        Assert.Equal([1L, 2L, 0L, 0L], array);
    }

    [Fact]
    public void SpanFillWithFactoryAssignsProducedValues()
    {
        var array = new long[4];
        var next = 10L;

        array.AsSpan().Fill(() => next++);

        Assert.Equal([10L, 11L, 12L, 13L], array);
    }

    [Fact]
    public void SpanFillWithFactoryInvokesFactoryOncePerElement()
    {
        var array = new long[6];
        var calls = 0;

        array.AsSpan().Fill(() =>
        {
            calls++;
            return 7L;
        });

        Assert.Equal(6, calls);
        Assert.All(array, static v => Assert.Equal(7L, v));
    }

    [Fact]
    public void SpanFillWithFactoryOnEmptySpanNeverInvokesFactory()
    {
        var array = Array.Empty<long>();
        var calls = 0;

        array.AsSpan().Fill(() =>
        {
            calls++;
            return 1L;
        });

        Assert.Equal(0, calls);
    }

    [Fact]
    public void SpanFillWithPreviousValueFactoryStartsWithDefault()
    {
        var array = new long[3];
        var seen = new List<long>();

        array.AsSpan().Fill(prev =>
        {
            seen.Add(prev);
            return prev + 5;
        });

        Assert.Equal([0L, 5L, 10L], seen);
        Assert.Equal([5L, 10L, 15L], array);
    }

    [Fact]
    public void SpanFillWithPreviousValueFactoryStartsWithNullForReferenceTypes()
    {
        var array = new string[3];

        array.AsSpan().Fill(prev => prev is null ? "a" : prev + "a");

        Assert.Equal(["a", "aa", "aaa"], array);
    }

    [Fact]
    public void SpanFillWithPreviousValueFactoryIgnoresPreExistingContent()
    {
        var array = new[] { 100L, 200L, 300L };

        array.AsSpan().Fill(prev => prev + 1);

        Assert.Equal([1L, 2L, 3L], array);
    }

    [Fact]
    public void SpanFillWithPreviousValueFactoryResolvesForInt()
    {
        var array = new int[3];

        array.AsSpan().Fill(prev => prev + 1);

        Assert.Equal([1, 2, 3], array);
    }

    [Fact]
    public void SpanFillWithPreviousValueFactoryOnEmptySpanNeverInvokesFactory()
    {
        var array = Array.Empty<long>();
        var calls = 0;

        array.AsSpan().Fill(prev =>
        {
            calls++;
            return prev;
        });

        Assert.Equal(0, calls);
    }

    [Fact]
    public void SpanFillIndexedPassesSequentialIndices()
    {
        var array = new long[5];

        array.AsSpan().FillIndexed(i => i * 2L);

        Assert.Equal([0L, 2L, 4L, 6L, 8L], array);
    }

    [Fact]
    public void SpanFillIndexedResolvesForInt()
    {
        var array = new int[4];

        array.AsSpan().FillIndexed(i => i * 2);

        Assert.Equal([0, 2, 4, 6], array);
    }

    [Fact]
    public void SpanFillIndexedIndicesAreRelativeToSlice()
    {
        var array = new long[6];

        array.AsSpan(2, 3).FillIndexed(i => i + 1L);

        Assert.Equal([0L, 0L, 1L, 2L, 3L, 0L], array);
    }

    [Fact]
    public void SpanFillIndexedOnEmptySpanNeverInvokesFactory()
    {
        var array = Array.Empty<long>();
        var calls = 0;

        array.AsSpan().FillIndexed(i =>
        {
            calls++;
            return (long)i;
        });

        Assert.Equal(0, calls);
    }

    [Fact]
    public void SpanFillIndexedWithPreviousValuePassesBoth()
    {
        var array = new long[4];
        var indices = new List<int>();
        var previous = new List<long>();

        array.AsSpan().FillIndexed((i, prev) =>
        {
            indices.Add(i);
            previous.Add(prev);
            return prev + i;
        });

        Assert.Equal([0, 1, 2, 3], indices);
        Assert.Equal([0L, 0L, 1L, 3L], previous);
        Assert.Equal([0L, 1L, 3L, 6L], array);
    }

    [Fact]
    public void SpanFillIndexedWithPreviousValueResolvesForInt()
    {
        var array = new int[4];

        array.AsSpan().FillIndexed((i, prev) => prev + i + 1);

        Assert.Equal([1, 3, 6, 10], array);
    }

    [Fact]
    public void SpanFillIndexedWithPreviousValueOnEmptySpanNeverInvokesFactory()
    {
        var array = Array.Empty<long>();
        var calls = 0;

        array.AsSpan().FillIndexed((i, prev) =>
        {
            calls++;
            return prev;
        });

        Assert.Equal(0, calls);
    }

    [Fact]
    public void SpanFillPropagatesFactoryExceptionLeavingEarlierElementsAssigned()
    {
        var array = new long[3];

        Assert.Throws<InvalidOperationException>(() =>
        {
            var span = array.AsSpan();
            span.FillIndexed(i => i == 1 ? throw new InvalidOperationException() : 5L);
        });

        Assert.Equal([5L, 0L, 0L], array);
    }

    [Fact]
    public void MultiDimArrayEnumerableZeroMemoryClearsEntireArray()
    {
        var array = new long[2, 3] { { 1, 2, 3 }, { 4, 5, 6 } };

        using (var enumerable = new MultiDimArrayEnumerable<long>(array))
        {
            enumerable.ZeroMemory();
        }

        Assert.Equal(new long[6], Flatten<long>(array));
    }

    [Fact]
    public void MultiDimArrayEnumerableFillDefaultResetsAllElements()
    {
        var array = new long[2, 2] { { 1, 2 }, { 3, 4 } };

        using (var enumerable = new MultiDimArrayEnumerable<long>(array))
        {
            enumerable.Fill();
        }

        Assert.Equal(new long[4], Flatten<long>(array));
    }

    [Fact]
    public void MultiDimArrayEnumerableFillWithFactoryAssignsProducedValues()
    {
        var array = new long[2, 2];
        var next = 1L;

        using (var enumerable = new MultiDimArrayEnumerable<long>(array))
        {
            enumerable.Fill(() => next++);
        }

        Assert.Equal([1L, 2L, 3L, 4L], Flatten<long>(array));
        Assert.Equal(1L, array[0, 0]);
        Assert.Equal(4L, array[1, 1]);
    }

    [Fact]
    public void MultiDimArrayEnumerableFillWithPreviousValueFactoryChainsValues()
    {
        var array = new long[2, 2];

        using (var enumerable = new MultiDimArrayEnumerable<long>(array))
        {
            enumerable.Fill(prev => prev + 3);
        }

        Assert.Equal([3L, 6L, 9L, 12L], Flatten<long>(array));
    }

    [Fact]
    public void MultiDimArrayEnumerableFillResolvesForInt()
    {
        var array = new int[2, 2];

        using (var enumerable = new MultiDimArrayEnumerable<int>(array))
        {
            enumerable.Fill(prev => prev + 1);
        }

        Assert.Equal([1, 2, 3, 4], Flatten<int>(array));
    }

    [Fact]
    public void MultiDimArrayEnumerableFillIndexedUsesFlattenedIndices()
    {
        var array = new long[2, 3];

        using (var enumerable = new MultiDimArrayEnumerable<long>(array))
        {
            enumerable.FillIndexed(i => (long)i);
        }

        Assert.Equal([0L, 1L, 2L, 3L, 4L, 5L], Flatten<long>(array));
        Assert.Equal(3L, array[1, 0]);
    }

    [Fact]
    public void MultiDimArrayEnumerableFillIndexedWithPreviousValuePassesBoth()
    {
        var array = new long[3, 1];

        using (var enumerable = new MultiDimArrayEnumerable<long>(array))
        {
            enumerable.FillIndexed((i, prev) => prev + i + 1);
        }

        Assert.Equal([1L, 3L, 6L], Flatten<long>(array));
    }

    [Fact]
    public void MultiDimArrayEnumerableFillIndexedCoversThreeDimensionalArray()
    {
        var array = new long[2, 2, 2];

        using (var enumerable = new MultiDimArrayEnumerable<long>(array))
        {
            enumerable.FillIndexed(i => i + 1L);
        }

        Assert.Equal([1L, 2L, 3L, 4L, 5L, 6L, 7L, 8L], Flatten<long>(array));
        Assert.Equal(1L, array[0, 0, 0]);
        Assert.Equal(8L, array[1, 1, 1]);
    }

    [Fact]
    public void MultiDimArrayEnumerableFillWorksWithSingleDimensionalArray()
    {
        var array = new long[3];

        using (var enumerable = new MultiDimArrayEnumerable<long>(array))
        {
            enumerable.Fill(() => 9L);
        }

        Assert.Equal([9L, 9L, 9L], array);
    }

    [Fact]
    public void MultiDimArrayEnumerableFillOnEmptyArrayNeverInvokesFactory()
    {
        var array = new long[0, 3];
        var calls = 0;

        using (var enumerable = new MultiDimArrayEnumerable<long>(array))
        {
            enumerable.Fill(() =>
            {
                calls++;
                return 1L;
            });
        }

        Assert.Equal(0, calls);
    }

    [Fact]
    public void MemoryZeroMemoryClearsAllElements()
    {
        var array = new[] { 1, 2, 3 };

        array.AsMemory().ZeroMemory();

        Assert.Equal([0, 0, 0], array);
    }

    [Fact]
    public void MemoryZeroMemoryNullsReferences()
    {
        var array = new[] { "a", "b" };

        array.AsMemory().ZeroMemory();

        Assert.All(array, static s => Assert.Null(s));
    }

    [Fact]
    public void MemoryZeroMemoryOnSliceLeavesRemainderIntact()
    {
        var array = new[] { 1, 2, 3, 4 };

        array.AsMemory(1, 2).ZeroMemory();

        Assert.Equal([1, 0, 0, 4], array);
    }

    [Fact]
    public void MemoryFillDefaultResetsAllElements()
    {
        var array = new[] { 1L, 2L, 3L };

        array.AsMemory().Fill();

        Assert.Equal([0L, 0L, 0L], array);
    }

    [Fact]
    public void MemoryFillDefaultOnEmptyMemoryDoesNothing()
    {
        var array = Array.Empty<long>();

        array.AsMemory().Fill();

        Assert.Empty(array);
    }

    [Fact]
    public void MemoryFillWithFactoryAssignsProducedValues()
    {
        var array = new long[4];
        var next = 10L;

        array.AsMemory().Fill(() => next++);

        Assert.Equal([10L, 11L, 12L, 13L], array);
    }

    [Fact]
    public void MemoryFillWithFactoryInvokesFactoryOncePerElement()
    {
        var array = new long[5];
        var calls = 0;

        array.AsMemory().Fill(() =>
        {
            calls++;
            return 2L;
        });

        Assert.Equal(5, calls);
        Assert.All(array, static v => Assert.Equal(2L, v));
    }

    [Fact]
    public void MemoryFillWithPreviousValueFactoryChainsValues()
    {
        var array = new long[3];

        array.AsMemory().Fill(prev => prev + 4);

        Assert.Equal([4L, 8L, 12L], array);
    }

    [Fact]
    public void MemoryFillWithPreviousValueFactoryResolvesForInt()
    {
        var array = new int[3];

        array.AsMemory().Fill(prev => prev + 4);

        Assert.Equal([4, 8, 12], array);
    }

    [Fact]
    public void MemoryFillIndexedPassesSequentialIndices()
    {
        var array = new long[4];

        array.AsMemory().FillIndexed(i => i * 3L);

        Assert.Equal([0L, 3L, 6L, 9L], array);
    }

    [Fact]
    public void MemoryFillIndexedResolvesForInt()
    {
        var array = new int[4];

        array.AsMemory().FillIndexed(i => i * 3);

        Assert.Equal([0, 3, 6, 9], array);
    }

    [Fact]
    public void MemoryFillIndexedWithPreviousValuePassesBoth()
    {
        var array = new long[4];

        array.AsMemory().FillIndexed((i, prev) => prev + i + 1);

        Assert.Equal([1L, 3L, 6L, 10L], array);
    }

    [Fact]
    public void MemoryFillIndexedOnSliceAffectsOnlySlice()
    {
        var array = new[] { 9L, 9L, 9L, 9L, 9L };

        array.AsMemory(1, 3).FillIndexed(i => i + 1L);

        Assert.Equal([9L, 1L, 2L, 3L, 9L], array);
    }

    [Fact]
    public async Task MemoryFillAsyncWithFactoryAssignsProducedValues()
    {
        var array = new long[4];
        var next = 1L;

        await array.AsMemory().FillAsync(() => ValueTask.FromResult(next++));

        Assert.Equal([1L, 2L, 3L, 4L], array);
    }

    [Fact]
    public async Task MemoryFillAsyncWithFactoryInvokesFactoryOncePerElement()
    {
        var array = new long[5];
        var calls = 0;

        await array.AsMemory().FillAsync(() =>
        {
            calls++;
            return ValueTask.FromResult(6L);
        });

        Assert.Equal(5, calls);
        Assert.All(array, static v => Assert.Equal(6L, v));
    }

    [Fact]
    public async Task MemoryFillAsyncWithPreviousValueFactoryChainsValues()
    {
        var array = new long[3];

        await array.AsMemory().FillAsync(prev => ValueTask.FromResult(prev + 2));

        Assert.Equal([2L, 4L, 6L], array);
    }

    [Fact]
    public async Task MemoryFillAsyncWithPreviousValueFactoryResolvesForInt()
    {
        var array = new int[3];

        await array.AsMemory().FillAsync(prev => ValueTask.FromResult(prev + 2));

        Assert.Equal([2, 4, 6], array);
    }

    [Fact]
    public async Task MemoryFillAsyncWithPreviousValueFactoryStartsWithNullForReferenceTypes()
    {
        var array = new string[3];

        await array.AsMemory().FillAsync(prev => ValueTask.FromResult(prev is null ? "a" : prev + "a"));

        Assert.Equal(["a", "aa", "aaa"], array);
    }

    [Fact]
    public async Task MemoryFillIndexedAsyncPassesSequentialIndices()
    {
        var array = new long[4];

        await array.AsMemory().FillIndexedAsync(i => ValueTask.FromResult(i * 3L));

        Assert.Equal([0L, 3L, 6L, 9L], array);
    }

    [Fact]
    public async Task MemoryFillIndexedAsyncResolvesForInt()
    {
        var array = new int[4];

        await array.AsMemory().FillIndexedAsync(i => ValueTask.FromResult(i * 3));

        Assert.Equal([0, 3, 6, 9], array);
    }

    [Fact]
    public async Task MemoryFillIndexedAsyncWithPreviousValuePassesBoth()
    {
        var array = new long[4];
        var indices = new List<int>();
        var previous = new List<long>();

        await array.AsMemory().FillIndexedAsync((i, prev) =>
        {
            indices.Add(i);
            previous.Add(prev);
            return ValueTask.FromResult(prev + i + 1);
        });

        Assert.Equal([0, 1, 2, 3], indices);
        Assert.Equal([0L, 1L, 3L, 6L], previous);
        Assert.Equal([1L, 3L, 6L, 10L], array);
    }

    [Fact]
    public async Task MemoryFillIndexedAsyncSurvivesSuspensionAcrossAwaits()
    {
        var array = new long[4];

        await array.AsMemory().FillIndexedAsync(async i =>
        {
            await Task.Yield();
            return i + 1L;
        });

        Assert.Equal([1L, 2L, 3L, 4L], array);
    }

    [Fact]
    public async Task MemoryFillIndexedAsyncAwaitsFactoriesSequentially()
    {
        var array = new long[4];
        var inFlight = 0;
        var maxInFlight = 0;

        await array.AsMemory().FillIndexedAsync(async i =>
        {
            maxInFlight = Math.Max(maxInFlight, Interlocked.Increment(ref inFlight));
            await Task.Yield();
            Interlocked.Decrement(ref inFlight);
            return i + 1L;
        });

        Assert.Equal(1, maxInFlight);
        Assert.Equal([1L, 2L, 3L, 4L], array);
    }

    [Fact]
    public async Task MemoryFillAsyncOnEmptyMemoryNeverInvokesFactory()
    {
        var calls = 0;

        await Memory<long>.Empty.FillAsync(() =>
        {
            calls++;
            return ValueTask.FromResult(1L);
        });

        Assert.Equal(0, calls);
    }

    [Fact]
    public async Task MemoryFillIndexedAsyncOnSliceAffectsOnlySlice()
    {
        var array = new[] { 9L, 9L, 9L, 9L, 9L };

        await array.AsMemory(1, 3).FillIndexedAsync(i => ValueTask.FromResult(i + 1L));

        Assert.Equal([9L, 1L, 2L, 3L, 9L], array);
    }

    [Fact]
    public async Task MemoryFillIndexedAsyncPropagatesFactoryExceptionLeavingEarlierElementsAssigned()
    {
        var array = new long[3];
        var memory = array.AsMemory();

        await Assert.ThrowsAsync<InvalidOperationException>(() => memory.FillIndexedAsync(i => i == 1 ? throw new InvalidOperationException() : ValueTask.FromResult(5L)));

        Assert.Equal([5L, 0L, 0L], array);
    }
}
