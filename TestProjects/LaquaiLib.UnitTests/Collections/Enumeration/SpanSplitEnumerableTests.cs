using LaquaiLib.Collections.Enumeration;
using LaquaiLib.Extensions;
using LaquaiLib.Util;

namespace LaquaiLib.UnitTests.Collections.Enumeration;

public class SpanSplitEnumerableTests
{
    [Fact]
    public void EmptyYieldsSingleEmpty()
    {
        var span = ReadOnlySpan<char>.Empty;
        var result = new SpanSplitEnumerable<char>(span, "abc");
        Assert.True(result.MoveNext());
        Assert.Equal("", result.Current);
        Assert.False(result.MoveNext());
    }
    [Fact]
    public void ExactMatchYieldsInputLengthPlus1Empty()
    {
        var span = "abc".AsSpan();
        var result = new SpanSplitEnumerable<char>(span, "abc");

        var i = 0;
        while (result.MoveNext())
        {
            i++;
            Assert.Equal("", result.Current);
        }
        Assert.Equal(span.Length + 1, i);
    }
    [Fact]
    public void DifferentCaseYieldsInput()
    {
        var span = "abc".AsSpan();
        var result = new SpanSplitEnumerable<char>(span, "ABC");
        Assert.True(result.MoveNext());
        Assert.Equal("abc", result.Current);
        Assert.False(result.MoveNext());
    }
    [Fact]
    public void DifferentCaseWithComparerMatchYieldsInputLengthPlus1Empty()
    {
        var span = "abc".AsSpan();
        var result = new SpanSplitEnumerable<char>(span, "ABC", CharComparer.CurrentCultureIgnoreCase);

        var i = 0;
        while (result.MoveNext())
        {
            i++;
            Assert.Equal("", result.Current);
        }
        Assert.Equal(span.Length + 1, i);
    }
    [Fact]
    public void AtypicalSplitApplicationWorks()
    {
        // This should be done using SpanSplitByStringsEnumerable because of its explicit string comparer support, but it still needs to work
        var span = "abc#def!ghi".AsSpan();
        var result = new SpanSplitEnumerable<char>(span, "#!");

        Assert.True(result.MoveNext());
        Assert.Equal("abc", result.Current);
        Assert.True(result.MoveNext());
        Assert.Equal("def", result.Current);
        Assert.True(result.MoveNext());
        Assert.Equal("ghi", result.Current);
        Assert.False(result.MoveNext());
    }
    [Fact]
    public void TypicalSplitApplicationWorks()
    {
        ReadOnlySpan<byte> span = [1, 2, 3, 7, 3, 5, 7, 8, 0, 2, 3, 5, 5, 2, 255, 3, 5, 6, 7];
        var result = new SpanSplitEnumerable<byte>(span, [0, 255]);

        Assert.True(result.MoveNext());
        Assert.True(result.Current.SequenceEqual((ReadOnlySpan<byte>)[1, 2, 3, 7, 3, 5, 7, 8]));
        Assert.True(result.MoveNext());
        Assert.True(result.Current.SequenceEqual((ReadOnlySpan<byte>)[2, 3, 5, 5, 2]));
        Assert.True(result.MoveNext());
        Assert.True(result.Current.SequenceEqual((ReadOnlySpan<byte>)[3, 5, 6, 7]));
        Assert.False(result.MoveNext());
    }
}
