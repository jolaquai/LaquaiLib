using System.Buffers;
using System.Runtime.InteropServices;

using LaquaiLib.Extensions;

namespace LaquaiLib.UnitTests.Extensions;

public class ArrayPoolExtensionsTests
{
    private struct StructWithReference
    {
        public int A;
        public string S;
    }

    private struct ThreeInts
    {
        public int A;
        public int B;
        public int C;
    }

    private sealed class RecordingArrayPool<T> : ArrayPool<T>
    {
        public T[] LastRentedArray;
        public int? LastRentMinimumLength;
        public T[] LastReturnedArray;
        public bool? LastReturnedClearArray;
        public bool ReturnWasCalled;

        public override T[] Rent(int minimumLength)
        {
            LastRentMinimumLength = minimumLength;
            LastRentedArray = new T[minimumLength];
            return LastRentedArray;
        }

        public override void Return(T[] array, bool clearArray = false)
        {
            ReturnWasCalled = true;
            LastReturnedArray = array;
            LastReturnedClearArray = clearArray;
        }
    }

    [Fact]
    public void ReturnSafeDoesNothingForNullArray()
    {
        var pool = new RecordingArrayPool<string>();

        pool.ReturnSafe(null);

        Assert.False(pool.ReturnWasCalled);
    }

    [Fact]
    public void ReturnSafeClearsArrayForReferenceTypeElements()
    {
        var pool = new RecordingArrayPool<string>();
        var array = new string[4];

        pool.ReturnSafe(array);

        Assert.True(pool.ReturnWasCalled);
        Assert.Same(array, pool.LastReturnedArray);
        Assert.True(pool.LastReturnedClearArray);
    }

    [Fact]
    public void ReturnSafeClearsArrayForStructsContainingReferences()
    {
        var pool = new RecordingArrayPool<StructWithReference>();
        var array = new StructWithReference[4];

        pool.ReturnSafe(array);

        Assert.True(pool.ReturnWasCalled);
        Assert.True(pool.LastReturnedClearArray);
    }

    [Fact]
    public void ReturnSafeDoesNotClearArrayForPureValueTypeElements()
    {
        var pool = new RecordingArrayPool<int>();
        var array = new int[4];

        pool.ReturnSafe(array);

        Assert.True(pool.ReturnWasCalled);
        Assert.False(pool.LastReturnedClearArray);
    }

    [Fact]
    public void ReturnSafeWorksWithSharedPool()
    {
        var array = ArrayPool<byte>.Shared.Rent(16);

        ArrayPool<byte>.Shared.ReturnSafe(array);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(7)]
    [InlineData(100)]
    [InlineData(4096)]
    public void RentByteSpanCoversEntireRentedArray(int minimumSize)
    {
        var array = ArrayPool<byte>.Shared.Rent<byte>(minimumSize, out var span);

        Assert.True(array.Length >= minimumSize);
        Assert.Equal(array.Length, span.Length);

        ArrayPool<byte>.Shared.Return(array);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(100)]
    [InlineData(4096)]
    public void RentCharSpanFitsMaximallyWithinRentedBytes(int minimumSize)
    {
        var array = ArrayPool<byte>.Shared.Rent<char>(minimumSize, out var span);

        Assert.True(span.Length >= minimumSize);
        Assert.True(span.Length * sizeof(char) <= array.Length);
        Assert.True((span.Length + 1) * sizeof(char) > array.Length);

        ArrayPool<byte>.Shared.Return(array);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(5)]
    [InlineData(10)]
    [InlineData(127)]
    [InlineData(1000)]
    public void RentGuidSpanFitsMaximallyWithinRentedBytes(int minimumSize)
    {
        var array = ArrayPool<byte>.Shared.Rent<Guid>(minimumSize, out var span);

        Assert.True(span.Length >= minimumSize);
        Assert.True(span.Length * Unsafe.SizeOf<Guid>() <= array.Length);
        Assert.True((span.Length + 1) * Unsafe.SizeOf<Guid>() > array.Length);

        ArrayPool<byte>.Shared.Return(array);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(7)]
    [InlineData(13)]
    [InlineData(100)]
    [InlineData(337)]
    public void RentThreeIntsSpanFitsMaximallyWithinRentedBytes(int minimumSize)
    {
        var array = ArrayPool<byte>.Shared.Rent<ThreeInts>(minimumSize, out var span);

        Assert.True(span.Length >= minimumSize);
        Assert.True(span.Length * Unsafe.SizeOf<ThreeInts>() <= array.Length);
        Assert.True((span.Length + 1) * Unsafe.SizeOf<ThreeInts>() > array.Length);

        ArrayPool<byte>.Shared.Return(array);
    }

    [Fact]
    public void RentSpanIsALiveViewOverTheRentedArray()
    {
        var array = ArrayPool<byte>.Shared.Rent<int>(4, out var span);

        span[0] = 123456789;

        var readBack = MemoryMarshal.Cast<byte, int>(array.AsSpan())[0];
        Assert.Equal(123456789, readBack);

        ArrayPool<byte>.Shared.Return(array);
    }

    [Fact]
    public void RentWithZeroMinimumSizeReturnsEmptyArrayAndSpan()
    {
        var array = ArrayPool<byte>.Shared.Rent<byte>(0, out var span);

        Assert.Empty(array);
        Assert.Equal(0, span.Length);
    }

    [Fact]
    public void RentWithZeroMinimumSizeWorksForLargerElementType()
    {
        var array = ArrayPool<byte>.Shared.Rent<Guid>(0, out var span);

        Assert.Empty(array);
        Assert.Equal(0, span.Length);
    }

    [Fact]
    public void RentThrowsWhenEffectiveSizeExceedsArrayMaxLength()
    {
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => ArrayPool<byte>.Shared.Rent<byte>(Array.MaxLength + 1, out _));

        Assert.Equal("minimumSize", ex.ParamName);
    }

    [Fact]
    public void RentThrowsForNegativeMinimumSize()
        => Assert.Throws<ArgumentOutOfRangeException>(() => ArrayPool<byte>.Shared.Rent<byte>(-1, out _));

    [Fact]
    public void RentThrowsWhenElementSizeMultiplicationOverflows()
        => Assert.Throws<ArgumentOutOfRangeException>(() => ArrayPool<byte>.Shared.Rent<Guid>(200_000_000, out _));

    [Fact]
    public void RentPassesEffectiveByteSizeToUnderlyingPool()
    {
        var pool = new RecordingArrayPool<byte>();

        var array = pool.Rent<char>(10, out var span);

        Assert.Equal(20, pool.LastRentMinimumLength);
        Assert.Same(pool.LastRentedArray, array);
        Assert.True(span.Length >= 10);
    }

    [Fact]
    public void RentedArrayCanBeReturnedThroughReturnSafe()
    {
        var array = ArrayPool<byte>.Shared.Rent<int>(8, out var span);

        span[0] = 42;

        ArrayPool<byte>.Shared.ReturnSafe(array);
    }
}
