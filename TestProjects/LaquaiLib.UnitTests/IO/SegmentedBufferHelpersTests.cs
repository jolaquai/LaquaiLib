using LaquaiLib.IO;

namespace LaquaiLib.UnitTests.IO;

public class SegmentedBufferHelpersTests
{
    private static byte[][] Segments(params int[] lengths)
    {
        var segments = new byte[lengths.Length][];
        for (var i = 0; i < lengths.Length; i++)
            segments[i] = new byte[lengths[i]];
        return segments;
    }

    [Theory]
    [InlineData(0, 0, 0)]
    [InlineData(1, 0, 1)]
    [InlineData(3, 0, 3)]
    [InlineData(4, 1, 0)]
    [InlineData(5, 1, 1)]
    [InlineData(9, 1, 5)]
    [InlineData(10, 2, 0)]
    [InlineData(19, 2, 9)]
    public void AbsoluteToRelativeMapsIndexIntoOwningSegment(long index, int expectedSegment, int expectedOffset)
    {
        var (segment, offset) = SegmentedBufferHelpers.AbsoluteToRelative<byte>(Segments(4, 6, 10), index);
        Assert.Equal(expectedSegment, segment);
        Assert.Equal(expectedOffset, offset);
    }

    [Fact]
    public void AbsoluteToRelativeThrowsForIndexEqualToTotalLength()
    {
        var segments = Segments(4, 6, 10);
        Assert.Throws<ArgumentOutOfRangeException>(() => SegmentedBufferHelpers.AbsoluteToRelative<byte>(segments, 20));
    }

    [Fact]
    public void AbsoluteToRelativeThrowsForIndexBeyondTotalLength()
    {
        var segments = Segments(4, 6, 10);
        Assert.Throws<ArgumentOutOfRangeException>(() => SegmentedBufferHelpers.AbsoluteToRelative<byte>(segments, 21));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-5)]
    [InlineData(long.MinValue)]
    public void AbsoluteToRelativeThrowsForNegativeIndex(long index)
    {
        var segments = Segments(4, 6, 10);
        Assert.Throws<ArgumentOutOfRangeException>(() => SegmentedBufferHelpers.AbsoluteToRelative<byte>(segments, index));
    }

    [Fact]
    public void AbsoluteToRelativeReturnsOriginForZeroIndexOnEmptyChain()
    {
        var (segment, offset) = SegmentedBufferHelpers.AbsoluteToRelative<byte>([], 0);
        Assert.Equal(0, segment);
        Assert.Equal(0, offset);
    }

    [Fact]
    public void AbsoluteToRelativeThrowsForNonZeroIndexOnEmptyChain()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => SegmentedBufferHelpers.AbsoluteToRelative<byte>([], 1));
    }

    [Fact]
    public void AbsoluteToRelativeSkipsZeroLengthSegments()
    {
        var (segment, offset) = SegmentedBufferHelpers.AbsoluteToRelative<byte>(Segments(0, 3), 1);
        Assert.Equal(1, segment);
        Assert.Equal(1, offset);
    }

    [Fact]
    public void AbsoluteToRelativeHandlesSingleSegment()
    {
        var (segment, offset) = SegmentedBufferHelpers.AbsoluteToRelative<byte>(Segments(8), 7);
        Assert.Equal(0, segment);
        Assert.Equal(7, offset);
    }

    [Theory]
    [InlineData(0, 0, 0)]
    [InlineData(0, 3, 3)]
    [InlineData(1, 0, 4)]
    [InlineData(1, 2, 6)]
    [InlineData(2, 0, 10)]
    [InlineData(2, 5, 15)]
    public void RelativeToAbsoluteSumsPrecedingSegments(int segment, int index, long expected)
    {
        Assert.Equal(expected, SegmentedBufferHelpers.RelativeToAbsolute<byte>(Segments(4, 6, 10), segment, index));
    }

    [Fact]
    public void RelativeToAbsoluteReturnsTotalLengthForSegmentPastEnd()
    {
        Assert.Equal(20L, SegmentedBufferHelpers.RelativeToAbsolute<byte>(Segments(4, 6, 10), 3, 0));
    }

    [Fact]
    public void RelativeToAbsoluteThrowsForSegmentBeyondChain()
    {
        var segments = Segments(4, 6, 10);
        Assert.Throws<IndexOutOfRangeException>(() => SegmentedBufferHelpers.RelativeToAbsolute<byte>(segments, 4, 0));
    }

    [Fact]
    public void RelativeToAbsoluteReturnsZeroForEmptyChainAtOrigin()
    {
        Assert.Equal(0L, SegmentedBufferHelpers.RelativeToAbsolute<byte>([], 0, 0));
    }

    [Fact]
    public void RelativeToAbsoluteIgnoresZeroLengthSegments()
    {
        Assert.Equal(2L, SegmentedBufferHelpers.RelativeToAbsolute<byte>(Segments(0, 3), 1, 2));
    }

    [Fact]
    public void ConversionsRoundTripAcrossEveryValidIndex()
    {
        var segments = Segments(4, 6, 10);
        for (var i = 0L; i < 20; i++)
        {
            var (segment, offset) = SegmentedBufferHelpers.AbsoluteToRelative<byte>(segments, i);
            Assert.Equal(i, SegmentedBufferHelpers.RelativeToAbsolute<byte>(segments, segment, offset));
        }
    }

    [Fact]
    public void ConversionsRoundTripWithUnevenSegmentSizes()
    {
        var segments = Segments(1, 7, 2, 13, 1);
        for (var i = 0L; i < 24; i++)
        {
            var (segment, offset) = SegmentedBufferHelpers.AbsoluteToRelative<byte>(segments, i);
            Assert.InRange(segment, 0, segments.Length - 1);
            Assert.InRange(offset, 0, segments[segment].Length - 1);
            Assert.Equal(i, SegmentedBufferHelpers.RelativeToAbsolute<byte>(segments, segment, offset));
        }
    }

    [Fact]
    public void HelpersOperateOnReferenceTypeSegments()
    {
        var segments = new string[][] { new string[2], new string[3] };
        var (segment, offset) = SegmentedBufferHelpers.AbsoluteToRelative(segments, 3);
        Assert.Equal(1, segment);
        Assert.Equal(1, offset);
        Assert.Equal(3L, SegmentedBufferHelpers.RelativeToAbsolute(segments, segment, offset));
    }
}
