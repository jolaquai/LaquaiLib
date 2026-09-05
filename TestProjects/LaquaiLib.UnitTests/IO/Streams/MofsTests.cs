using LaquaiLib.IO.Streams;

namespace LaquaiLib.UnitTests.IO.Streams;

public class MofsTests
{
    [Fact]
    public void BelowCutoffReturnsMemoryStream()
    {
        MemoryOrFileStream.Cutoff = 2;
        using var stream = MemoryOrFileStream.Create(1);
        Assert.IsType<MemoryStream>((Stream)stream);
    }
    [Fact]
    public void AtCutoffReturnsFileStream()
    {
        MemoryOrFileStream.Cutoff = 2;
        using var stream = MemoryOrFileStream.Create(2);
        Assert.IsType<FileStream>((Stream)stream);
    }
    [Fact]
    public void AboveCutoffReturnsFileStream()
    {
        MemoryOrFileStream.Cutoff = 2;
        using var stream = MemoryOrFileStream.Create(3);
        Assert.IsType<FileStream>((Stream)stream);
    }
    [Fact]
    public void DisposeDeletesTempFile()
    {
        MemoryOrFileStream.Cutoff = 2;
        string tempFile = null;
        using (var stream = MemoryOrFileStream.Create(3))
        {
            var fs = Assert.IsType<FileStream>((Stream)stream);
            tempFile = fs.Name;
            Assert.True(File.Exists(tempFile));
        }
        Assert.False(File.Exists(tempFile));
    }

    [Fact]
    public void CanChangeCutoff()
    {
        MemoryOrFileStream.Cutoff = 2;
        Assert.Equal(2, MemoryOrFileStream.Cutoff);
        MemoryOrFileStream.Cutoff = 4;
        Assert.Equal(4, MemoryOrFileStream.Cutoff);
    }
    [Fact]
    public void ChangingCutoffIsLive()
    {
        MemoryOrFileStream.Cutoff = 2;
        using (var stream = MemoryOrFileStream.Create(3))
            Assert.IsType<FileStream>((Stream)stream);
        MemoryOrFileStream.Cutoff = 4;
        using (var stream = MemoryOrFileStream.Create(3))
            Assert.IsType<MemoryStream>((Stream)stream);
    }

    [Fact]
    public void CutoffParameterIgnoresGlobalCutoff()
    {
        MemoryOrFileStream.Cutoff = 2;
        using (var stream = MemoryOrFileStream.Create(3, 4))
            Assert.IsType<MemoryStream>((Stream)stream);
        using (var stream = MemoryOrFileStream.Create(4, 2))
            Assert.IsType<FileStream>((Stream)stream);
    }
}
