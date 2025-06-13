using LaquaiLib.Extensions;

namespace LaquaiLib.UnitTests.Extensions;

public class ArrayExtensionsByteTests
{
    [Fact]
    public void ToMemoryStreamWithByteArrayCreatesCorrectStream()
    {
        byte[] bytes = [1, 2, 3, 4, 5];

        using var stream = bytes.ToMemoryStream();

        Assert.Equal(bytes.Length, stream.Position);
        Assert.True(stream.CanRead);
        Assert.True(stream.CanWrite);
        Assert.True(stream.CanSeek);
    }

    [Fact]
    public void ToMemoryStreamVerifyStreamContentsMatchesSourceArray()
    {
        byte[] bytes = [10, 20, 30, 40, 50];

        using var stream = bytes.ToMemoryStream();
        stream.Position = 0;
        var readBytes = new byte[bytes.Length];
        stream.Read(readBytes, 0, readBytes.Length);

        Assert.Equal(bytes, readBytes);
    }

    [Fact]
    public void ToMemoryStreamWithEmptyArrayCreatesValidStream()
    {
        byte[] bytes =[];

        using var stream = bytes.ToMemoryStream();

        Assert.Equal(0, stream.Position);
        Assert.Equal(1, stream.Capacity);
    }

    [Fact]
    public void ToMemoryStreamHasCorrectCapacityEqualsArrayLengthPlusOne()
    {
        var bytes = new byte[100];

        using var stream = bytes.ToMemoryStream();

        Assert.Equal(bytes.Length + 1, stream.Capacity);
    }

    [Fact]
    public void ToMemoryStreamStreamIsResizableCanWriteBeyondCapacity()
    {
        byte[] bytes = [1, 2, 3];

        using var stream = bytes.ToMemoryStream();
        byte[] additional = [4, 5, 6];
        stream.Write(additional, 0, additional.Length);

        Assert.Equal(bytes.Length + additional.Length, stream.Position);

        stream.Position = 0;
        var result = new byte[bytes.Length + additional.Length];
        stream.Read(result, 0, result.Length);

        Assert.Equal([.. bytes, .. additional], result);
    }
}
