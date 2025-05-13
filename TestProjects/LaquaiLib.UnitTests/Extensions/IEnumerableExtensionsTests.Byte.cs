using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using LaquaiLib.Extensions;

namespace LaquaiLib.UnitTests.Extensions;

public class IEnumerableExtensionsByteTests
{
    [Fact]
    public void IntoStructConvertsArrayToStruct()
    {
        var value = 42;
        var bytes = BitConverter.GetBytes(value);
        var result = bytes.IntoStruct<int>();

        Assert.Equal(value, result);
    }

    [Fact]
    public void IntoStructConvertsListToStruct()
    {
        var guid = Guid.NewGuid();
        var bytes = guid.ToByteArray().ToList();
        var result = bytes.IntoStruct<Guid>();

        Assert.Equal(guid, result);
    }

    [Fact]
    public void IntoStructConvertsCustomEnumerableToStruct()
    {
        var value = 123.456f;
        var bytes = BitConverter.GetBytes(value);
        var customEnumerable = new CustomByteCollection(bytes);
        var result = customEnumerable.IntoStruct<float>();

        Assert.Equal(value, result);
    }

    [Fact]
    public void IntoStructWorksWithCustomStructType()
    {
        var testStruct = new TestStruct { X = 42, Y = 12345 };
        var bytes = StructToBytes(testStruct);
        var result = bytes.IntoStruct<TestStruct>();

        Assert.Equal(testStruct.X, result.X);
        Assert.Equal(testStruct.Y, result.Y);
    }

    [Fact]
    public void IntoStructHandlesLargeStructs()
    {
        var largeStruct = new LargeStruct
        {
            Value1 = 42,
            Value2 = 12345,
            Guid = Guid.NewGuid(),
            Value3 = 98765
        };

        var bytes = StructToBytes(largeStruct);
        var result = bytes.IntoStruct<LargeStruct>();

        Assert.Equal(largeStruct.Value1, result.Value1);
        Assert.Equal(largeStruct.Value2, result.Value2);
        Assert.Equal(largeStruct.Guid, result.Guid);
        Assert.Equal(largeStruct.Value3, result.Value3);
    }

    private static byte[] StructToBytes<T>(T value) where T : struct
    {
        var size = Unsafe.SizeOf<T>();
        var arr = new byte[size];
        var handle = GCHandle.Alloc(arr, GCHandleType.Pinned);
        try
        {
            Marshal.StructureToPtr(value, handle.AddrOfPinnedObject(), false);
            return arr;
        }
        finally
        {
            handle.Free();
        }
    }

    private class CustomByteCollection(byte[] bytes) : IEnumerable<byte>
    {
        private readonly byte[] _bytes = bytes;

        public IEnumerator<byte> GetEnumerator() => ((IEnumerable<byte>)_bytes).GetEnumerator();

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => _bytes.GetEnumerator();
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct TestStruct
    {
        public int X;
        public long Y;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct LargeStruct
    {
        public int Value1;
        public long Value2;
        public Guid Guid;
        public long Value3;
    }
}
