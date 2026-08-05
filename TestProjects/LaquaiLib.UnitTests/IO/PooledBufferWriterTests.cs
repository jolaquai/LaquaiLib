using System.Buffers;

using LaquaiLib.IO;

namespace LaquaiLib.UnitTests.IO;

public class PooledBufferWriterTests
{
    private sealed class TrackingArrayPool<T> : ArrayPool<T>
    {
        public List<int> RentRequests { get; } = [];
        public List<(T[] Array, bool Cleared)> Returns { get; } = [];

        public override T[] Rent(int minimumLength)
        {
            RentRequests.Add(minimumLength);
            return new T[minimumLength];
        }
        public override void Return(T[] array, bool clearArray = false)
        {
            Returns.Add((array, clearArray));
            if (clearArray)
                Array.Clear(array);
        }
    }

    private static PooledBufferWriter<byte> WriterWith(params byte[] data)
    {
        var writer = new PooledBufferWriter<byte>();
        data.CopyTo(writer.GetSpan(data.Length));
        writer.Advance(data.Length);
        return writer;
    }

    [Fact]
    public void FreshWriterReportsZeroLength()
    {
        using var writer = new PooledBufferWriter<byte>();
        Assert.Equal(0L, writer.AbsoluteLength);
    }

    [Fact]
    public void FreshWriterProducesEmptyArray()
    {
        using var writer = new PooledBufferWriter<byte>();
        Assert.Empty(writer.ToArray());
    }

    [Fact]
    public void FreshWriterCanBeCleared()
    {
        using var writer = new PooledBufferWriter<byte>();
        writer.Clear();
        Assert.Equal(0L, writer.AbsoluteLength);
    }

    [Fact]
    public void FreshWriterAcceptsSetLengthZero()
    {
        using var writer = new PooledBufferWriter<byte>();
        writer.SetLength(0);
        Assert.Equal(0L, writer.AbsoluteLength);
    }

    [Fact]
    public void AdvanceBeforeAnyBufferRequestThrows()
    {
        using var writer = new PooledBufferWriter<byte>();
        Assert.Throws<InvalidOperationException>(() => writer.Advance(1));
    }

    [Fact]
    public void FreshWriterEnumeratesNoSegments()
    {
        using var writer = new PooledBufferWriter<byte>();
        var enumerator = writer.GetSegments();
        Assert.False(enumerator.MoveNext());
    }

    [Fact]
    public void GetSpanReturnsWritableBuffer()
    {
        using var writer = new PooledBufferWriter<byte>();
        var span = writer.GetSpan();
        Assert.False(span.IsEmpty);
        span[0] = 1;
        writer.Advance(1);
        Assert.Equal(1L, writer.AbsoluteLength);
    }

    [Fact]
    public void GetSpanHonorsSizeHintLargerThanDefaultSegment()
    {
        using var writer = new PooledBufferWriter<byte>();
        Assert.True(writer.GetSpan(8192).Length >= 8192);
    }

    [Fact]
    public void GetMemoryHonorsSizeHintLargerThanRemainder()
    {
        using var writer = new PooledBufferWriter<byte>();
        var initial = writer.GetSpan().Length;
        writer.Advance(initial - 1);
        Assert.True(writer.GetMemory(64).Length >= 64);
    }

    [Fact]
    public void GetSpanReturnsRemainderOfCurrentSegmentAfterAdvance()
    {
        using var writer = new PooledBufferWriter<byte>();
        var initial = writer.GetSpan().Length;
        writer.Advance(3);
        Assert.Equal(initial - 3, writer.GetSpan().Length);
    }

    [Fact]
    public void RepeatedGetSpanWithoutAdvanceIsStable()
    {
        using var writer = new PooledBufferWriter<byte>();
        var first = writer.GetSpan().Length;
        Assert.Equal(first, writer.GetSpan().Length);
        Assert.Equal(first, writer.GetSpan().Length);
        Assert.Equal(0L, writer.AbsoluteLength);
    }

    [Fact]
    public void WriteAndAdvanceRoundTripThroughToArray()
    {
        using var writer = WriterWith(1, 2, 3, 4);
        Assert.Equal(4L, writer.AbsoluteLength);
        Assert.Equal(new byte[] { 1, 2, 3, 4 }, writer.ToArray());
    }

    [Fact]
    public void SuccessiveWritesAccumulate()
    {
        using var writer = new PooledBufferWriter<byte>();
        new byte[] { 1, 2 }.CopyTo(writer.GetSpan(2));
        writer.Advance(2);
        new byte[] { 3, 4, 5 }.CopyTo(writer.GetSpan(3));
        writer.Advance(3);
        Assert.Equal(5L, writer.AbsoluteLength);
        Assert.Equal(new byte[] { 1, 2, 3, 4, 5 }, writer.ToArray());
    }

    [Fact]
    public void WritesSpanningSegmentsPreserveOrder()
    {
        using var writer = new PooledBufferWriter<byte>();
        var first = writer.GetSpan();
        var firstLength = first.Length;
        for (var i = 0; i < firstLength; i++)
            first[i] = (byte)(i & 0xFF);
        writer.Advance(firstLength);

        var second = writer.GetSpan();
        second[0] = 0xAB;
        second[1] = 0xCD;
        writer.Advance(2);

        var array = writer.ToArray();
        Assert.Equal(firstLength + 2, array.Length);
        for (var i = 0; i < firstLength; i++)
            Assert.Equal((byte)(i & 0xFF), array[i]);
        Assert.Equal(0xAB, array[firstLength]);
        Assert.Equal(0xCD, array[firstLength + 1]);
    }

    [Fact]
    public void AbsoluteLengthTracksWritesAcrossSegments()
    {
        using var writer = new PooledBufferWriter<byte>();
        var firstLength = writer.GetSpan().Length;
        writer.Advance(firstLength);
        Assert.Equal(firstLength, writer.AbsoluteLength);
        writer.GetSpan();
        writer.Advance(7);
        Assert.Equal(firstLength + 7L, writer.AbsoluteLength);
    }

    [Fact]
    public void PositionTracksSegmentAndOffset()
    {
        using var writer = new PooledBufferWriter<byte>();
        writer.GetSpan();
        writer.Advance(4);
        var position = writer.Position;
        Assert.Equal(0, position.SegmentIndex);
        Assert.Equal(4, position.Offset);
        Assert.False(position.Segment.IsEmpty);
    }

    [Fact]
    public void PositionMovesToNextSegmentWhenCurrentIsFull()
    {
        using var writer = new PooledBufferWriter<byte>();
        writer.Advance(writer.GetSpan().Length);
        writer.GetSpan();
        var position = writer.Position;
        Assert.Equal(1, position.SegmentIndex);
        Assert.Equal(0, position.Offset);
    }

    [Fact]
    public void AdvancePastSegmentEndThrows()
    {
        using var writer = new PooledBufferWriter<byte>();
        var length = writer.GetSpan().Length;
        Assert.Throws<ArgumentOutOfRangeException>(() => writer.Advance(length + 1));
    }

    [Fact]
    public void AdvanceWithNegativeCountThrows()
    {
        using var writer = new PooledBufferWriter<byte>();
        writer.GetSpan();
        Assert.Throws<ArgumentOutOfRangeException>(() => writer.Advance(-1));
    }

    [Fact]
    public void AdvanceZeroIsNoOp()
    {
        using var writer = WriterWith(1, 2);
        writer.Advance(0);
        Assert.Equal(2L, writer.AbsoluteLength);
    }

    [Fact]
    public void SetLengthTruncatesWrittenData()
    {
        using var writer = WriterWith(1, 2, 3, 4, 5);
        writer.SetLength(2);
        Assert.Equal(2L, writer.AbsoluteLength);
        Assert.Equal(new byte[] { 1, 2 }, writer.ToArray());
    }

    [Fact]
    public void SetLengthToZeroDiscardsEverything()
    {
        using var writer = WriterWith(1, 2, 3);
        writer.SetLength(0);
        Assert.Equal(0L, writer.AbsoluteLength);
        Assert.Empty(writer.ToArray());
    }

    [Fact]
    public void SetLengthBeyondCurrentLengthThrows()
    {
        using var writer = WriterWith(1, 2, 3);
        Assert.Throws<ArgumentOutOfRangeException>(() => writer.SetLength(4));
    }

    [Fact]
    public void SetLengthToCurrentLengthIsNoOp()
    {
        using var writer = WriterWith(1, 2, 3);
        writer.SetLength(3);
        Assert.Equal(3L, writer.AbsoluteLength);
        Assert.Equal(new byte[] { 1, 2, 3 }, writer.ToArray());
    }

    [Fact]
    public void WritingAfterTruncationOverwritesTail()
    {
        using var writer = WriterWith(1, 2, 3, 4, 5);
        writer.SetLength(2);
        new byte[] { 9, 8 }.CopyTo(writer.GetSpan(2));
        writer.Advance(2);
        Assert.Equal(4L, writer.AbsoluteLength);
        Assert.Equal(new byte[] { 1, 2, 9, 8 }, writer.ToArray());
    }

    [Fact]
    public void SetLengthRewindsIntoAnEarlierSegment()
    {
        using var writer = new PooledBufferWriter<byte>();
        var first = writer.GetSpan();
        first.Fill(1);
        writer.Advance(first.Length);
        var second = writer.GetSpan();
        second[0] = 2;
        writer.Advance(1);

        writer.SetLength(2);
        var span = writer.GetSpan(3);
        span[0] = 9;
        span[1] = 8;
        span[2] = 7;
        writer.Advance(3);

        Assert.Equal(5L, writer.AbsoluteLength);
        Assert.Equal(new byte[] { 1, 1, 9, 8, 7 }, writer.ToArray());
    }

    [Fact]
    public void ClearResetsLengthWithoutReleasingSegments()
    {
        using var writer = WriterWith(1, 2, 3);
        writer.Clear();
        Assert.Equal(0L, writer.AbsoluteLength);
        Assert.Empty(writer.ToArray());
        var enumerator = writer.GetSegments();
        Assert.True(enumerator.MoveNext());
    }

    [Fact]
    public void ClearWithZeroErasesWrittenData()
    {
        using var writer = WriterWith(1, 2, 3);
        writer.Clear(true);
        var span = writer.GetSpan(3);
        Assert.Equal(0, span[0]);
        Assert.Equal(0, span[1]);
        Assert.Equal(0, span[2]);
    }

    [Fact]
    public void ClearWithoutZeroLeavesDataInPlace()
    {
        using var writer = WriterWith(1, 2, 3);
        writer.Clear();
        Assert.Equal(1, writer.GetSpan(3)[0]);
    }

    [Fact]
    public void GetSegmentsEnumeratesEveryRentedSegment()
    {
        using var writer = new PooledBufferWriter<byte>();
        writer.Advance(writer.GetSpan().Length);
        writer.Advance(writer.GetSpan().Length);
        writer.GetSpan();

        var count = 0;
        var enumerator = writer.GetSegments();
        while (enumerator.MoveNext())
        {
            Assert.False(enumerator.Current.IsEmpty);
            count++;
        }
        Assert.Equal(3, count);
    }

    [Fact]
    public void SegmentEnumeratorMoveNextStaysFalseAtEnd()
    {
        using var writer = WriterWith(1);
        var enumerator = writer.GetSegments();
        Assert.True(enumerator.MoveNext());
        Assert.False(enumerator.MoveNext());
        Assert.False(enumerator.MoveNext());
    }

    [Fact]
    public void SegmentEnumeratorResetRestartsIteration()
    {
        using var writer = WriterWith(1);
        var enumerator = writer.GetSegments();
        Assert.True(enumerator.MoveNext());
        Assert.False(enumerator.MoveNext());
        enumerator.Reset();
        Assert.True(enumerator.MoveNext());
    }

    [Fact]
    public void SegmentEnumeratorExposesUntypedCurrent()
    {
        using var writer = WriterWith(1);
        IEnumerator enumerator = writer.GetSegments();
        Assert.True(enumerator.MoveNext());
        Assert.IsType<Memory<byte>>(enumerator.Current);
    }

    [Fact]
    public void ToArrayReturnsIndependentCopy()
    {
        using var writer = WriterWith(5, 6);
        var array = writer.ToArray();
        array[0] = 99;
        Assert.Equal(5, writer.ToArray()[0]);
    }

    [Fact]
    public void ToArrayIgnoresUnwrittenTailOfSegment()
    {
        using var writer = new PooledBufferWriter<byte>();
        writer.GetSpan().Fill(7);
        writer.Advance(2);
        Assert.Equal(new byte[] { 7, 7 }, writer.ToArray());
    }

    [Fact]
    public void SegmentSizeScalesWithElementSize()
    {
        using var bytes = new PooledBufferWriter<byte>();
        using var ints = new PooledBufferWriter<int>();
        using var longs = new PooledBufferWriter<long>();
        Assert.True(bytes.GetSpan().Length >= 2048);
        Assert.True(ints.GetSpan().Length >= 512);
        Assert.True(longs.GetSpan().Length >= 256);
    }

    [Fact]
    public void WriterSupportsReferenceTypeElements()
    {
        using var writer = new PooledBufferWriter<string>();
        var span = writer.GetSpan(2);
        span[0] = "a";
        span[1] = "b";
        writer.Advance(2);
        Assert.Equal(new[] { "a", "b" }, writer.ToArray());
    }

    [Fact]
    public void WriterRentsFromSuppliedPool()
    {
        var pool = new TrackingArrayPool<byte>();
        using var writer = new PooledBufferWriter<byte>(pool);
        writer.GetSpan();
        Assert.Single(pool.RentRequests);
    }

    [Fact]
    public void DisposeReturnsEverySegmentToPool()
    {
        var pool = new TrackingArrayPool<byte>();
        var writer = new PooledBufferWriter<byte>(pool);
        writer.Advance(writer.GetSpan().Length);
        writer.GetSpan();
        writer.Dispose();
        Assert.Equal(2, pool.Returns.Count);
    }

    [Fact]
    public void DisposeClearsSegmentsWhenZeroOnDisposeIsSet()
    {
        var pool = new TrackingArrayPool<byte>();
        var writer = new PooledBufferWriter<byte>(pool, true);
        var span = writer.GetSpan();
        span[0] = 42;
        writer.Advance(1);
        writer.Dispose();
        Assert.All(pool.Returns, entry => Assert.True(entry.Cleared));
        Assert.All(pool.Returns, entry => Assert.Equal(0, entry.Array[0]));
    }

    [Fact]
    public void DisposeDoesNotClearSegmentsByDefault()
    {
        var pool = new TrackingArrayPool<byte>();
        var writer = new PooledBufferWriter<byte>(pool);
        var span = writer.GetSpan();
        span[0] = 42;
        writer.Advance(1);
        writer.Dispose();
        Assert.All(pool.Returns, entry => Assert.False(entry.Cleared));
        Assert.Equal(42, pool.Returns[0].Array[0]);
    }

    [Fact]
    public void DisposeIsIdempotent()
    {
        var pool = new TrackingArrayPool<byte>();
        var writer = new PooledBufferWriter<byte>(pool);
        writer.GetSpan();
        writer.Dispose();
        writer.Dispose();
        Assert.Single(pool.Returns);
    }

    [Fact]
    public void GetMemoryAfterDisposeThrows()
    {
        var pool = new TrackingArrayPool<byte>();
        var writer = new PooledBufferWriter<byte>(pool);
        writer.GetSpan();
        writer.Dispose();
        Assert.Throws<ObjectDisposedException>(() => { writer.GetMemory(); });
    }

    [Fact]
    public void ToArrayAfterDisposeThrows()
    {
        var pool = new TrackingArrayPool<byte>();
        var writer = new PooledBufferWriter<byte>(pool);
        var span = writer.GetSpan();
        span[0] = 1;
        writer.Advance(1);
        writer.Dispose();
        Assert.Throws<ObjectDisposedException>(() => { writer.ToArray(); });
    }

    [Fact]
    public void WriterIsUsableThroughIBufferWriterInterface()
    {
        using var writer = new PooledBufferWriter<byte>();
        IBufferWriter<byte> buffered = writer;
        var payload = new byte[6000];
        for (var i = 0; i < payload.Length; i++)
            payload[i] = (byte)(i & 0xFF);

        var written = 0;
        while (written < payload.Length)
        {
            var span = buffered.GetSpan(1);
            var toWrite = Math.Min(span.Length, payload.Length - written);
            payload.AsSpan(written, toWrite).CopyTo(span);
            buffered.Advance(toWrite);
            written += toWrite;
        }

        Assert.Equal(payload, writer.ToArray());
    }

    [Fact]
    public void LargeSizeHintRentsASingleContiguousBuffer()
    {
        var pool = new TrackingArrayPool<byte>();
        using var writer = new PooledBufferWriter<byte>(pool);
        var span = writer.GetSpan(10000);
        Assert.True(span.Length >= 10000);
        Assert.Single(pool.RentRequests);
    }
}
