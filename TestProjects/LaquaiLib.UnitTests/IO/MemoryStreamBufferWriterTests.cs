using LaquaiLib.Extensions;
using LaquaiLib.IO;

namespace LaquaiLib.UnitTests.IO;

public class MemoryStreamBufferWriterTests
{
    [Fact]
    public void ParameterlessCtorProducesUsableWriter()
    {
        using var writer = new MemoryStreamBufferWriter();
        Assert.NotNull(writer.BaseStream);
        var memory = writer.GetMemory();
        Assert.True(memory.Length >= 1);
        Assert.Equal(256, writer.BaseStream.Capacity);
    }

    [Fact]
    public void WriteThenAdvanceRoundTrips()
    {
        using var writer = new MemoryStreamBufferWriter();
        var span = writer.GetMemory(4).Span;
        span[0] = 1;
        span[1] = 2;
        span[2] = 3;
        span[3] = 4;
        writer.Advance(4);

        Assert.Equal(4, writer.BaseStream.Length);
        Assert.Equal(4, writer.BaseStream.Position);
        Assert.Equal(new byte[] { 1, 2, 3, 4 }, writer.BaseStream.ToArray());
    }

    [Fact]
    public void MultipleWriteCyclesForceReallocationAndPreserveData()
    {
        using var writer = new MemoryStreamBufferWriter();
        var expected = new List<byte>();
        byte value = 0;
        foreach (var chunk in new[] { 10, 200, 100, 500, 1000 })
        {
            var span = writer.GetMemory(chunk).Span;
            for (var i = 0; i < chunk; i++)
            {
                span[i] = value;
                expected.Add(value);
                value++;
            }
            writer.Advance(chunk);
        }

        Assert.Equal(expected.ToArray(), writer.BaseStream.ToArray());
    }

    [Fact]
    public void GetSpanMatchesGetMemory()
    {
        using var writer = new MemoryStreamBufferWriter();
        var span = writer.GetSpan(10);
        span[0] = 42;
        writer.Advance(1);
        Assert.Equal(42, writer.BaseStream.ToArray()[0]);
    }

    [Fact]
    public void InterleavedStreamAndWriterWritesConcatenateInOrder()
    {
        using var writer = new MemoryStreamBufferWriter();
        writer.BaseStream.Write([1, 2]);

        var span = writer.GetMemory(2).Span;
        span[0] = 3;
        span[1] = 4;
        writer.Advance(2);

        writer.BaseStream.Write([5, 6]);

        Assert.Equal(new byte[] { 1, 2, 3, 4, 5, 6 }, writer.BaseStream.ToArray());
    }

    [Fact]
    public void WrappingPrepopulatedStreamAppendsFromCurrentPosition()
    {
        var stream = new MemoryStream();
        stream.Write([1, 2, 3, 4]);
        stream.Position = 2;

        using var writer = new MemoryStreamBufferWriter(stream);
        Assert.Equal(2, writer.Length);

        var span = writer.GetMemory(2).Span;
        span[0] = 9;
        span[1] = 8;
        writer.Advance(2);

        Assert.Equal(new byte[] { 1, 2, 9, 8 }, stream.ToArray());
    }

    [Fact]
    public void SizeHintLargerThanDoublingStepYieldsSufficientSpan()
    {
        using var writer = new MemoryStreamBufferWriter();
        var memory = writer.GetMemory(5000);
        Assert.True(memory.Length >= 5000);
    }

    [Fact]
    public void AdvancePastEndOfBufferThrows()
    {
        using var writer = new MemoryStreamBufferWriter();
        var memory = writer.GetMemory(4);
        Assert.Throws<ArgumentOutOfRangeException>(() => writer.Advance(memory.Length + 1));
    }

    [Fact]
    public void AdvanceNegativeThrows()
    {
        using var writer = new MemoryStreamBufferWriter();
        writer.GetMemory();
        Assert.Throws<ArgumentOutOfRangeException>(() => writer.Advance(-1));
    }

    [Fact]
    public void GetMemoryNegativeSizeHintThrows()
    {
        using var writer = new MemoryStreamBufferWriter();
        Assert.Throws<ArgumentOutOfRangeException>(() => writer.GetMemory(-1));
    }

    [Fact]
    public void SetLengthTruncatesAndAdjustsPosition()
    {
        using var writer = new MemoryStreamBufferWriter();
        var span = writer.GetMemory(5).Span;
        span[0] = 1;
        span[1] = 2;
        span[2] = 3;
        span[3] = 4;
        span[4] = 5;
        writer.Advance(5);

        writer.SetLength(2);
        Assert.Equal(2, writer.BaseStream.Length);
        Assert.Equal(2, writer.BaseStream.Position);

        Assert.Throws<ArgumentOutOfRangeException>(() => writer.SetLength(3));
        Assert.Throws<ArgumentOutOfRangeException>(() => writer.SetLength(-1));
    }

    [Fact]
    public void SetLengthClampsPositionWhenBelowCurrentPosition()
    {
        using var writer = new MemoryStreamBufferWriter();
        var span = writer.GetMemory(5).Span;
        span.ZeroMemory();
        writer.Advance(5);
        writer.BaseStream.Position = 5;

        writer.SetLength(2);
        Assert.Equal(2, writer.BaseStream.Position);
    }

    [Fact]
    public void ClearWithoutZeroEmptiesStreamButLeavesBytes()
    {
        using var writer = new MemoryStreamBufferWriter();
        var span = writer.GetMemory(3).Span;
        span[0] = 1;
        span[1] = 2;
        span[2] = 3;
        writer.Advance(3);

        writer.Clear(false);
        Assert.Equal(0, writer.BaseStream.Length);
        Assert.Equal(1, writer.BaseStream.AsSpan()[0]);
    }

    [Fact]
    public void ClearWithZeroErasesWrittenBytes()
    {
        using var writer = new MemoryStreamBufferWriter();
        var span = writer.GetMemory(3).Span;
        span[0] = 1;
        span[1] = 2;
        span[2] = 3;
        writer.Advance(3);

        writer.Clear(true);
        Assert.Equal(0, writer.BaseStream.Length);
        Assert.Equal(0, writer.BaseStream.AsSpan()[0]);
        Assert.Equal(0, writer.BaseStream.AsSpan()[1]);
        Assert.Equal(0, writer.BaseStream.AsSpan()[2]);
    }

    [Fact]
    public void LeaveOpenTrueKeepsStreamUsableAfterDispose()
    {
        var stream = new MemoryStream();
        var writer = new MemoryStreamBufferWriter(stream, leaveOpen: true);
        writer.Dispose();
        stream.WriteByte(1);
        Assert.Equal(1, stream.Length);
        stream.Dispose();
    }

    [Fact]
    public void LeaveOpenFalseDisposesStream()
    {
        var stream = new MemoryStream();
        var writer = new MemoryStreamBufferWriter(stream);
        writer.Dispose();
        Assert.Throws<ObjectDisposedException>(() => stream.WriteByte(1));
    }

    [Fact]
    public void DoubleDisposeDoesNotThrow()
    {
        var writer = new MemoryStreamBufferWriter();
        writer.Dispose();
        writer.Dispose();
    }

    [Fact]
    public void PostDisposeGetMemoryThrows()
    {
        var writer = new MemoryStreamBufferWriter();
        writer.Dispose();
        Assert.Throws<ObjectDisposedException>(() => writer.GetMemory());
    }

    [Fact]
    public void PostDisposeSetLengthThrows()
    {
        var writer = new MemoryStreamBufferWriter();
        writer.Dispose();
        Assert.Throws<ObjectDisposedException>(() => writer.SetLength(0));
    }

    [Fact]
    public void PostDisposeClearThrows()
    {
        var writer = new MemoryStreamBufferWriter();
        writer.Dispose();
        Assert.Throws<ObjectDisposedException>(() => writer.Clear());
    }

    [Fact]
    public void NonWritableStreamCtorThrows()
    {
        var bytes = new byte[16];
        var stream = new MemoryStream(bytes, writable: false);
        Assert.Throws<ArgumentException>(() => new MemoryStreamBufferWriter(stream));
    }

    [Fact]
    public void NullStreamCtorThrows()
    {
        Assert.Throws<ArgumentNullException>(() => new MemoryStreamBufferWriter(null));
    }

    [Fact]
    public void NonExpandableStreamFillsThenThrowsOnOverflow()
    {
        var bytes = new byte[16];
        var stream = new MemoryStream(bytes);
        using var writer = new MemoryStreamBufferWriter(stream);

        var span = writer.GetMemory(16).Span;
        Assert.Equal(16, span.Length);
        span.Fill(7);
        writer.Advance(16);

        Assert.All(bytes, b => Assert.Equal(7, b));
        Assert.Throws<NotSupportedException>(() => writer.GetMemory(1));
    }

    [Fact]
    public void OffsetConstructedStreamRespectsOriginAndBounds()
    {
        var array = new byte[20];
        Array.Fill(array, (byte)0xAA);
        var stream = new MemoryStream(array, 4, 8);
        using var writer = new MemoryStreamBufferWriter(stream);

        Assert.Equal(0, writer.Length);

        var memory = writer.GetMemory();
        Assert.Equal(8, memory.Length);

        var span = memory.Span;
        for (var i = 0; i < 8; i++)
            span[i] = (byte)(i + 1);
        writer.Advance(8);

        Assert.Equal(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }, array[4..12]);
        Assert.True(array[0..4].All(b => b == 0xAA));
        Assert.True(array[12..20].All(b => b == 0xAA));

        Assert.Throws<NotSupportedException>(() => writer.GetMemory(9));
    }

    [Fact]
    public void SeekingPastLengthThenWritingZeroesTheGap()
    {
        var stream = new MemoryStream();
        stream.Write([0xFF, 0xFF, 0xFF, 0xFF]);
        stream.Position = 0;

        using var writer = new MemoryStreamBufferWriter(stream);
        writer.Clear(false);
        Assert.Equal(0, stream.Length);

        stream.Position = 4;
        var span = writer.GetMemory(2).Span;
        span[0] = 1;
        span[1] = 2;
        writer.Advance(2);

        var result = stream.ToArray();
        Assert.Equal(new byte[] { 0, 0, 0, 0, 1, 2 }, result);
    }
}
