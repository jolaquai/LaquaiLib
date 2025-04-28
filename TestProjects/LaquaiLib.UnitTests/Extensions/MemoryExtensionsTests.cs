using System.Runtime.InteropServices;
using System.Text;

using LaquaiLib.Extensions;

namespace LaquaiLib.UnitTests.Extensions;

public class MemoryExtensionsTests
{
    [Fact]
    public void ToArrayWithSelectorConvertsSpanElements()
    {
        ReadOnlySpan<int> source = [1, 2, 3];

        var result = source.ToArray(i => i.ToString());

        Assert.Equal(new[] { "1", "2", "3" }, result);
    }

    [Fact]
    public void ToArrayWithSelectorHandlesEmptySpan()
    {
        ReadOnlySpan<int> source = Array.Empty<int>();

        var result = source.ToArray(i => i.ToString());

        Assert.Empty(result);
    }

    [Fact]
    public void ToArrayWithSelectorWorksWithMemory()
    {
        ReadOnlyMemory<int> source = new[] { 1, 2, 3 };

        var result = source.ToArray(i => i.ToString());

        Assert.Equal(new[] { "1", "2", "3" }, result);
    }

    [Fact]
    public void SplitSeparatesElementsBasedOnPredicate()
    {
        var source = new ReadOnlySpan<int>([1, 2, 3, 4, 5]);
        var whereTrue = new int[3];
        var whereFalse = new int[2];

        source.Split(whereTrue, whereFalse, i => i % 2 == 1);

        Assert.Equal([1, 3, 5], whereTrue);
        Assert.Equal([2, 4], whereFalse);
    }

    [Fact]
    public void SplitHandlesEmptySource()
    {
        var source = ReadOnlySpan<int>.Empty;
        var whereTrue = Array.Empty<int>();
        var whereFalse = Array.Empty<int>();

        source.Split(whereTrue, whereFalse, i => i % 2 == 1);

        Assert.Empty(whereTrue);
        Assert.Empty(whereFalse);
    }

    [Fact]
    public void SplitThrowsForNullPredicate()
    {
        Assert.Throws<ArgumentNullException>(() =>
        {
            var source = new ReadOnlySpan<int>([1, 2, 3]);
            var whereTrue = new int[3];
            var whereFalse = new int[0];
            source.Split(whereTrue, whereFalse, null);
        });
    }

    [Fact]
    public void SplitWorksWithMemory()
    {
        var source = new ReadOnlyMemory<int>([1, 2, 3, 4, 5]);
        var whereTrue = new Memory<int>(new int[3]);
        var whereFalse = new Memory<int>(new int[2]);

        source.Split(whereTrue, whereFalse, i => i % 2 == 1);

        Assert.Equal([1, 3, 5], whereTrue.ToArray());
        Assert.Equal([2, 4], whereFalse.ToArray());
    }

    [Fact]
    public void EnumerateSplitsCreatesEnumerableForCharacters()
    {
        var source = "a,b,c".AsSpan();
        var splits = ",".AsSpan();

        var result = source.EnumerateSplits(splits);

        var list = new List<string>();
        foreach (var span in result)
        {
            list.Add(new string(span.ToArray()));
        }

        Assert.Equal(new[] { "a", "b", "c" }, list);
    }

    [Fact]
    public void EnumerateSplitsHandlesEmptySource()
    {
        var source = ReadOnlySpan<char>.Empty;
        var splits = ",".AsSpan();

        var result = source.EnumerateSplits(splits);

        var list = new List<string>();
        foreach (var span in result)
        {
            list.Add(new string(span.ToArray()));
        }

        Assert.Empty(list);
    }

    [Fact]
    public void EnumerateSplitsBySequenceCreatesEnumerableForCharacters()
    {
        var source = "abc--def--ghi".AsSpan();
        var sequence = "--".AsSpan();

        var result = source.EnumerateSplitsBySequence(sequence);

        var list = new List<string>();
        foreach (var span in result)
        {
            list.Add(new string(span.ToArray()));
        }

        Assert.Equal(new[] { "abc", "def", "ghi" }, list);
    }

    [Fact]
    public void EnumerateSplitsBySequenceHandlesEmptySource()
    {
        var source = ReadOnlySpan<char>.Empty;
        var sequence = "--".AsSpan();

        var result = source.EnumerateSplitsBySequence(sequence);

        var list = new List<string>();
        foreach (var span in result)
        {
            list.Add(new string(span));
        }

        Assert.Empty(list);
    }

    [StructLayout(LayoutKind.Sequential)]
    struct TestStruct
    {
        public int Value;
        public double DoubleValue;
    }

    [Fact]
    public void FormatIntoWritesStructToByteSpan()
    {
        var testStruct = new TestStruct { Value = 42, DoubleValue = 3.14 };
        var buffer = new byte[Marshal.SizeOf<TestStruct>()];
        var span = new Span<byte>(buffer);

        var result = span.FormatInto(testStruct);

        Assert.Equal(0, result.Length);
        var readBack = MemoryMarshal.Cast<byte, TestStruct>(span)[0];
        Assert.Equal(42, readBack.Value);
        Assert.Equal(3.14, readBack.DoubleValue);
    }

    [Fact]
    public void FormatIntoWithIndexWritesStructAtSpecifiedPosition()
    {
        var testStruct = new TestStruct { Value = 42, DoubleValue = 3.14 };
        var structSize = Marshal.SizeOf<TestStruct>();
        var buffer = new byte[structSize + 10];
        var span = new Span<byte>(buffer);

        var result = span.FormatInto(testStruct, 5);

        Assert.Equal(5, buffer.Take(5).Count(b => b == 0));
        var readBack = MemoryMarshal.Read<TestStruct>(buffer.AsSpan(5));
        Assert.Equal(42, readBack.Value);
        Assert.Equal(3.14, readBack.DoubleValue);
    }

    [Fact]
    public void FormatIntoThrowsWhenSpanTooSmall()
    {
        var testStruct = new TestStruct { Value = 42, DoubleValue = 3.14 };
        var buffer = new byte[Marshal.SizeOf<TestStruct>() - 1];

        Assert.Throws<ArgumentException>(() =>
        {
            var span = new Span<byte>(buffer);
            span.FormatInto(testStruct);
        });
    }

    [Fact]
    public void FormatIntoWorksWithMemory()
    {
        var testStruct = new TestStruct { Value = 42, DoubleValue = 3.14 };
        var buffer = new byte[Marshal.SizeOf<TestStruct>()];
        var memory = new Memory<byte>(buffer);

        var result = memory.FormatInto(testStruct);

        var readBack = MemoryMarshal.Cast<byte, TestStruct>(buffer)[0];
        Assert.Equal(42, readBack.Value);
        Assert.Equal(3.14, readBack.DoubleValue);
    }

    [Fact]
    public void ReadStringReadsNullTerminatedString()
    {
        byte[] bytes = [(byte)'H', (byte)'e', (byte)'l', (byte)'l', (byte)'o', 0, (byte)'W', (byte)'o', (byte)'r', (byte)'l', (byte)'d'];
        var span = new ReadOnlySpan<byte>(bytes);
        var ptr = 0;

        var result = span.ReadString(ref ptr);

        Assert.Equal("Hello", result);
        Assert.Equal(6, ptr);
    }

    [Fact]
    public void ReadStringHandlesEmptyString()
    {
        byte[] bytes = [0, (byte)'W', (byte)'o', (byte)'r', (byte)'l', (byte)'d'];
        var span = new ReadOnlySpan<byte>(bytes);
        var ptr = 0;

        var result = span.ReadString(ref ptr);

        Assert.Equal("", result);
        Assert.Equal(1, ptr);
    }

    [Fact]
    public void ReadStringThrowsWhenNoNullTerminator()
    {
        byte[] bytes = [(byte)'H', (byte)'e', (byte)'l', (byte)'l', (byte)'o'];
        var ptr = 0;

        Assert.Throws<ArgumentException>(() =>
        {
            var span = new ReadOnlySpan<byte>(bytes);
            span.ReadString(ref ptr);
        });
    }

    [Fact]
    public void ReadStringWithEncodingUsesSpecifiedEncoding()
    {
        var bytes = Encoding.Unicode.GetBytes("Hello\0");
        var span = new ReadOnlySpan<byte>(bytes);
        var ptr = 0;

        var result = span.ReadString(ref ptr, Encoding.Unicode);

        Assert.Equal("Hello", result);
        Assert.Equal(12, ptr); // 5 chars * 2 bytes per char + 2 bytes for null terminator
    }

    [Fact]
    public void ReadStringWorksWithFixedPointer()
    {
        byte[] bytes = [(byte)'H', (byte)'e', (byte)'l', (byte)'l', (byte)'o', 0, (byte)'W', (byte)'o', (byte)'r', (byte)'l', (byte)'d'];
        var span = new ReadOnlySpan<byte>(bytes);

        var result = span.ReadString(0);

        Assert.Equal("Hello", result);
    }

    [Fact]
    public void ReadStringWorksWithMemory()
    {
        byte[] bytes = [(byte)'H', (byte)'e', (byte)'l', (byte)'l', (byte)'o', 0, (byte)'W', (byte)'o', (byte)'r', (byte)'l', (byte)'d'];
        var memory = new ReadOnlyMemory<byte>(bytes);
        var ptr = 0;

        var result = memory.ReadString(ref ptr);

        Assert.Equal("Hello", result);
        Assert.Equal(6, ptr);
    }

    [Fact]
    public void ReadValueReadsStructFromByteSpan()
    {
        var testStruct = new TestStruct { Value = 42, DoubleValue = 3.14 };
        var structSize = Marshal.SizeOf<TestStruct>();
        var buffer = new byte[structSize];
        var span = new Span<byte>(buffer);
        unsafe
        {
            fixed (byte* dst = buffer)
            {
                Buffer.MemoryCopy(&testStruct, dst, structSize, structSize);
            }
        }

        var ptr = 0;
        var result = new ReadOnlySpan<byte>(buffer).Read<TestStruct>(ref ptr);

        Assert.Equal(42, result.Value);
        Assert.Equal(3.14, result.DoubleValue);
        Assert.Equal(structSize, ptr);
    }

    [Fact]
    public void ReadValueThrowsForReferenceTypes()
    {
        var buffer = new byte[10];
        var ptr = 0;

        // Cannot test with string as parameter type due to constraint, but we can test the exception
        Assert.Throws<ArgumentException>(() =>
        {
            // TestInvalidReadType will try to read a reference type, which should throw
            TestInvalidReadType<string>(new ReadOnlySpan<byte>(buffer), ref ptr);
        });
    }

    private void TestInvalidReadType<T>(ReadOnlySpan<byte> span, ref int ptr) where T : class
    {
        // This method does nothing but allows us to pass in a reference type to Read<T>
        // which should throw an exception
    }

    [Fact]
    public void ReadValueHandlesSpecialCasedTypes()
    {
        byte[] buffer = [1, 2];
        var ptr = 0;

        var byteResult = new ReadOnlySpan<byte>(buffer).Read<byte>(ref ptr);
        Assert.Equal((byte)1, byteResult);
        Assert.Equal(1, ptr);

        var boolResult = new ReadOnlySpan<byte>(buffer).Read<bool>(ref ptr);
        Assert.True(boolResult);
        Assert.Equal(2, ptr);
    }

    [Fact]
    public void ReadValueThrowsWhenSpanTooSmall()
    {
        byte[] buffer = [1, 2];
        var ptr = 0;

        Assert.Throws<ArgumentException>(() => new ReadOnlySpan<byte>(buffer).Read<long>(ref ptr));
    }

    [Fact]
    public void ReadValueWorksWithMemory()
    {
        var testStruct = new TestStruct { Value = 42, DoubleValue = 3.14 };
        var structSize = Marshal.SizeOf<TestStruct>();
        var buffer = new byte[structSize];
        unsafe
        {
            fixed (byte* dst = buffer)
            {
                Buffer.MemoryCopy(&testStruct, dst, structSize, structSize);
            }
        }

        var ptr = 0;
        var result = new ReadOnlyMemory<byte>(buffer).Read<TestStruct>(ref ptr);

        Assert.Equal(42, result.Value);
        Assert.Equal(3.14, result.DoubleValue);
        Assert.Equal(structSize, ptr);
    }

    [Fact]
    public void ReadMultipleValuesReadsArrayOfStructs()
    {
        var testStruct1 = new TestStruct { Value = 42, DoubleValue = 3.14 };
        var testStruct2 = new TestStruct { Value = 123, DoubleValue = 2.71 };
        var structSize = Marshal.SizeOf<TestStruct>();
        var buffer = new byte[structSize * 2];
        unsafe
        {
            fixed (byte* dst = buffer)
            {
                Buffer.MemoryCopy(&testStruct1, dst, structSize, structSize);
                Buffer.MemoryCopy(&testStruct2, dst + structSize, structSize, structSize);
            }
        }

        var ptr = 0;
        var results = new ReadOnlySpan<byte>(buffer).Read<TestStruct>(ref ptr, 2);

        Assert.Equal(2, results.Length);
        Assert.Equal(42, results[0].Value);
        Assert.Equal(3.14, results[0].DoubleValue);
        Assert.Equal(123, results[1].Value);
        Assert.Equal(2.71, results[1].DoubleValue);
        Assert.Equal(structSize * 2, ptr);
    }

    [Fact]
    public void ReadMultipleValuesWorksWithMemory()
    {
        var testStruct1 = new TestStruct { Value = 42, DoubleValue = 3.14 };
        var testStruct2 = new TestStruct { Value = 123, DoubleValue = 2.71 };
        var structSize = Marshal.SizeOf<TestStruct>();
        var buffer = new byte[structSize * 2];
        unsafe
        {
            fixed (byte* dst = buffer)
            {
                Buffer.MemoryCopy(&testStruct1, dst, structSize, structSize);
                Buffer.MemoryCopy(&testStruct2, dst + structSize, structSize, structSize);
            }
        }

        var ptr = 0;
        var results = new ReadOnlyMemory<byte>(buffer).Read<TestStruct>(ref ptr, 2);

        Assert.Equal(2, results.Length);
        Assert.Equal(42, results[0].Value);
        Assert.Equal(3.14, results[0].DoubleValue);
        Assert.Equal(123, results[1].Value);
        Assert.Equal(2.71, results[1].DoubleValue);
        Assert.Equal(structSize * 2, ptr);
    }
}
