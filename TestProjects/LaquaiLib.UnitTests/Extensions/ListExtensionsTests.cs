using LaquaiLib.Extensions;

namespace LaquaiLib.UnitTests.Extensions;

public class ListExtensionsTests
{
    [Fact]
    public void RemoveAtRemovesElementAtSpecifiedIndex()
    {
        var list = new List<int> { 1, 2, 3, 4, 5 };

        list.RemoveAt(2);

        Assert.Equal([1, 2, 4, 5], list);
    }

    [Fact]
    public void RemoveAtWithIndexFromEndRemovesCorrectElement()
    {
        var list = new List<int> { 1, 2, 3, 4, 5 };

        list.RemoveAt(^2);

        Assert.Equal([1, 2, 3, 5], list);
    }

    [Fact]
    public void RemoveAtThrowsForIndexOutOfRange()
    {
        var list = new List<int> { 1, 2, 3 };

        Assert.Throws<ArgumentOutOfRangeException>(() => list.RemoveAt(5));
        Assert.Throws<ArgumentOutOfRangeException>(() => list.RemoveAt(^5));
    }

    [Fact]
    public void RemoveAtWorksWithIList()
    {
        IList<int> list = [1, 2, 3, 4, 5];

        list.RemoveAt(2);

        Assert.Equal([1, 2, 4, 5], list);
    }

    [Fact]
    public void RemoveRangeRemovesElementsInSpecifiedRange()
    {
        var list = new List<int> { 1, 2, 3, 4, 5 };

        list.RemoveRange(1..4);

        Assert.Equal([1, 5], list);
    }

    [Fact]
    public void RemoveRangeWithIndexFromEndRemovesCorrectElements()
    {
        var list = new List<int> { 1, 2, 3, 4, 5 };

        list.RemoveRange(1..^1);

        Assert.Equal([1, 5], list);
    }

    [Fact]
    public void RemoveRangeRemovesElementsFromStartToEnd()
    {
        var list = new List<int> { 1, 2, 3, 4, 5 };

        list.RemoveRange(..);

        Assert.Empty(list);
    }

    [Fact]
    public void RemoveRangeWithEmptyRange()
    {
        var list = new List<int> { 1, 2, 3, 4, 5 };
        var expected = new List<int>(list);

        list.RemoveRange(2..2);

        Assert.Equal(expected, list);
    }

    [Fact]
    public void RemoveRangeThrowsForInvalidOffset()
    {
        var list = new List<int> { 1, 2, 3 };

        Assert.Throws<ArgumentOutOfRangeException>(() => list.RemoveRange(4..5));
    }

    [Fact]
    public void RemoveRangeThrowsForInvalidLength()
    {
        var list = new List<int> { 1, 2, 3 };

        Assert.Throws<ArgumentOutOfRangeException>(() => list.RemoveRange(1..10));
    }

    [Fact]
    public void KeepOnlyRetainsElementsMatchingPredicate()
    {
        var list = new List<int> { 1, 2, 3, 4, 5 };

        list.KeepOnly(x => x % 2 == 0);

        Assert.Equal([2, 4], list);
    }

    [Fact]
    public void KeepOnlyWithNoMatchingElementsClearsTheList()
    {
        var list = new List<int> { 1, 2, 3, 4, 5 };

        list.KeepOnly(x => x > 10);

        Assert.Empty(list);
    }

    [Fact]
    public void KeepOnlyWithAllMatchingElementsKeepsAllElements()
    {
        var list = new List<int> { 1, 2, 3, 4, 5 };
        var original = new List<int>(list);

        list.KeepOnly(x => x > 0);

        Assert.Equal(original, list);
    }

    [Fact]
    public void KeepOnlyWorksWithEmptyList()
    {
        var list = new List<int>();

        list.KeepOnly(x => x % 2 == 0);

        Assert.Empty(list);
    }

    [Fact]
    public void AsSpanReturnsSpanOverEntireList()
    {
        var list = new List<int> { 1, 2, 3, 4, 5 };

        var span = list.AsSpan();

        Assert.Equal(5, span.Length);
        Assert.Equal(1, span[0]);
        Assert.Equal(5, span[4]);
    }

    [Fact]
    public void AsSpanWithStartIndexReturnsSpanFromSpecifiedIndex()
    {
        var list = new List<int> { 1, 2, 3, 4, 5 };

        var span = list.AsSpan(2);

        Assert.Equal(3, span.Length);
        Assert.Equal(3, span[0]);
        Assert.Equal(5, span[2]);
    }

    [Fact]
    public void AsSpanWithStartIndexAndLengthReturnsSpanWithSpecifiedLength()
    {
        var list = new List<int> { 1, 2, 3, 4, 5 };

        var span = list.AsSpan(1, 2);

        Assert.Equal(2, span.Length);
        Assert.Equal(2, span[0]);
        Assert.Equal(3, span[1]);
    }

    [Fact]
    public void AsSpanWithIndexFromEndReturnsSpanFromEnd()
    {
        var list = new List<int> { 1, 2, 3, 4, 5 };

        var span = list.AsSpan(^3);

        Assert.Equal(3, span.Length);
        Assert.Equal(3, span[0]);
        Assert.Equal(5, span[2]);
    }

    [Fact]
    public void AsSpanWithEmptyList()
    {
        var list = new List<int>();

        var span = list.AsSpan();

        Assert.Equal(0, span.Length);
    }

    [Fact]
    public void AsSpanWithZeroLength()
    {
        var list = new List<int> { 1, 2, 3, 4, 5 };

        var span = list.AsSpan(2, 0);

        Assert.Equal(0, span.Length);
    }

    [Fact]
    public void AsSpanWithRangeReturnsSpanOverSpecifiedRange()
    {
        var list = new List<int> { 1, 2, 3, 4, 5 };

        var span = list.AsSpan(1..4);

        Assert.Equal(3, span.Length);
        Assert.Equal(2, span[0]);
        Assert.Equal(4, span[2]);
    }

    [Fact]
    public void AsSpanWithRangeFromEndReturnsSpanFromEnd()
    {
        var list = new List<int> { 1, 2, 3, 4, 5 };

        var span = list.AsSpan(1..^1);

        Assert.Equal(3, span.Length);
        Assert.Equal(2, span[0]);
        Assert.Equal(4, span[2]);
    }

    [Fact]
    public void AsSpanWithFullRangeReturnsSpanOverEntireList()
    {
        var list = new List<int> { 1, 2, 3, 4, 5 };

        var span = list.AsSpan(..);

        Assert.Equal(5, span.Length);
        Assert.Equal(1, span[0]);
        Assert.Equal(5, span[4]);
    }

    [Fact]
    public void AsSpanWithEmptyRange()
    {
        var list = new List<int> { 1, 2, 3, 4, 5 };

        var span = list.AsSpan(2..2);

        Assert.Equal(0, span.Length);
    }

    [Fact]
    public void SetCountIncreasesListCount()
    {
        var list = new List<int> { 1, 2, 3 };

        list.SetCount(5);

        Assert.Equal(5, list.Count);
    }

    [Fact]
    public void SetCountDecreasesListCount()
    {
        var list = new List<int> { 1, 2, 3, 4, 5 };

        list.SetCount(3);

        Assert.Equal(3, list.Count);
        Assert.Equal([1, 2, 3], list);
    }

    [Fact]
    public void SetCountWithZero()
    {
        var list = new List<int> { 1, 2, 3, 4, 5 };

        list.SetCount(0);

        Assert.Empty(list);
    }

    [Fact]
    public void ExpandByIncreasesListCount()
    {
        var list = new List<int> { 1, 2, 3 };

        var span = list.ExpandBy(2);

        Assert.Equal(5, list.Count);
        Assert.Equal(2, span.Length);
    }

    [Fact]
    public void ExpandByWithCustomStartAtAllowsControllingSpanLocation()
    {
        var list = new List<int> { 1, 2, 3 };

        var span = list.ExpandBy(2, 1);

        Assert.Equal(3, list.Count);
        Assert.Equal(2, span.Length);
    }

    [Fact]
    public void ExpandByAllowsModifyingReturnedSpan()
    {
        var list = new List<int> { 1, 2, 3 };

        var span = list.ExpandBy(2);
        span[0] = 4;
        span[1] = 5;

        Assert.Equal(5, list.Count);
        Assert.Equal(1, list[0]);
        Assert.Equal(2, list[1]);
        Assert.Equal(3, list[2]);
        Assert.Equal(4, list[3]);
        Assert.Equal(5, list[4]);
    }

    [Fact]
    public void ExpandByWithZeroCount()
    {
        var list = new List<int> { 1, 2, 3 };
        var originalCount = list.Count;

        var span = list.ExpandBy(0);

        Assert.Equal(originalCount, list.Count);
        Assert.Equal(0, span.Length);
    }

    [Fact]
    public void ExpandByOnEmptyList()
    {
        var list = new List<int>();

        var span = list.ExpandBy(3);

        Assert.Equal(3, list.Count);
        Assert.Equal(3, span.Length);
    }
}
