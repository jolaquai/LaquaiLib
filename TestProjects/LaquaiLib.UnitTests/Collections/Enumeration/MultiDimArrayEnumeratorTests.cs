using LaquaiLib.Collections.Enumeration;

namespace LaquaiLib.UnitTests.Collections.Enumeration;

public class MultiDimArrayEnumeratorTests
{
    [Fact]
    public void OneDimArray()
    {
        int[] array = [1, 2, 3];
        var enumerable = new MultiDimArrayEnumerable<int>(array);
        Assert.Collection(enumerable,
            static i => Assert.Equal(1, i),
            static i => Assert.Equal(2, i),
            static i => Assert.Equal(3, i)
        );
    }
    [Fact]
    public void TwoDimArray()
    {
        int[,] array =
        {
            {1, 2, 3},
            {4, 5, 6}
        };
        var enumerable = new MultiDimArrayEnumerable<int>(array);
        Assert.Collection(enumerable,
            static i => Assert.Equal(1, i),
            static i => Assert.Equal(2, i),
            static i => Assert.Equal(3, i),
            static i => Assert.Equal(4, i),
            static i => Assert.Equal(5, i),
            static i => Assert.Equal(6, i)
        );
    }
    [Fact]
    public void ThreeDimArray()
    {
        int[,,] array =
        {
            {
                {1, 2, 3},
                {4, 5, 6}
            },
            {
                {7, 8, 9},
                {10, 11, 12}
            }
        };
        var enumerable = new MultiDimArrayEnumerable<int>(array);
        Assert.Collection(enumerable,
            static i => Assert.Equal(1, i),
            static i => Assert.Equal(2, i),
            static i => Assert.Equal(3, i),
            static i => Assert.Equal(4, i),
            static i => Assert.Equal(5, i),
            static i => Assert.Equal(6, i),
            static i => Assert.Equal(7, i),
            static i => Assert.Equal(8, i),
            static i => Assert.Equal(9, i),
            static i => Assert.Equal(10, i),
            static i => Assert.Equal(11, i),
            static i => Assert.Equal(12, i)
        );
    }
}
