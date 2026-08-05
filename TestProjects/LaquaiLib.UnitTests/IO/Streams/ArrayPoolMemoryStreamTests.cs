using LaquaiLib.IO.Streams;

namespace LaquaiLib.UnitTests.IO.Streams;

public class ArrayPoolMemoryStreamTests
{
    private static byte[] Sequence(int length)
    {
        var data = new byte[length];
        for (var i = 0; i < length; i++)
            data[i] = (byte)((i % 251) + 1);
        return data;
    }

    private static ArrayPoolMemoryStream StreamWith(params byte[] data)
    {
        var stream = new ArrayPoolMemoryStream();
        stream.Write(data, 0, data.Length);
        stream.Position = 0;
        return stream;
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ConstructorRejectsNonPositiveSmallSegmentSize(int size) => Assert.Throws<ArgumentOutOfRangeException>(() => new ArrayPoolMemoryStream(size));

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ConstructorRejectsNonPositiveLargeSegmentSize(int size) => Assert.Throws<ArgumentOutOfRangeException>(() => new ArrayPoolMemoryStream(2048, size));

    [Fact]
    public void ConstructorRejectsNegativeCapacity() => Assert.Throws<ArgumentOutOfRangeException>(() => new ArrayPoolMemoryStream(2048, 16384, -1));

    [Fact]
    public void NewStreamIsEmpty()
    {
        using var stream = new ArrayPoolMemoryStream();
        Assert.Equal(0L, stream.Length);
    }

    [Fact]
    public void NewStreamPositionIsZero()
    {
        using var stream = new ArrayPoolMemoryStream();
        Assert.Equal(0L, stream.Position);
    }

    [Fact]
    public void StreamSupportsAllCapabilities()
    {
        using var stream = new ArrayPoolMemoryStream();
        Assert.True(stream.CanRead);
        Assert.True(stream.CanSeek);
        Assert.True(stream.CanWrite);
    }

    [Fact]
    public void PreallocatedStreamHasZeroLength()
    {
        using var stream = new ArrayPoolMemoryStream(capacity: 100);
        Assert.Equal(0L, stream.Length);
    }

    [Fact]
    public void PreallocatedStreamStartsAtBeginning()
    {
        using var stream = new ArrayPoolMemoryStream(capacity: 100);
        Assert.Equal(0L, stream.Position);
    }

    [Fact]
    public void CapacityReflectsPreallocatedMemory()
    {
        using var stream = new ArrayPoolMemoryStream(capacity: 100);
        Assert.True(stream.Capacity >= 100);
    }

    [Fact]
    public void CapacityCoversRequestAcrossMultipleSegments()
    {
        using var stream = new ArrayPoolMemoryStream(2048, 16384, 20000);
        Assert.True(stream.Capacity >= 20000);
    }

    [Fact]
    public void WriteAdvancesLength()
    {
        using var stream = new ArrayPoolMemoryStream();
        stream.Write(new byte[] { 1, 2, 3, 4 }, 0, 4);
        Assert.Equal(4L, stream.Length);
    }

    [Fact]
    public void WriteAdvancesPosition()
    {
        using var stream = new ArrayPoolMemoryStream();
        stream.Write(new byte[] { 1, 2, 3, 4 }, 0, 4);
        Assert.Equal(4L, stream.Position);
    }

    [Fact]
    public void PositionMatchesLengthAfterSuccessiveWrites()
    {
        using var stream = new ArrayPoolMemoryStream();
        stream.Write(new byte[] { 1, 2, 3, 4 }, 0, 4);
        stream.Write(new byte[] { 5, 6 }, 0, 2);
        Assert.Equal(6L, stream.Length);
        Assert.Equal(6L, stream.Position);
    }

    [Fact]
    public void SuccessiveWritesAreContiguous()
    {
        using var stream = new ArrayPoolMemoryStream();
        stream.Write(new byte[] { 1, 2, 3, 4 }, 0, 4);
        stream.Write(new byte[] { 5, 6 }, 0, 2);
        stream.Write(new byte[] { 7, 8, 9 }, 0, 3);
        stream.Position = 0;

        var buffer = new byte[9];
        Assert.Equal(9, stream.Read(buffer, 0, 9));
        Assert.Equal(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 }, buffer);
    }

    [Fact]
    public void WriteOfEmptyBufferIsNoOp()
    {
        using var stream = new ArrayPoolMemoryStream();
        stream.Write([], 0, 0);
        Assert.Equal(0L, stream.Length);
    }

    [Fact]
    public void WriteByteAppendsASingleByte()
    {
        using var stream = new ArrayPoolMemoryStream();
        stream.WriteByte(1);
        stream.WriteByte(2);
        Assert.Equal(2L, stream.Length);
        stream.Position = 0;
        Assert.Equal(1, stream.ReadByte());
        Assert.Equal(2, stream.ReadByte());
    }

    [Fact]
    public void OverwritingDoesNotGrowLength()
    {
        using var stream = new ArrayPoolMemoryStream();
        stream.Write(new byte[] { 1, 2, 3, 4 }, 0, 4);
        stream.Position = 0;
        stream.Write(new byte[] { 9, 9 }, 0, 2);
        Assert.Equal(4L, stream.Length);
    }

    [Fact]
    public void OverwritingReplacesExistingBytes()
    {
        using var stream = new ArrayPoolMemoryStream();
        stream.Write(new byte[] { 1, 2, 3, 4 }, 0, 4);
        stream.Position = 1;
        stream.Write(new byte[] { 9, 9 }, 0, 2);
        stream.Position = 0;
        var buffer = new byte[4];
        Assert.Equal(4, stream.Read(buffer, 0, 4));
        Assert.Equal(new byte[] { 1, 9, 9, 4 }, buffer);
    }

    [Fact]
    public void ReadFromEmptyStreamReturnsZero()
    {
        using var stream = new ArrayPoolMemoryStream();
        Assert.Equal(0, stream.Read(new byte[4], 0, 4));
    }

    [Fact]
    public void ReadByteOnEmptyStreamReturnsMinusOne()
    {
        using var stream = new ArrayPoolMemoryStream();
        Assert.Equal(-1, stream.ReadByte());
    }

    [Fact]
    public void ReadIntoEmptyBufferReturnsZero()
    {
        using var stream = StreamWith(1, 2, 3);
        Assert.Equal(0, stream.Read([], 0, 0));
    }

    [Fact]
    public void ReadReturnsWrittenBytes()
    {
        using var stream = StreamWith(1, 2, 3, 4, 5);
        var buffer = new byte[5];
        Assert.Equal(5, stream.Read(buffer, 0, 5));
        Assert.Equal(new byte[] { 1, 2, 3, 4, 5 }, buffer);
    }

    [Fact]
    public void ReadAdvancesPosition()
    {
        using var stream = StreamWith(1, 2, 3, 4, 5);
        Assert.Equal(2, stream.Read(new byte[2], 0, 2));
        Assert.Equal(2L, stream.Position);
    }

    [Fact]
    public void ReadStopsAtEndOfStream()
    {
        using var stream = StreamWith(1, 2, 3);
        Assert.Equal(3, stream.Read(new byte[16], 0, 16));
    }

    [Fact]
    public void ReadDoesNotExposeBytesBeyondLength()
    {
        using var stream = StreamWith(1, 2, 3);
        var buffer = new byte[6];
        Assert.Equal(3, stream.Read(buffer, 0, 6));
        Assert.Equal(new byte[] { 1, 2, 3, 0, 0, 0 }, buffer);
    }

    [Fact]
    public void SequentialReadsReturnSuccessiveChunks()
    {
        using var stream = StreamWith(1, 2, 3, 4, 5, 6);
        var first = new byte[3];
        var second = new byte[3];
        Assert.Equal(3, stream.Read(first, 0, 3));
        Assert.Equal(3, stream.Read(second, 0, 3));
        Assert.Equal(new byte[] { 1, 2, 3 }, first);
        Assert.Equal(new byte[] { 4, 5, 6 }, second);
    }

    [Fact]
    public void ReadAtEndOfStreamReturnsZero()
    {
        using var stream = StreamWith(1, 2, 3);
        Assert.Equal(3, stream.Read(new byte[3], 0, 3));
        Assert.Equal(0, stream.Read(new byte[3], 0, 3));
    }

    [Fact]
    public void ReadFromNonZeroPositionSkipsLeadingBytes()
    {
        using var stream = StreamWith(1, 2, 3, 4, 5);
        stream.Position = 2;
        var buffer = new byte[3];
        Assert.Equal(3, stream.Read(buffer, 0, 3));
        Assert.Equal(new byte[] { 3, 4, 5 }, buffer);
    }

    [Fact]
    public void ReadByteReturnsBytesInOrderThenMinusOne()
    {
        using var stream = StreamWith(1, 2, 3);
        Assert.Equal(1, stream.ReadByte());
        Assert.Equal(2, stream.ReadByte());
        Assert.Equal(3, stream.ReadByte());
        Assert.Equal(-1, stream.ReadByte());
    }

    [Fact]
    public void ReadByteAdvancesPosition()
    {
        using var stream = StreamWith(1, 2, 3);
        stream.ReadByte();
        Assert.Equal(1L, stream.Position);
    }

    [Fact]
    public void LargeWriteSpansMultipleSegments()
    {
        using var stream = new ArrayPoolMemoryStream(16, 32);
        var data = Sequence(200);
        stream.Write(data, 0, data.Length);
        Assert.Equal(200L, stream.Length);
        stream.Position = 0;
        var buffer = new byte[200];
        Assert.Equal(200, stream.Read(buffer, 0, 200));
        Assert.Equal(data, buffer);
    }

    [Fact]
    public void ReadAcrossSegmentBoundaryFromOffsetIsContiguous()
    {
        using var stream = new ArrayPoolMemoryStream(16, 16);
        var data = Sequence(48);
        stream.Write(data, 0, data.Length);
        stream.Position = 8;
        var buffer = new byte[40];
        Assert.Equal(40, stream.Read(buffer, 0, 40));
        Assert.Equal(data.AsSpan(8).ToArray(), buffer);
    }

    [Fact]
    public void SeekFromBeginSetsAbsolutePosition()
    {
        using var stream = StreamWith(1, 2, 3, 4, 5);
        Assert.Equal(3L, stream.Seek(3, SeekOrigin.Begin));
        Assert.Equal(3L, stream.Position);
    }

    [Fact]
    public void SeekFromCurrentIsRelative()
    {
        using var stream = StreamWith(1, 2, 3, 4, 5);
        stream.Seek(2, SeekOrigin.Begin);
        Assert.Equal(4L, stream.Seek(2, SeekOrigin.Current));
    }

    [Fact]
    public void SeekFromEndIsRelativeToLength()
    {
        using var stream = StreamWith(1, 2, 3, 4, 5);
        Assert.Equal(3L, stream.Seek(-2, SeekOrigin.End));
    }

    [Fact]
    public void SeekWithInvalidOriginThrows()
    {
        using var stream = StreamWith(1, 2, 3);
        Assert.Throws<ArgumentOutOfRangeException>(() => stream.Seek(0, (SeekOrigin)42));
    }

    [Fact]
    public void SeekToNegativePositionThrows()
    {
        using var stream = StreamWith(1, 2, 3);
        Assert.Throws<ArgumentOutOfRangeException>(() => stream.Seek(-1, SeekOrigin.Begin));
    }

    [Fact]
    public void PositionSetterRejectsNegativeValues()
    {
        using var stream = StreamWith(1, 2, 3);
        Assert.Throws<ArgumentOutOfRangeException>(() => stream.Position = -1);
    }

    [Fact]
    public void SetLengthTruncates()
    {
        using var stream = StreamWith(1, 2, 3, 4, 5);
        stream.SetLength(2);
        Assert.Equal(2L, stream.Length);
    }

    [Fact]
    public void SetLengthToCurrentLengthIsNoOp()
    {
        using var stream = StreamWith(1, 2, 3);
        stream.SetLength(3);
        Assert.Equal(3L, stream.Length);
    }

    [Fact]
    public void SetLengthCannotGrowTheStream()
    {
        using var stream = StreamWith(1, 2, 3);
        Assert.Throws<NotSupportedException>(() => stream.SetLength(100));
    }

    [Fact]
    public void TruncatedStreamDoesNotYieldDiscardedBytes()
    {
        using var stream = StreamWith(1, 2, 3, 4, 5);
        stream.SetLength(2);
        stream.Position = 0;
        var buffer = new byte[5];
        Assert.Equal(2, stream.Read(buffer, 0, 5));
    }

    [Fact]
    public void CopyToWritesStreamContents()
    {
        using var stream = StreamWith(1, 2, 3);
        using var target = new MemoryStream();
        stream.CopyTo(target);
        Assert.Equal(new byte[] { 1, 2, 3 }, target.ToArray());
    }

    [Fact]
    public void CopyToStartsAtCurrentPosition()
    {
        using var stream = StreamWith(1, 2, 3, 4, 5);
        stream.Position = 2;
        using var target = new MemoryStream();
        stream.CopyTo(target);
        Assert.Equal(new byte[] { 3, 4, 5 }, target.ToArray());
    }

    [Fact]
    public void CopyToRejectsNullDestination()
    {
        using var stream = StreamWith(1, 2, 3);
        Assert.Throws<ArgumentNullException>(() => stream.CopyTo(null, 4096));
    }

    [Fact]
    public async Task CopyToAsyncWritesStreamContents()
    {
        using var stream = StreamWith(1, 2, 3);
        using var target = new MemoryStream();
        await stream.CopyToAsync(target);
        Assert.Equal(new byte[] { 1, 2, 3 }, target.ToArray());
    }

    [Fact]
    public async Task CopyToAsyncRejectsNullDestination()
    {
        using var stream = StreamWith(1, 2, 3);
        await Assert.ThrowsAsync<ArgumentNullException>(async () => { await stream.CopyToAsync(null, 4096, default); });
    }

    [Fact]
    public async Task WriteAsyncMatchesSynchronousWrite()
    {
        using var stream = new ArrayPoolMemoryStream();
        await stream.WriteAsync(new byte[] { 1, 2, 3 }.AsMemory());
        Assert.Equal(3L, stream.Length);
        stream.Position = 0;
        var buffer = new byte[3];
        Assert.Equal(3, stream.Read(buffer, 0, 3));
        Assert.Equal(new byte[] { 1, 2, 3 }, buffer);
    }

    [Fact]
    public async Task WriteAsyncArrayOverloadHonoursOffsetAndCount()
    {
        using var stream = new ArrayPoolMemoryStream();
        await stream.WriteAsync(new byte[] { 0, 1, 2, 3, 0 }, 1, 3, default);
        Assert.Equal(3L, stream.Length);
        stream.Position = 0;
        var buffer = new byte[3];
        Assert.Equal(3, stream.Read(buffer, 0, 3));
        Assert.Equal(new byte[] { 1, 2, 3 }, buffer);
    }

    [Fact]
    public async Task ReadAsyncReturnsWrittenBytes()
    {
        using var stream = StreamWith(1, 2, 3);
        var buffer = new byte[3];
        Assert.Equal(3, await stream.ReadAsync(buffer.AsMemory()));
        Assert.Equal(new byte[] { 1, 2, 3 }, buffer);
    }

    [Fact]
    public async Task ReadAsyncArrayOverloadHonoursOffsetAndCount()
    {
        using var stream = StreamWith(1, 2, 3);
        var buffer = new byte[5];
        Assert.Equal(3, await stream.ReadAsync(buffer, 1, 3, default));
        Assert.Equal(new byte[] { 0, 1, 2, 3, 0 }, buffer);
    }

    [Fact]
    public async Task ReadAsyncAdvancesPosition()
    {
        using var stream = StreamWith(1, 2, 3);
        Assert.Equal(2, await stream.ReadAsync(new byte[2].AsMemory()));
        Assert.Equal(2L, stream.Position);
    }

    [Fact]
    public void FlushDoesNotThrow()
    {
        using var stream = StreamWith(1, 2, 3);
        stream.Flush();
        Assert.Equal(3L, stream.Length);
    }

    [Fact]
    public async Task FlushAsyncCompletes()
    {
        using var stream = StreamWith(1, 2, 3);
        await stream.FlushAsync();
        Assert.Equal(3L, stream.Length);
    }

    [Fact]
    public void ReadWriteRoundTripSurvivesInterleavedSeeks()
    {
        using var stream = new ArrayPoolMemoryStream(32, 64);
        var data = Sequence(150);
        stream.Write(data, 0, data.Length);

        for (var offset = 0; offset < data.Length; offset += 37)
        {
            stream.Position = offset;
            var take = Math.Min(20, data.Length - offset);
            var buffer = new byte[take];
            Assert.Equal(take, stream.Read(buffer, 0, take));
            Assert.Equal(data.AsSpan(offset, take).ToArray(), buffer);
        }
    }

    [Fact]
    public void DisposeDoesNotThrow()
    {
        var stream = StreamWith(1, 2, 3);
        stream.Dispose();
        stream.Dispose();
    }
}
