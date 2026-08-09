using System.Buffers;

using LaquaiLib.IO.Streams;

namespace LaquaiLib.UnitTests.IO.Streams;

public class ArrayPoolMemoryStreamTests
{
    // hands out exact-size arrays prefilled with a known byte, so segment layout and zeroing are observable without relying on any real pool's behaviour
    private sealed class TrackingArrayPool(byte fill = 0) : ArrayPool<byte>
    {
        public List<int> RentRequests { get; } = [];
        public List<byte[]> Rented { get; } = [];
        public List<byte[]> Returns { get; } = [];

        public override byte[] Rent(int minimumLength)
        {
            var array = new byte[minimumLength];
            array.AsSpan().Fill(fill);
            RentRequests.Add(minimumLength);
            Rented.Add(array);
            return array;
        }
        public override void Return(byte[] array, bool clearArray = false)
        {
            Returns.Add(array);
            if (clearArray)
                Array.Clear(array);
        }
    }

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
    public void ConstructorRejectsNonPositiveMinimumSegmentSize(int size) => Assert.Throws<ArgumentOutOfRangeException>(() => new ArrayPoolMemoryStream(size));

    [Fact]
    public void ConstructorRejectsNegativeCapacity() => Assert.Throws<ArgumentOutOfRangeException>(() => new ArrayPoolMemoryStream(2048, -1));

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
    public void CapacityCoversLargePreallocationRequest()
    {
        using var stream = new ArrayPoolMemoryStream(2048, 20000);
        Assert.True(stream.Capacity >= 20000);
    }

    [Fact]
    public void PreallocationAboveMinimumRentsOneSegment()
    {
        var pool = new TrackingArrayPool();
        using var stream = new ArrayPoolMemoryStream(2048, 20000, pool: pool);
        Assert.Equal(new[] { 20000 }, pool.RentRequests);
    }

    [Fact]
    public void StreamRentsFromSuppliedPool()
    {
        var pool = new TrackingArrayPool();
        using var stream = new ArrayPoolMemoryStream(pool: pool);
        stream.WriteByte(1);
        Assert.Single(pool.RentRequests);
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
    public void SingleWriteAboveMinimumRentsOneSegment()
    {
        var pool = new TrackingArrayPool();
        using var stream = new ArrayPoolMemoryStream(16, pool: pool);
        var data = Sequence(10000);
        stream.Write(data, 0, data.Length);
        Assert.Equal(new[] { 10000 }, pool.RentRequests);
    }

    [Fact]
    public void WritesBelowMinimumRentAtTheMinimumSize()
    {
        var pool = new TrackingArrayPool();
        using var stream = new ArrayPoolMemoryStream(16, pool: pool);
        for (var i = 0; i < 4; i++)
            stream.Write(Sequence(16), 0, 16);
        Assert.Equal(new[] { 16, 16, 16, 16 }, pool.RentRequests);
    }

    [Fact]
    public void SuccessiveWritesSpanMultipleSegments()
    {
        var pool = new TrackingArrayPool();
        using var stream = new ArrayPoolMemoryStream(16, pool: pool);
        var data = Sequence(200);
        for (var offset = 0; offset < data.Length; offset += 8)
            stream.Write(data, offset, 8);

        Assert.True(pool.RentRequests.Count > 1);
        Assert.Equal(200L, stream.Length);
        stream.Position = 0;
        var buffer = new byte[200];
        Assert.Equal(200, stream.Read(buffer, 0, 200));
        Assert.Equal(data, buffer);
    }

    [Fact]
    public void ReadAcrossDifferentlySizedSegmentsFromOffsetIsContiguous()
    {
        var pool = new TrackingArrayPool();
        using var stream = new ArrayPoolMemoryStream(16, pool: pool);
        var data = Sequence(1048);
        stream.Write(data, 0, 16);
        stream.Write(data, 16, 32);
        stream.Write(data, 48, 1000);
        Assert.Equal(new[] { 16, 32, 1000 }, pool.RentRequests);

        stream.Position = 8;
        var buffer = new byte[1040];
        Assert.Equal(1040, stream.Read(buffer, 0, 1040));
        Assert.Equal(data.AsSpan(8).ToArray(), buffer);
    }

    [Fact]
    public void WriteAcrossDifferentlySizedSegmentsIsContiguous()
    {
        var pool = new TrackingArrayPool();
        using var stream = new ArrayPoolMemoryStream(16, pool: pool);
        stream.Write(Sequence(16), 0, 16);
        stream.Write(Sequence(48), 0, 48);
        Assert.Equal(new[] { 16, 48 }, pool.RentRequests);

        var data = Sequence(64);
        stream.Position = 0;
        stream.Write(data, 0, 64);
        Assert.Equal(new[] { 16, 48 }, pool.RentRequests);

        stream.Position = 0;
        var buffer = new byte[64];
        Assert.Equal(64, stream.Read(buffer, 0, 64));
        Assert.Equal(data, buffer);
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
    public void SetLengthRejectsNegativeValues()
    {
        using var stream = StreamWith(1, 2, 3);
        Assert.Throws<ArgumentOutOfRangeException>(() => stream.SetLength(-1));
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
    public void SetLengthTruncationClampsPosition()
    {
        using var stream = StreamWith(1, 2, 3, 4, 5);
        stream.Position = 5;
        stream.SetLength(2);
        Assert.Equal(2L, stream.Position);
    }

    [Fact]
    public void SetLengthGrowsTheStream()
    {
        using var stream = StreamWith(1, 2, 3);
        stream.SetLength(10);
        Assert.Equal(10L, stream.Length);
    }

    [Fact]
    public void SetLengthGrowthLeavesPositionAlone()
    {
        using var stream = StreamWith(1, 2, 3);
        stream.Position = 1;
        stream.SetLength(10);
        Assert.Equal(1L, stream.Position);
    }

    [Fact]
    public void SetLengthGrowthBeyondCapacityRentsMoreMemory()
    {
        using var stream = new ArrayPoolMemoryStream(16);
        stream.SetLength(5000);
        Assert.Equal(5000L, stream.Length);
        Assert.True(stream.Capacity >= 5000);
    }

    [Fact]
    public void SetLengthGrowthExposesZeroedBytes()
    {
        using var stream = StreamWith(1, 2, 3);
        stream.SetLength(6);
        stream.Position = 0;
        var buffer = new byte[6];
        Assert.Equal(6, stream.Read(buffer, 0, 6));
        Assert.Equal(new byte[] { 1, 2, 3, 0, 0, 0 }, buffer);
    }

    [Fact]
    public void SetLengthGrowthZeroesBytesDiscardedByAnEarlierTruncation()
    {
        using var stream = StreamWith(1, 2, 3, 4, 5);
        stream.SetLength(2);
        stream.SetLength(5);
        stream.Position = 0;
        var buffer = new byte[5];
        Assert.Equal(5, stream.Read(buffer, 0, 5));
        Assert.Equal(new byte[] { 1, 2, 0, 0, 0 }, buffer);
    }

    [Fact]
    public void SetLengthGrowthAcrossASegmentBoundaryZeroesEverythingExposed()
    {
        var pool = new TrackingArrayPool(0xFF);
        using var stream = new ArrayPoolMemoryStream(16, pool: pool);
        stream.Write(Sequence(16), 0, 16);
        stream.SetLength(40);
        Assert.Equal(new[] { 16, 24 }, pool.RentRequests);

        stream.Position = 16;
        var buffer = new byte[24];
        Assert.Equal(24, stream.Read(buffer, 0, 24));
        Assert.Equal(new byte[24], buffer);
    }

    [Fact]
    public void SetLengthGrowthSkipsZeroingWhenAskedTo()
    {
        var pool = new TrackingArrayPool(0xFF);
        using var stream = new ArrayPoolMemoryStream(64, skipZeroing: true, pool: pool);
        stream.Write(new byte[] { 1, 2, 3 }, 0, 3);
        stream.SetLength(8);
        stream.Position = 0;
        var buffer = new byte[8];
        Assert.Equal(8, stream.Read(buffer, 0, 8));
        Assert.Equal(new byte[] { 1, 2, 3, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF }, buffer);
    }

    [Fact]
    public void WriteAfterSeekingPastEndZeroesTheGap()
    {
        var pool = new TrackingArrayPool(0xFF);
        using var stream = new ArrayPoolMemoryStream(64, pool: pool);
        stream.Write(new byte[] { 1, 2 }, 0, 2);
        stream.Position = 5;
        stream.WriteByte(9);
        stream.Position = 0;
        var buffer = new byte[6];
        Assert.Equal(6, stream.Read(buffer, 0, 6));
        Assert.Equal(new byte[] { 1, 2, 0, 0, 0, 9 }, buffer);
    }

    [Fact]
    public void WriteAfterSeekingPastEndSkipsZeroingWhenAskedTo()
    {
        var pool = new TrackingArrayPool(0xFF);
        using var stream = new ArrayPoolMemoryStream(64, skipZeroing: true, pool: pool);
        stream.Write(new byte[] { 1, 2 }, 0, 2);
        stream.Position = 5;
        stream.WriteByte(9);
        stream.Position = 0;
        var buffer = new byte[6];
        Assert.Equal(6, stream.Read(buffer, 0, 6));
        Assert.Equal(new byte[] { 1, 2, 0xFF, 0xFF, 0xFF, 9 }, buffer);
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
        using var stream = new ArrayPoolMemoryStream(32);
        var data = Sequence(150);
        for (var written = 0; written < data.Length; written += 25)
            stream.Write(data, written, 25);

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

    [Fact]
    public void DisposeReturnsEverySegmentExactlyOnce()
    {
        var pool = new TrackingArrayPool();
        var stream = new ArrayPoolMemoryStream(16, pool: pool);
        stream.Write(Sequence(16), 0, 16);
        stream.Write(Sequence(1000), 0, 1000);
        Assert.Equal(2, pool.Rented.Count);

        stream.Dispose();
        stream.Dispose();

        Assert.Equal(pool.Rented.Count, pool.Returns.Count);
        for (var i = 0; i < pool.Rented.Count; i++)
            Assert.Same(pool.Rented[i], pool.Returns[i]);
    }
}
