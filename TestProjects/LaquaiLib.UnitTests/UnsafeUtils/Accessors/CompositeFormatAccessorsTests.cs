using System.Text;

using LaquaiLib.UnsafeUtils.Accessors;

namespace LaquaiLib.UnitTests.UnsafeUtils.Accessors;

public class CompositeFormatAccessorsTests
{
    [Fact]
    public void SegmentsCoverEveryFormatArgument()
    {
        var format = CompositeFormat.Parse("{0} literal {1}");

        ref var segments = ref CompositeFormatAccessors._segments(format);

        Assert.Equal(format.MinimumArgumentCount, segments.Count(s => s.ArgIndex >= 0));
    }
}
