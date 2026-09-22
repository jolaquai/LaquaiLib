namespace LaquaiLib.Generators.RuntimeTests.Runtime;

public class MemoryStreamProxyTests
{
    [Fact]
    public void WriteSeekReadRoundTripsBytesAndReportsLengthPositionCapacity()
    {
        using var ms = new System.IO.MemoryStream();
        var proxy = new MemoryStreamProxy(ms);
        var data = new byte[] { 1, 2, 3, 4, 5 };

        proxy.Write(data, 0, data.Length);

        Assert.Equal(5, proxy.Length);
        Assert.Equal(5, proxy.Position);
        Assert.True(proxy.Capacity >= 5);

        proxy.Seek(0, SeekOrigin.Begin);
        var readBack = new byte[5];
        var read = proxy.Read(readBack, 0, 5);

        Assert.Equal(5, read);
        Assert.Equal(data, readBack);
    }

    [Fact]
    public void DisposeThroughIDisposableInterfaceWorks()
    {
        var ms = new System.IO.MemoryStream();
        var proxy = new MemoryStreamProxy(ms);

        ((System.IDisposable)proxy).Dispose();

        Assert.Throws<ObjectDisposedException>(() => ms.WriteByte(1));
    }
}
