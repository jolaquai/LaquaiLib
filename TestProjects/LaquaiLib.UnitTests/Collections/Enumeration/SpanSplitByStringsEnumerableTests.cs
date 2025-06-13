using LaquaiLib.Collections.Enumeration;
using LaquaiLib.Extensions;

namespace LaquaiLib.UnitTests.Collections.Enumeration;

public class SpanSplitByStringsEnumerableTests
{
    [Fact]
    public void EmptyYieldsSingleEmpty()
    {
        var span = ReadOnlySpan<char>.Empty;
        var result = new SpanSplitByStringsEnumerable(span, ["abc"]);
        Assert.True(result.MoveNext());
        Assert.Equal("", result.Current);
        Assert.False(result.MoveNext());
    }
    [Fact]
    public void ExactMatchYieldsTwoEmpty()
    {
        var span = "abc".AsSpan();
        var result = new SpanSplitByStringsEnumerable(span, ["abc"]);
        Assert.True(result.MoveNext());
        Assert.Equal("", result.Current);
        Assert.True(result.MoveNext());
        Assert.Equal("", result.Current);
        Assert.False(result.MoveNext());
    }
    [Fact]
    public void DifferentCaseYieldsInput()
    {
        var span = "abc".AsSpan();
        var result = new SpanSplitByStringsEnumerable(span, ["ABC"]);
        Assert.True(result.MoveNext());
        Assert.Equal("abc", result.Current);
        Assert.False(result.MoveNext());
    }
    [Fact]
    public void DifferentCaseWithComparisonMatchYieldsTwoEmpty()
    {
        var span = "abc".AsSpan();
        var result = new SpanSplitByStringsEnumerable(span, ["ABC"], StringComparison.OrdinalIgnoreCase);
        Assert.True(result.MoveNext());
        Assert.Equal("", result.Current);
        Assert.True(result.MoveNext());
        Assert.Equal("", result.Current);
        Assert.False(result.MoveNext());
    }
    [Fact]
    public void TypicalSplitApplicationWorksNoComparison()
    {
        var span = "abcXXXdefXXXghi".AsSpan();
        var result = new SpanSplitByStringsEnumerable(span, ["XXX"]);

        Assert.True(result.MoveNext());
        Assert.Equal("abc", result.Current);
        Assert.True(result.MoveNext());
        Assert.Equal("def", result.Current);
        Assert.True(result.MoveNext());
        Assert.Equal("ghi", result.Current);
        Assert.False(result.MoveNext());
    }
    [Fact]
    public void TypicalSplitApplicationWorksWithComparison()
    {
        var span = "abcXXXdefXXXghi".AsSpan();
        var result = new SpanSplitByStringsEnumerable(span, ["xxx"], StringComparison.OrdinalIgnoreCase);

        Assert.True(result.MoveNext());
        Assert.Equal("abc", result.Current);
        Assert.True(result.MoveNext());
        Assert.Equal("def", result.Current);
        Assert.True(result.MoveNext());
        Assert.Equal("ghi", result.Current);
        Assert.False(result.MoveNext());
    }
}
