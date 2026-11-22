using LaquaiLib.Extensions;

namespace LaquaiLib.UnitTests.Extensions;

public class StreamExtensionsMemoryStreamTests
{
    [Fact]
    public void AsSpanUnsafeSpansEntireBackingArray()
    {
        var ms = new MemoryStream(16);
        ms.WriteByte(1);
        Assert.Equal(16, ms.AsSpanUnsafe().Length);
        Assert.Equal(1, ms.AsSpanUnsafe()[0]);
    }

    [Fact]
    public void AsSpanUnsafeIgnoresOrigin()
    {
        var arr = new byte[] { 1, 2, 3, 4, 5 };
        using var ms = new MemoryStream(arr, 2, 2);
        Assert.Equal(5, ms.AsSpanUnsafe().Length);
        Assert.Equal(1, ms.AsSpanUnsafe()[0]);
    }

    [Fact]
    public void AsSpanIsBoundedByLengthNotCapacity()
    {
        var ms = new MemoryStream(256);
        ms.Write([1, 2, 3]);
        Assert.Equal(3, ms.AsSpan().Length);
        Assert.Equal([1, 2, 3], ms.AsSpan().ToArray());
    }

    [Fact]
    public void AsSpanHonorsOrigin()
    {
        var arr = new byte[] { 1, 2, 3, 4, 5 };
        using var ms = new MemoryStream(arr, 1, 3);
        Assert.Equal([2, 3, 4], ms.AsSpan().ToArray());
        Assert.Equal([3, 4], ms.AsSpan(1).ToArray());
        Assert.Equal([3, 4], ms.AsSpan(^2).ToArray());
        Assert.Equal([3], ms.AsSpan(1..2).ToArray());
        Assert.Equal([3, 4], ms.AsSpan(1, 2).ToArray());
    }

    [Fact]
    public void AsSpanIsEmptyForEmptyStream()
    {
        using var ms = new MemoryStream();
        Assert.True(ms.AsSpan().IsEmpty);
        Assert.True(ms.AsSpan(0).IsEmpty);
    }

    [Fact]
    public void AsSpanThrowsPastLength()
    {
        var ms = new MemoryStream(256);
        ms.Write([1, 2, 3]);
        Assert.Throws<ArgumentOutOfRangeException>(() => ms.AsSpan(4));
        Assert.Throws<ArgumentOutOfRangeException>(() => ms.AsSpan(2, 2));
        Assert.Throws<ArgumentOutOfRangeException>(() => ms.AsSpan(0..4));
    }

    [Fact]
    public void AsSpanWritesAreVisibleInStream()
    {
        var ms = new MemoryStream();
        ms.Write([1, 2, 3]);
        ms.AsSpan(1, 1)[0] = 9;
        Assert.Equal([1, 9, 3], ms.ToArray());
    }

    [Fact]
    public void CopyBlockHonorsOrigin()
    {
        var arr = new byte[] { 1, 2, 3, 4, 5 };
        using var ms = new MemoryStream(arr, 1, 3);
        Span<byte> dest = stackalloc byte[2];
        ms.CopyBlock(1, 2, dest);
        Assert.Equal([3, 4], dest.ToArray());
    }
}
