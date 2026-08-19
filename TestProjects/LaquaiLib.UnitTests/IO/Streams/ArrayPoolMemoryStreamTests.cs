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

    // real pools round up to bucket sizes; that surplus is what makes segment merging tricky, so it needs to be reproducible
    private sealed class RoundingArrayPool(byte fill = 0) : ArrayPool<byte>
    {
        public List<int> RentRequests { get; } = [];
        public List<byte[]> Rented { get; } = [];
        public List<byte[]> Returns { get; } = [];

        private static int RoundUp(int minimumLength)
        {
            var size = 16;
            while (size < minimumLength)
                size <<= 1;
            return size;
        }

        public override byte[] Rent(int minimumLength)
        {
            var array = new byte[RoundUp(minimumLength)];
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

    private static ArrayPoolMemoryStream ThreeSegmentStream(TrackingArrayPool pool, byte[] data)
    {
        var stream = new ArrayPoolMemoryStream(16, pool: pool);
        for (var offset = 0; offset < 48; offset += 16)
            stream.Write(data, offset, 16);
        return stream;
    }

    [Fact]
    public void TrimExcessReleasesSegmentsBeyondLength()
    {
        var pool = new TrackingArrayPool();
        using var stream = ThreeSegmentStream(pool, Sequence(48));
        stream.SetLength(20);
        stream.TrimExcess();

        Assert.Single(pool.Returns);
        Assert.Equal(32L, stream.Capacity);
    }

    [Fact]
    public void TrimExcessKeepsTheSegmentContainingLength()
    {
        var pool = new TrackingArrayPool();
        using var stream = ThreeSegmentStream(pool, Sequence(48));
        stream.SetLength(17);
        stream.TrimExcess();
        Assert.True(stream.Capacity > stream.Length);
    }

    [Fact]
    public void TrimExcessAtASegmentBoundaryLeavesNoSlack()
    {
        var pool = new TrackingArrayPool();
        using var stream = ThreeSegmentStream(pool, Sequence(48));
        stream.SetLength(32);
        stream.TrimExcess();
        Assert.Equal(32L, stream.Capacity);
    }

    [Fact]
    public void TrimExcessAfterFullTruncationReleasesEverything()
    {
        var pool = new TrackingArrayPool();
        using var stream = ThreeSegmentStream(pool, Sequence(48));
        stream.SetLength(0);
        stream.TrimExcess();

        Assert.Equal(3, pool.Returns.Count);
        Assert.Equal(0L, stream.Capacity);
    }

    [Fact]
    public void TrimExcessWithNothingToReleaseIsANoOp()
    {
        var pool = new TrackingArrayPool();
        using var stream = ThreeSegmentStream(pool, Sequence(48));
        stream.TrimExcess();

        Assert.Empty(pool.Returns);
        Assert.Equal(48L, stream.Capacity);
    }

    [Fact]
    public void TrimExcessPreservesReadableContent()
    {
        var pool = new TrackingArrayPool();
        var data = Sequence(48);
        using var stream = ThreeSegmentStream(pool, data);
        stream.SetLength(20);
        stream.TrimExcess();

        stream.Position = 0;
        var buffer = new byte[20];
        Assert.Equal(20, stream.Read(buffer, 0, 20));
        Assert.Equal(data.AsSpan(0, 20).ToArray(), buffer);
    }

    [Fact]
    public void WriteAfterTrimExcessRentsAgainAndStaysCorrect()
    {
        var pool = new TrackingArrayPool();
        var data = Sequence(48);
        using var stream = ThreeSegmentStream(pool, data);
        stream.SetLength(20);
        stream.TrimExcess();

        var tail = new byte[20];
        Array.Fill(tail, (byte)7);
        stream.Position = 20;
        stream.Write(tail, 0, 20);
        Assert.Equal(4, pool.Rented.Count);

        var expected = new byte[40];
        data.AsSpan(0, 20).CopyTo(expected);
        tail.CopyTo(expected.AsSpan(20));

        stream.Position = 0;
        var buffer = new byte[40];
        Assert.Equal(40, stream.Read(buffer, 0, 40));
        Assert.Equal(expected, buffer);
    }

    [Fact]
    public void DisposeAfterTrimExcessDoesNotReturnSegmentsTwice()
    {
        var pool = new TrackingArrayPool();
        var stream = ThreeSegmentStream(pool, Sequence(48));
        stream.SetLength(20);
        stream.TrimExcess();
        stream.Dispose();

        Assert.Equal(pool.Rented.Count, pool.Returns.Count);
        Assert.Equal(pool.Returns.Count, pool.Returns.Distinct().Count());
        foreach (var rented in pool.Rented)
            Assert.True(pool.Returns.Any(returned => ReferenceEquals(returned, rented)));
    }

    [Fact]
    public void TrimExcessThrowsWhenDisposed()
    {
        var stream = StreamWith(1, 2, 3);
        stream.Dispose();
        Assert.Throws<ObjectDisposedException>(stream.TrimExcess);
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
    public void ReadAsyncArrayOverloadRejectsNullBuffer()
    {
        using var stream = StreamWith(1, 2, 3);
        Assert.Throws<ArgumentNullException>(() => { _ = stream.ReadAsync(null, 0, 1, default); });
    }

    [Fact]
    public void WriteAsyncArrayOverloadRejectsNullBuffer()
    {
        using var stream = new ArrayPoolMemoryStream();
        Assert.Throws<ArgumentNullException>(() => { _ = stream.WriteAsync(null, 0, 1, default); });
    }

    [Fact]
    public void ReadAsyncArrayOverloadValidatesBeforeObservingCancellation()
    {
        using var stream = StreamWith(1, 2, 3);
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        Assert.ThrowsAny<ArgumentException>(() => { _ = stream.ReadAsync(new byte[4], 2, 5, cts.Token); });
    }

    [Fact]
    public void WriteAsyncArrayOverloadValidatesBeforeObservingCancellation()
    {
        using var stream = new ArrayPoolMemoryStream();
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        Assert.ThrowsAny<ArgumentException>(() => { _ = stream.WriteAsync(new byte[4], 2, 5, cts.Token); });
    }

    [Fact]
    public void CopyToAsyncRejectsNullDestinationSynchronously()
    {
        using var stream = StreamWith(1, 2, 3);
        Assert.Throws<ArgumentNullException>(() => { _ = stream.CopyToAsync(null, 4096, default); });
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

    private static ArrayPoolMemoryStream DisposedStream()
    {
        var stream = StreamWith(1, 2, 3);
        stream.Dispose();
        return stream;
    }

    [Fact]
    public void DisposedStreamReportsNoCapabilities()
    {
        var stream = DisposedStream();
        Assert.False(stream.CanRead);
        Assert.False(stream.CanSeek);
        Assert.False(stream.CanWrite);
    }

    [Fact]
    public void ReadThrowsWhenDisposed() => Assert.Throws<ObjectDisposedException>(() => DisposedStream().Read(new byte[3], 0, 3));

    [Fact]
    public void ReadSpanThrowsWhenDisposed()
    {
        var stream = DisposedStream();
        Assert.Throws<ObjectDisposedException>(() => stream.Read(new byte[3].AsSpan()));
    }

    [Fact]
    public void ReadByteThrowsWhenDisposed() => Assert.Throws<ObjectDisposedException>(() => DisposedStream().ReadByte());

    [Fact]
    public void WriteThrowsWhenDisposed() => Assert.Throws<ObjectDisposedException>(() => DisposedStream().Write(new byte[3], 0, 3));

    [Fact]
    public void WriteByteThrowsWhenDisposed() => Assert.Throws<ObjectDisposedException>(() => DisposedStream().WriteByte(1));

    [Fact]
    public void SetLengthThrowsWhenDisposed() => Assert.Throws<ObjectDisposedException>(() => DisposedStream().SetLength(1));

    [Fact]
    public void PositionSetterThrowsWhenDisposed() => Assert.Throws<ObjectDisposedException>(() => DisposedStream().Position = 1);

    [Fact]
    public void SeekThrowsWhenDisposed() => Assert.Throws<ObjectDisposedException>(() => DisposedStream().Seek(0, SeekOrigin.Begin));

    [Fact]
    public void CopyToThrowsWhenDisposed()
    {
        var stream = DisposedStream();
        using var target = new MemoryStream();
        Assert.Throws<ObjectDisposedException>(() => stream.CopyTo(target, 4096));
    }

    [Fact]
    public void CopyToAsyncThrowsWhenDisposed()
    {
        var stream = DisposedStream();
        using var target = new MemoryStream();
        Assert.Throws<ObjectDisposedException>(() => { _ = stream.CopyToAsync(target, 4096, default); });
    }

    [Fact]
    public void ReadRejectsNullBuffer()
    {
        using var stream = StreamWith(1, 2, 3);
        Assert.Throws<ArgumentNullException>(() => stream.Read(null, 0, 1));
    }

    [Fact]
    public void ReadRejectsCountBeyondBuffer()
    {
        using var stream = StreamWith(1, 2, 3);
        Assert.ThrowsAny<ArgumentException>(() => stream.Read(new byte[4], 2, 5));
    }

    [Fact]
    public void WriteRejectsNullBuffer()
    {
        using var stream = new ArrayPoolMemoryStream();
        Assert.Throws<ArgumentNullException>(() => stream.Write(null, 0, 1));
    }

    [Fact]
    public void WriteRejectsNegativeOffset()
    {
        using var stream = new ArrayPoolMemoryStream();
        Assert.ThrowsAny<ArgumentException>(() => stream.Write(new byte[4], -1, 2));
    }

    [Fact]
    public void CopyToRejectsUnwritableDestination()
    {
        using var stream = StreamWith(1, 2, 3);
        using var target = new MemoryStream(new byte[8], false);
        Assert.Throws<NotSupportedException>(() => stream.CopyTo(target, 4096));
    }

    [Fact]
    public void CopyToAsyncRejectsUnwritableDestinationSynchronously()
    {
        using var stream = StreamWith(1, 2, 3);
        using var target = new MemoryStream(new byte[8], false);
        Assert.Throws<NotSupportedException>(() => { _ = stream.CopyToAsync(target, 4096, default); });
    }

    [Fact]
    public void CopyToRejectsClosedDestination()
    {
        using var stream = StreamWith(1, 2, 3);
        var target = new MemoryStream();
        target.Dispose();
        Assert.Throws<ObjectDisposedException>(() => stream.CopyTo(target, 4096));
    }

    [Fact]
    public void WriteSpanOverloadAppendsBytes()
    {
        using var stream = new ArrayPoolMemoryStream();
        stream.Write([1, 2, 3]);
        Assert.Equal(3L, stream.Length);

        stream.Position = 0;
        var buffer = new byte[3];
        Assert.Equal(3, stream.Read(buffer, 0, 3));
        Assert.Equal(new byte[] { 1, 2, 3 }, buffer);
    }

    private static ArrayPoolMemoryStream ThreeUnevenSegmentStream(TrackingArrayPool pool, byte[] data)
    {
        var stream = new ArrayPoolMemoryStream(16, pool: pool);
        stream.Write(data, 0, 16);
        stream.Write(data, 16, 32);
        stream.Write(data, 48, 1000);
        return stream;
    }

    [Fact]
    public void CopyToWalksEverySegment()
    {
        var pool = new TrackingArrayPool();
        var data = Sequence(1048);
        using var stream = ThreeUnevenSegmentStream(pool, data);
        Assert.Equal(new[] { 16, 32, 1000 }, pool.RentRequests);

        stream.Position = 0;
        using var target = new MemoryStream();
        stream.CopyTo(target);
        Assert.Equal(data, target.ToArray());
        Assert.Equal(1048L, stream.Position);
    }

    [Fact]
    public async Task CopyToAsyncWalksEverySegment()
    {
        var pool = new TrackingArrayPool();
        var data = Sequence(1048);
        using var stream = ThreeUnevenSegmentStream(pool, data);
        Assert.Equal(new[] { 16, 32, 1000 }, pool.RentRequests);

        stream.Position = 0;
        using var target = new MemoryStream();
        await stream.CopyToAsync(target);
        Assert.Equal(data, target.ToArray());
        Assert.Equal(1048L, stream.Position);
    }

    [Fact]
    public void CopyToFromMidSegmentWalksTheRemainder()
    {
        var pool = new TrackingArrayPool();
        var data = Sequence(1048);
        using var stream = ThreeUnevenSegmentStream(pool, data);

        stream.Position = 8;
        using var target = new MemoryStream();
        stream.CopyTo(target);
        Assert.Equal(data.AsSpan(8).ToArray(), target.ToArray());
    }

    [Fact]
    public void CopyToAtEndOfStreamWritesNothing()
    {
        using var stream = StreamWith(1, 2, 3);
        stream.Position = 3;
        using var target = new MemoryStream();
        stream.CopyTo(target);
        Assert.Empty(target.ToArray());
    }

    [Fact]
    public async Task CopyToAsyncAtEndOfStreamWritesNothing()
    {
        using var stream = StreamWith(1, 2, 3);
        stream.Position = 3;
        using var target = new MemoryStream();
        await stream.CopyToAsync(target);
        Assert.Empty(target.ToArray());
    }

    [Fact]
    public async Task ReadAsyncArrayOverloadHonoursCancellation()
    {
        using var stream = StreamWith(1, 2, 3);
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => stream.ReadAsync(new byte[3], 0, 3, cts.Token));
    }

    [Fact]
    public async Task ReadAsyncMemoryOverloadHonoursCancellation()
    {
        using var stream = StreamWith(1, 2, 3);
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => stream.ReadAsync(new byte[3].AsMemory(), cts.Token).AsTask());
    }

    [Fact]
    public async Task WriteAsyncArrayOverloadHonoursCancellation()
    {
        using var stream = new ArrayPoolMemoryStream();
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => stream.WriteAsync(new byte[3], 0, 3, cts.Token));
    }

    [Fact]
    public async Task WriteAsyncMemoryOverloadHonoursCancellation()
    {
        using var stream = new ArrayPoolMemoryStream();
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await stream.WriteAsync(new byte[3].AsMemory(), cts.Token));
    }

    [Fact]
    public async Task ReadAsyncReusesTheCompletedTaskForRepeatedCounts()
    {
        using var stream = StreamWith(1, 2, 3, 4);
        var first = stream.ReadAsync(new byte[2], 0, 2, default);
        var second = stream.ReadAsync(new byte[2], 0, 2, default);
        Assert.Same(first, second);
        Assert.Equal(2, await first);
    }

    [Fact]
    public async Task ReadAsyncIssuesAFreshTaskWhenTheCountChanges()
    {
        using var stream = StreamWith(1, 2, 3);
        var first = stream.ReadAsync(new byte[2], 0, 2, default);
        var second = stream.ReadAsync(new byte[2], 0, 2, default);
        Assert.NotSame(first, second);
        Assert.Equal(2, await first);
        Assert.Equal(1, await second);
    }

    [Fact]
    public void GetSpanWithZeroSizeHintReturnsNonEmptyBuffer()
    {
        using var stream = new ArrayPoolMemoryStream();
        Assert.True(stream.GetSpan(0).Length >= 1);
    }

    [Fact]
    public void GetMemoryWithZeroSizeHintReturnsNonEmptyBuffer()
    {
        using var stream = new ArrayPoolMemoryStream();
        Assert.True(stream.GetMemory(0).Length >= 1);
    }

    [Fact]
    public void GetSpanRejectsNegativeSizeHint()
    {
        using var stream = new ArrayPoolMemoryStream();
        Assert.Throws<ArgumentOutOfRangeException>(() => stream.GetSpan(-1));
    }

    [Fact]
    public void GetMemoryRejectsNegativeSizeHint()
    {
        using var stream = new ArrayPoolMemoryStream();
        Assert.Throws<ArgumentOutOfRangeException>(() => stream.GetMemory(-1));
    }

    [Fact]
    public void GetSpanHonoursSizeHintFarLargerThanMinimumSegmentSize()
    {
        using var stream = new ArrayPoolMemoryStream(16);
        Assert.True(stream.GetSpan(5000).Length >= 5000);
    }

    [Fact]
    public void GetSpanStartsAtPositionAndExposesExistingContent()
    {
        using var stream = StreamWith(1, 2, 3, 4, 5);
        stream.Position = 1;
        var span = stream.GetSpan(3);
        Assert.True(span.Length >= 3);
        Assert.Equal(new byte[] { 2, 3, 4 }, span[..3].ToArray());
    }

    [Fact]
    public void GetSpanAndGetMemoryAliasTheSameBytes()
    {
        using var stream = new ArrayPoolMemoryStream();
        stream.Write(new byte[] { 1, 2, 3 }, 0, 3);
        var span = stream.GetSpan(4);
        span[0] = 42;
        var memory = stream.GetMemory(4);
        Assert.Equal(42, memory.Span[0]);
    }

    [Fact]
    public void GetSpanDoesNotChangePositionOrLength()
    {
        using var stream = StreamWith(1, 2, 3);
        stream.Position = 1;
        stream.GetSpan(10);
        Assert.Equal(1L, stream.Position);
        Assert.Equal(3L, stream.Length);
    }

    [Fact]
    public void GetMemoryDoesNotChangePositionOrLength()
    {
        using var stream = StreamWith(1, 2, 3);
        stream.Position = 1;
        stream.GetMemory(10);
        Assert.Equal(1L, stream.Position);
        Assert.Equal(3L, stream.Length);
    }

    [Fact]
    public void GetSpanThrowsWhenDisposed() => Assert.Throws<ObjectDisposedException>(() => DisposedStream().GetSpan(1));

    [Fact]
    public void GetMemoryThrowsWhenDisposed() => Assert.Throws<ObjectDisposedException>(() => DisposedStream().GetMemory(1));

    [Fact]
    public void AdvanceThrowsWhenDisposed() => Assert.Throws<ObjectDisposedException>(() => DisposedStream().Advance(1));

    [Fact]
    public void AdvanceRejectsNegativeCount()
    {
        using var stream = new ArrayPoolMemoryStream();
        Assert.Throws<ArgumentOutOfRangeException>(() => stream.Advance(-1));
    }

    [Fact]
    public void AdvanceZeroIsNoOp()
    {
        using var stream = StreamWith(1, 2, 3);
        stream.Advance(0);
        Assert.Equal(3L, stream.Length);
        Assert.Equal(0L, stream.Position);
    }

    [Fact]
    public void AdvanceExtendsLength()
    {
        using var stream = new ArrayPoolMemoryStream();
        stream.GetSpan(10);
        stream.Advance(5);
        Assert.Equal(5L, stream.Length);
    }

    [Fact]
    public void AdvanceMovesPosition()
    {
        using var stream = new ArrayPoolMemoryStream();
        stream.GetSpan(10);
        stream.Advance(5);
        Assert.Equal(5L, stream.Position);
    }

    [Fact]
    public void AdvancePastCapacityThrows()
    {
        // the requested capacity is a floor, not the real one, so the boundary has to come from Capacity itself
        using var stream = new ArrayPoolMemoryStream(capacity: 10);
        Assert.Throws<InvalidOperationException>(() => stream.Advance((int)stream.Capacity + 1));
    }

    [Fact]
    public void AdvanceExactlyToCapacityIsAllowed()
    {
        using var stream = new ArrayPoolMemoryStream(capacity: 10);
        stream.Advance((int)stream.Capacity);
        Assert.Equal(stream.Capacity, stream.Position);
        Assert.Equal(stream.Capacity, stream.Length);
    }

    [Fact]
    public void AdvanceInsideExistingContentDoesNotShrinkLength()
    {
        using var stream = StreamWith(1, 2, 3, 4, 5);
        stream.Position = 0;
        stream.Advance(2);
        Assert.Equal(5L, stream.Length);
        Assert.Equal(2L, stream.Position);
    }

    [Fact]
    public void GetSpanAndAdvanceRoundTripWritesAndReadsCorrectly()
    {
        using var stream = new ArrayPoolMemoryStream();
        var data = Sequence(50);
        var span = stream.GetSpan(50);
        data.CopyTo(span);
        stream.Advance(50);

        stream.Position = 0;
        var buffer = new byte[50];
        Assert.Equal(50, stream.Read(buffer, 0, 50));
        Assert.Equal(data, buffer);
    }

    [Fact]
    public void AdvanceAfterSeekingPastEndZeroesTheGapWithoutPriorGetSpan()
    {
        var pool = new TrackingArrayPool(0xCD);
        using var stream = new ArrayPoolMemoryStream(capacity: 64, pool: pool);
        stream.Position = 5;
        stream.Advance(1);

        stream.Position = 0;
        var buffer = new byte[6];
        Assert.Equal(6, stream.Read(buffer, 0, 6));
        Assert.Equal(new byte[] { 0, 0, 0, 0, 0, 0xCD }, buffer);
    }

    [Fact]
    public void AdvanceAfterSeekingPastEndSkipsZeroingWhenAskedTo()
    {
        var pool = new TrackingArrayPool(0xCD);
        using var stream = new ArrayPoolMemoryStream(capacity: 64, skipZeroing: true, pool: pool);
        stream.Position = 5;
        stream.Advance(1);

        stream.Position = 0;
        var buffer = new byte[6];
        Assert.Equal(6, stream.Read(buffer, 0, 6));
        Assert.Equal(new byte[] { 0xCD, 0xCD, 0xCD, 0xCD, 0xCD, 0xCD }, buffer);
    }

    [Fact]
    public void GetSpanAcrossSegmentsConsolidatesAndPreservesContent()
    {
        var pool = new TrackingArrayPool();
        var data = Sequence(64);
        using var stream = new ArrayPoolMemoryStream(16, pool: pool);
        for (var offset = 0; offset < data.Length; offset += 16)
            stream.Write(data, offset, 16);
        Assert.True(pool.RentRequests.Count >= 4);

        stream.Position = 0;
        var span = stream.GetSpan(64);
        Assert.True(span.Length >= 64);
        Assert.Equal(data, span[..64].ToArray());
    }

    [Fact]
    public void GetSpanConsolidationReturnsMergedSegmentsToThePool()
    {
        var pool = new TrackingArrayPool();
        var data = Sequence(64);
        using var stream = new ArrayPoolMemoryStream(16, pool: pool);
        for (var offset = 0; offset < data.Length; offset += 16)
            stream.Write(data, offset, 16);
        var rentedBeforeConsolidation = pool.Rented.ToList();

        stream.Position = 0;
        stream.GetSpan(64);

        Assert.True(pool.Returns.Count > 0);
        foreach (var returned in pool.Returns)
            Assert.Contains(returned, rentedBeforeConsolidation);
    }

    private static ArrayPoolMemoryStream MultiSegmentStream(ArrayPool<byte> pool, byte[] data, int chunk = 20)
    {
        var stream = new ArrayPoolMemoryStream(16, pool: pool);
        for (var offset = 0; offset < data.Length; offset += chunk)
        {
            var take = Math.Min(chunk, data.Length - offset);
            stream.Write(data, offset, take);
        }
        return stream;
    }

    [Fact]
    public void GetSpanConsolidationOfNonTailRunPreservesContentWithRoundingPool()
    {
        var pool = new RoundingArrayPool();
        var data = Sequence(200);
        using var stream = MultiSegmentStream(pool, data);

        stream.Position = 0;
        var hint = (int)(stream.Capacity / 3);
        Assert.True(hint > 0 && hint < stream.Capacity);
        var span = stream.GetSpan(hint);
        Assert.True(span.Length >= hint);

        stream.Position = 0;
        var buffer = new byte[stream.Length];
        Assert.Equal(buffer.Length, stream.Read(buffer, 0, buffer.Length));
        Assert.Equal(data, buffer);
    }

    [Fact]
    public void GetSpanConsolidationOfNonTailRunPreservesContentWithSharedPool()
    {
        var data = Sequence(200);
        using var stream = MultiSegmentStream(ArrayPool<byte>.Shared, data);

        stream.Position = 0;
        var hint = (int)(stream.Capacity / 3);
        Assert.True(hint > 0 && hint < stream.Capacity);
        var span = stream.GetSpan(hint);
        Assert.True(span.Length >= hint);

        stream.Position = 0;
        var buffer = new byte[stream.Length];
        Assert.Equal(buffer.Length, stream.Read(buffer, 0, buffer.Length));
        Assert.Equal(data, buffer);
    }

    [Fact]
    public void CapacityInvariantHoldsAfterConsolidationWithTrackingPool()
    {
        var pool = new TrackingArrayPool();
        var data = Sequence(200);
        using var stream = MultiSegmentStream(pool, data);

        stream.Position = 0;
        stream.GetSpan((int)(stream.Capacity / 3));

        var expected = pool.Rented.Sum(a => (long)a.Length) - pool.Returns.Sum(a => (long)a.Length);
        Assert.Equal(expected, stream.Capacity);
    }

    [Fact]
    public void CapacityInvariantHoldsAfterConsolidationWithRoundingPool()
    {
        var pool = new RoundingArrayPool();
        var data = Sequence(200);
        using var stream = MultiSegmentStream(pool, data);

        stream.Position = 0;
        stream.GetSpan((int)(stream.Capacity / 3));

        var expected = pool.Rented.Sum(a => (long)a.Length) - pool.Returns.Sum(a => (long)a.Length);
        Assert.Equal(expected, stream.Capacity);
    }

    [Fact]
    public void RepeatedGetSpanWithSameHintDoesNotKeepRenting()
    {
        var pool = new TrackingArrayPool();
        using var stream = new ArrayPoolMemoryStream(16, pool: pool);
        stream.GetSpan(40);
        var countAfterFirst = pool.RentRequests.Count;
        stream.GetSpan(40);
        stream.GetSpan(40);
        Assert.Equal(countAfterFirst, pool.RentRequests.Count);
    }

    [Fact]
    public void BufferWriterWritesAreVisibleThroughRead()
    {
        using var stream = new ArrayPoolMemoryStream();
        var data = Sequence(30);
        var span = stream.GetSpan(30);
        data.CopyTo(span);
        stream.Advance(30);

        stream.Position = 0;
        var buffer = new byte[30];
        Assert.Equal(30, stream.Read(buffer, 0, 30));
        Assert.Equal(data, buffer);
    }

    [Fact]
    public void SeekToMiddleThenGetSpanAdvanceOverwritesInPlaceAndLeavesSurroundingBytesIntact()
    {
        using var stream = StreamWith(1, 2, 3, 4, 5, 6, 7, 8);
        stream.Position = 3;
        var span = stream.GetSpan(2);
        span[0] = 100;
        span[1] = 101;
        stream.Advance(2);

        stream.Position = 0;
        var buffer = new byte[8];
        Assert.Equal(8, stream.Read(buffer, 0, 8));
        Assert.Equal(new byte[] { 1, 2, 3, 100, 101, 6, 7, 8 }, buffer);
        Assert.Equal(8L, stream.Length);
    }

    [Fact]
    public void SetLengthTruncationAfterAdvance()
    {
        using var stream = new ArrayPoolMemoryStream();
        var data = Sequence(10);
        var span = stream.GetSpan(10);
        data.CopyTo(span);
        stream.Advance(10);
        stream.SetLength(4);
        Assert.Equal(4L, stream.Length);

        stream.Position = 0;
        var buffer = new byte[4];
        Assert.Equal(4, stream.Read(buffer, 0, 4));
        Assert.Equal(data.AsSpan(0, 4).ToArray(), buffer);
    }

    [Fact]
    public void CopyToAfterBufferWriterWrites()
    {
        using var stream = new ArrayPoolMemoryStream();
        var data = Sequence(20);
        data.CopyTo(stream.GetSpan(20));
        stream.Advance(20);

        stream.Position = 0;
        using var target = new MemoryStream();
        stream.CopyTo(target);
        Assert.Equal(data, target.ToArray());
    }

    [Fact]
    public void TrimExcessAfterBufferWriterUsageStillLeavesContentReadable()
    {
        var pool = new TrackingArrayPool();
        using var stream = new ArrayPoolMemoryStream(16, pool: pool);
        var data = Sequence(48);
        for (var offset = 0; offset < data.Length; offset += 16)
        {
            var span = stream.GetSpan(16);
            data.AsSpan(offset, 16).CopyTo(span);
            stream.Advance(16);
        }
        stream.SetLength(20);
        stream.TrimExcess();

        stream.Position = 0;
        var buffer = new byte[20];
        Assert.Equal(20, stream.Read(buffer, 0, 20));
        Assert.Equal(data.AsSpan(0, 20).ToArray(), buffer);
    }

    [Fact]
    public void BufferWriterOperationsMatchMemoryStreamOracle()
    {
        var random = new Random(12345);
        using var stream = new ArrayPoolMemoryStream(16);
        using var oracle = new MemoryStream();

        for (var iteration = 0; iteration < 300; iteration++)
        {
            switch (random.Next(4))
            {
                case 0:
                {
                    var hint = random.Next(0, 40);
                    var span = stream.GetSpan(hint);
                    var writeLength = Math.Min(random.Next(0, 41), span.Length);
                    var chunk = new byte[writeLength];
                    random.NextBytes(chunk);
                    chunk.CopyTo(span);
                    stream.Advance(writeLength);
                    oracle.Write(chunk, 0, writeLength);
                    break;
                }
                case 1:
                {
                    var maxSeek = (int)Math.Max(oracle.Length, 1);
                    var target = random.Next(0, maxSeek + 20);
                    stream.Position = target;
                    oracle.Position = target;
                    break;
                }
                case 2:
                {
                    var newLength = random.Next(0, (int)oracle.Length + 40);
                    stream.SetLength(newLength);
                    oracle.SetLength(newLength);
                    break;
                }
                case 3:
                {
                    var hint = random.Next(0, 40);
                    var memory = stream.GetMemory(hint);
                    var writeLength = Math.Min(random.Next(0, 41), memory.Length);
                    var chunk = new byte[writeLength];
                    random.NextBytes(chunk);
                    chunk.CopyTo(memory);
                    stream.Advance(writeLength);
                    oracle.Write(chunk, 0, writeLength);
                    break;
                }
            }
        }

        Assert.Equal(oracle.Length, stream.Length);

        stream.Position = 0;
        oracle.Position = 0;
        var expected = oracle.ToArray();
        var actual = new byte[expected.Length];
        Assert.Equal(actual.Length, stream.Read(actual, 0, actual.Length));
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void GetSpanAtExactlyCapacityGrowsInsteadOfThrowing()
    {
        using var stream = new ArrayPoolMemoryStream(16);
        stream.Write(Sequence(16), 0, 16);
        stream.Position = stream.Capacity;
        var before = stream.Capacity;
        Assert.True(stream.GetSpan(1).Length >= 1);
        Assert.True(stream.Capacity > before);
    }

    [Fact]
    public void WritesThroughGetMemoryAreVisibleThroughRead()
    {
        using var stream = new ArrayPoolMemoryStream(16);
        var data = Sequence(40);
        data.CopyTo(stream.GetMemory(40));
        stream.Advance(40);

        stream.Position = 0;
        var actual = new byte[40];
        Assert.Equal(40, stream.Read(actual, 0, 40));
        Assert.Equal(data, actual);
    }

    [Fact]
    public void StreamIsUsableThroughTheIBufferWriterInterface()
    {
        using var stream = new ArrayPoolMemoryStream(16);
        IBufferWriter<byte> writer = stream;
        var data = Sequence(50);
        foreach (var b in data)
        {
            writer.GetSpan()[0] = b;
            writer.Advance(1);
        }

        stream.Position = 0;
        var actual = new byte[data.Length];
        Assert.Equal(data.Length, stream.Read(actual, 0, actual.Length));
        Assert.Equal(data, actual);
    }

    [Fact]
    public void ConsolidationOfTheTrailingRunPreservesContent()
    {
        var pool = new RoundingArrayPool(0xEE);
        using var stream = new ArrayPoolMemoryStream(16, pool: pool);
        var data = Sequence(64);
        stream.Write(data, 0, data.Length);

        // start inside the first segment so the merged run runs all the way to the end of the chain
        stream.Position = 8;
        Assert.True(stream.GetSpan((int)(stream.Capacity - 8)).Length >= stream.Capacity - 8);

        stream.Position = 0;
        var actual = new byte[data.Length];
        Assert.Equal(data.Length, stream.Read(actual, 0, actual.Length));
        Assert.Equal(data, actual);
        Assert.Equal(stream.Capacity, pool.Rented.Sum(a => (long)a.Length) - pool.Returns.Sum(a => (long)a.Length));
    }
}
