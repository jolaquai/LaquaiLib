using LaquaiLib.Collections.Enumeration;
using LaquaiLib.Extensions;

namespace LaquaiLib.UnitTests.Extensions;

public class MemoryExtensionsFillTests
{
    private static T[] Flatten<T>(Array array) => [.. array.Cast<T>()];

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
}
