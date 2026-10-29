using LaquaiLib.UnsafeUtils.Accessors;

namespace LaquaiLib.UnitTests.UnsafeUtils.Accessors;

public class MemoryStreamAccessorsTests
{
    [Fact]
    public void BufferLengthMatchesCapacityAtConstruction()
    {
        var ms = new MemoryStream(16);

        ref var buffer = ref MemoryStreamAccessors._buffer(ms);

        Assert.Equal(16, buffer.Length);
    }

    [Fact]
    public void CapacityMatchesPublicCapacity()
    {
        var ms = new MemoryStream(16);

        Assert.Equal(ms.Capacity, MemoryStreamAccessors._capacity(ms));
    }

    [Fact]
    public void LengthMatchesPublicLengthAfterWrite()
    {
        var ms = new MemoryStream(16);
        ms.Write([1, 2, 3, 4, 5]);

        Assert.Equal(ms.Length, MemoryStreamAccessors._length(ms));
    }

    [Fact]
    public void PositionMatchesPublicPositionAfterSeek()
    {
        var ms = new MemoryStream(16);
        ms.Write([1, 2, 3, 4, 5]);
        ms.Position = 2;

        Assert.Equal(ms.Position, MemoryStreamAccessors._position(ms));
    }

    [Fact]
    public void OriginMatchesUserProvidedBufferIndex()
    {
        var buffer = new byte[10];
        var ms = new MemoryStream(buffer, 3, 5);

        Assert.Equal(3, MemoryStreamAccessors._origin(ms));
    }

    [Fact]
    public void ExpandableIsFalseForUserProvidedBuffer()
    {
        var owned = new MemoryStream(16);
        var wrapped = new MemoryStream(new byte[4]);

        Assert.True(MemoryStreamAccessors._expandable(owned));
        Assert.False(MemoryStreamAccessors._expandable(wrapped));
    }

    [Fact]
    public void WritableReflectsConstructorArgument()
    {
        var readOnly = new MemoryStream(new byte[4], writable: false);

        Assert.False(MemoryStreamAccessors._writable(readOnly));
    }

    [Fact]
    public void ExposableIsFalseForUserProvidedBuffer()
    {
        var owned = new MemoryStream(16);
        var wrapped = new MemoryStream(new byte[4]);

        Assert.True(MemoryStreamAccessors._exposable(owned));
        Assert.False(MemoryStreamAccessors._exposable(wrapped));
    }

    [Fact]
    public void IsOpenBecomesFalseAfterDispose()
    {
        var ms = new MemoryStream(16);

        Assert.True(MemoryStreamAccessors._isOpen(ms));
        ms.Dispose();
        Assert.False(MemoryStreamAccessors._isOpen(ms));
    }
}
