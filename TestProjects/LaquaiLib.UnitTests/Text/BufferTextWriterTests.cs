using System.Globalization;
using System.Text;

using LaquaiLib.Core;
using LaquaiLib.Text;

namespace LaquaiLib.UnitTests.Text;

public class BufferTextWriterTests
{
    [Fact]
    public void ConstructorCreatesWriterWithDefaultCapacity()
    {
        var writer = new BufferTextWriter();
        Assert.Empty(writer.Span.ToArray());
    }

    [Fact]
    public void ConstructorCreatesWriterWithSpecifiedCapacity()
    {
        var writer = new BufferTextWriter(4096);
        Assert.Empty(writer.Span.ToArray());
    }

    [Fact]
    public void EncodingReturnsNull()
    {
        var writer = new BufferTextWriter();
        Assert.Null(writer.Encoding);
    }

    [Fact]
    public void FormatProviderReturnsCultureInfo()
    {
        var writer = new BufferTextWriter();
        Assert.Equal(CultureInfo.CurrentCulture, writer.FormatProvider);
    }

    [Fact]
    public void NewLineReturnsEnvironmentNewLine()
    {
        var writer = new BufferTextWriter();
        Assert.Equal(Environment.NewLine, writer.NewLine);
    }

    [Fact]
    public void NewLineCanBeModified()
    {
        var writer = new BufferTextWriter();
        writer.NewLine = "\n";
        Assert.Equal("\n", writer.NewLine);
    }

    [Fact]
    public void AsSpanReturnsWrittenCharacters()
    {
        var writer = new BufferTextWriter();
        writer.Write("Test");
        Assert.Equal("Test", new string(writer.Span));
    }

    [Fact]
    public void AsMemoryReturnsWrittenCharacters()
    {
        var writer = new BufferTextWriter();
        writer.Write("Test");
        Assert.Equal("Test", new string(writer.Memory.Span));
    }

    [Fact]
    public void FlushDoesNothing()
    {
        var writer = new BufferTextWriter();
        writer.Write("Test");
        writer.Flush();
        Assert.Equal("Test", new string(writer.Span));
    }

    [Fact]
    public async Task FlushAsyncDoesNothing()
    {
        var writer = new BufferTextWriter();
        writer.Write("Test");
        await writer.FlushAsync(TestContext.Current.CancellationToken);
        Assert.Equal("Test", new string(writer.Span));
    }

    [Fact]
    public async Task FlushAsyncWithCancellationTokenDoesNothing()
    {
        var writer = new BufferTextWriter();
        writer.Write("Test");
        await writer.FlushAsync(CancellationToken.None);
        Assert.Equal("Test", new string(writer.Span));
    }

    [Fact]
    public void ToStringReturnsWrittenCharacters()
    {
        var writer = new BufferTextWriter();
        writer.Write("Test");
        Assert.Equal("Test", writer.ToString());
    }

    [Fact]
    public void WriteBoolWritesToBuffer()
    {
        var writer = new BufferTextWriter();
        writer.Write(true);
        writer.Write(false);
        Assert.Equal("TrueFalse", writer.ToString());
    }

    [Fact]
    public void WriteCharWritesToBuffer()
    {
        var writer = new BufferTextWriter();
        writer.Write('A');
        Assert.Equal("A", writer.ToString());
    }

    [Fact]
    public void WriteCharArrayWritesToBuffer()
    {
        var writer = new BufferTextWriter();
        writer.Write(new[] { 'T', 'e', 's', 't' });
        Assert.Equal("Test", writer.ToString());
    }

    [Fact]
    public void WriteCharArrayWithIndexAndCountWritesToBuffer()
    {
        var writer = new BufferTextWriter();
        writer.Write(['H', 'e', 'l', 'l', 'o', ' ', 'W', 'o', 'r', 'l', 'd'], 0, 5);
        Assert.Equal("Hello", writer.ToString());
    }

    [Fact]
    public void WriteDecimalWritesToBuffer()
    {
        var writer = new BufferTextWriter();
        writer.Write(123.45m);
        Assert.Equal("123.45", writer.ToString());
    }

    [Fact]
    public void WriteDoubleWritesToBuffer()
    {
        var writer = new BufferTextWriter();
        writer.Write(123.45);
        Assert.Equal("123.45", writer.ToString());
    }

    [Fact]
    public void WriteIntWritesToBuffer()
    {
        var writer = new BufferTextWriter();
        writer.Write(12345);
        Assert.Equal("12345", writer.ToString());
    }

    [Fact]
    public void WriteLongWritesToBuffer()
    {
        var writer = new BufferTextWriter();
        writer.Write(12345L);
        Assert.Equal("12345", writer.ToString());
    }

    [Fact]
    public void WriteNullObjectWritesNullString()
    {
        var writer = new BufferTextWriter();
        writer.Write((object)null);
        Assert.Equal("null", writer.ToString());
    }

    [Fact]
    public void WriteObjectAsStringWritesToBuffer()
    {
        var writer = new BufferTextWriter();
        writer.Write((object)"Test");
        Assert.Equal("Test", writer.ToString());
    }

    [Fact]
    public void WriteObjectInvokesToStringWritesToBuffer()
    {
        var writer = new BufferTextWriter();
        var obj = new TestObject();
        writer.Write(obj);
        Assert.Equal("TestObject", writer.ToString());
    }

    [Fact]
    public void WriteReadOnlySpanWritesToBuffer()
    {
        var writer = new BufferTextWriter();
        writer.Write("Test".AsSpan());
        Assert.Equal("Test", writer.ToString());
    }

    [Fact]
    public void WriteFloatWritesToBuffer()
    {
        var writer = new BufferTextWriter();
        writer.Write(123.45f);
        Assert.Equal("123.45", writer.ToString());
    }

    [Fact]
    public void WriteStringWritesToBuffer()
    {
        var writer = new BufferTextWriter();
        writer.Write("Test");
        Assert.Equal("Test", writer.ToString());
    }

    [Fact]
    public void WriteEmptyStringWritesNothing()
    {
        var writer = new BufferTextWriter();
        writer.Write(string.Empty);
        Assert.Equal(string.Empty, writer.ToString());
    }

    [Fact]
    public void WriteStringWithOneArgFormatsCorrectly()
    {
        var writer = new BufferTextWriter();
        writer.Write("Hello, {0}!", "World");
        Assert.Equal("Hello, World!", writer.ToString());
    }

    [Fact]
    public void WriteStringWithTwoArgsFormatsCorrectly()
    {
        var writer = new BufferTextWriter();
        writer.Write("Hello, {0} {1}!", "Dear", "World");
        Assert.Equal("Hello, Dear World!", writer.ToString());
    }

    [Fact]
    public void WriteStringWithThreeArgsFormatsCorrectly()
    {
        var writer = new BufferTextWriter();
        writer.Write("{0}, {1} {2}!", "Hello", "Dear", "World");
        Assert.Equal("Hello, Dear World!", writer.ToString());
    }

    [Fact]
    public void WriteStringWithObjectArrayFormatsCorrectly()
    {
        var writer = new BufferTextWriter();
        writer.Write("{0}, {1} {2}!", new object[] { "Hello", "Dear", "World" });
        Assert.Equal("Hello, Dear World!", writer.ToString());
    }

    [Fact]
    public void WriteStringBuilderWritesToBuffer()
    {
        var writer = new BufferTextWriter();
        var sb = new StringBuilder();
        sb.Append("Test");
        writer.Write(sb);
        Assert.Equal("Test", writer.ToString());
    }

    [Fact]
    public void WriteUIntWritesToBuffer()
    {
        var writer = new BufferTextWriter();
        writer.Write(12345u);
        Assert.Equal("12345", writer.ToString());
    }

    [Fact]
    public void WriteULongWritesToBuffer()
    {
        var writer = new BufferTextWriter();
        writer.Write(12345ul);
        Assert.Equal("12345", writer.ToString());
    }

    [Fact]
    public void WriteLineWritesNewLine()
    {
        var writer = new BufferTextWriter();
        writer.WriteLine();
        Assert.Equal(Environment.NewLine, writer.ToString());
    }

    [Fact]
    public void WriteLineBoolWritesToBufferWithNewLine()
    {
        var writer = new BufferTextWriter();
        writer.WriteLine(true);
        Assert.Equal("True" + Environment.NewLine, writer.ToString());
    }

    [Fact]
    public void WriteLineCharWritesToBufferWithNewLine()
    {
        var writer = new BufferTextWriter();
        writer.WriteLine('A');
        Assert.Equal("A" + Environment.NewLine, writer.ToString());
    }

    [Fact]
    public void WriteLineCharArrayWritesToBufferWithNewLine()
    {
        var writer = new BufferTextWriter();
        writer.WriteLine(['T', 'e', 's', 't']);
        Assert.Equal("Test" + Environment.NewLine, writer.ToString());
    }

    [Fact]
    public void WriteLineCharArrayWithIndexAndCountWritesToBufferWithNewLine()
    {
        var writer = new BufferTextWriter();
        writer.WriteLine(['H', 'e', 'l', 'l', 'o', ' ', 'W', 'o', 'r', 'l', 'd'], 0, 5);
        Assert.Equal("Hello" + Environment.NewLine, writer.ToString());
    }

    [Fact]
    public void WriteLineDecimalWritesToBufferWithNewLine()
    {
        var writer = new BufferTextWriter();
        writer.WriteLine(123.45m);
        Assert.Equal("123.45" + Environment.NewLine, writer.ToString());
    }

    [Fact]
    public void WriteLineDoubleWritesToBufferWithNewLine()
    {
        var writer = new BufferTextWriter();
        writer.WriteLine(123.45);
        Assert.Equal("123.45" + Environment.NewLine, writer.ToString());
    }

    [Fact]
    public void WriteLineIntWritesToBufferWithNewLine()
    {
        var writer = new BufferTextWriter();
        writer.WriteLine(12345);
        Assert.Equal("12345" + Environment.NewLine, writer.ToString());
    }

    [Fact]
    public void WriteLineLongWritesToBufferWithNewLine()
    {
        var writer = new BufferTextWriter();
        writer.WriteLine(12345L);
        Assert.Equal("12345" + Environment.NewLine, writer.ToString());
    }

    [Fact]
    public void WriteLineNullObjectWritesNullStringWithNewLine()
    {
        var writer = new BufferTextWriter();
        writer.WriteLine((object)null);
        Assert.Equal("null" + Environment.NewLine, writer.ToString());
    }

    [Fact]
    public void WriteLineObjectAsStringWritesToBufferWithNewLine()
    {
        var writer = new BufferTextWriter();
        writer.WriteLine((object)"Test");
        Assert.Equal("Test" + Environment.NewLine, writer.ToString());
    }

    [Fact]
    public void WriteLineObjectInvokesToStringWritesToBufferWithNewLine()
    {
        var writer = new BufferTextWriter();
        var obj = new TestObject();
        writer.WriteLine(obj);
        Assert.Equal("TestObject" + Environment.NewLine, writer.ToString());
    }

    [Fact]
    public void WriteLineReadOnlySpanWritesToBufferWithNewLine()
    {
        var writer = new BufferTextWriter();
        writer.WriteLine("Test".AsSpan());
        Assert.Equal("Test" + Environment.NewLine, writer.ToString());
    }

    [Fact]
    public void WriteLineFloatWritesToBufferWithNewLine()
    {
        var writer = new BufferTextWriter();
        writer.WriteLine(123.45f);
        Assert.Equal("123.45" + Environment.NewLine, writer.ToString());
    }

    [Fact]
    public void WriteLineStringWritesToBufferWithNewLine()
    {
        var writer = new BufferTextWriter();
        writer.WriteLine("Test");
        Assert.Equal("Test" + Environment.NewLine, writer.ToString());
    }

    [Fact]
    public void WriteLineEmptyStringWritesOnlyNewLine()
    {
        var writer = new BufferTextWriter();
        writer.WriteLine(string.Empty);
        Assert.Equal(Environment.NewLine, writer.ToString());
    }

    [Fact]
    public void WriteLineStringWithOneArgFormatsCorrectlyWithNewLine()
    {
        var writer = new BufferTextWriter();
        writer.WriteLine("Hello, {0}!", "World");
        Assert.Equal("Hello, World!" + Environment.NewLine, writer.ToString());
    }

    [Fact]
    public void WriteLineStringWithTwoArgsFormatsCorrectlyWithNewLine()
    {
        var writer = new BufferTextWriter();
        writer.WriteLine("Hello, {0} {1}!", "Dear", "World");
        Assert.Equal("Hello, Dear World!" + Environment.NewLine, writer.ToString());
    }

    [Fact]
    public void WriteLineStringWithThreeArgsFormatsCorrectlyWithNewLine()
    {
        var writer = new BufferTextWriter();
        writer.WriteLine("{0}, {1} {2}!", "Hello", "Dear", "World");
        Assert.Equal("Hello, Dear World!" + Environment.NewLine, writer.ToString());
    }

    [Fact]
    public void WriteLineStringWithObjectArrayFormatsCorrectlyWithNewLine()
    {
        var writer = new BufferTextWriter();
        writer.WriteLine("{0}, {1} {2}!", new object[] { "Hello", "Dear", "World" });
        Assert.Equal("Hello, Dear World!" + Environment.NewLine, writer.ToString());
    }

    [Fact]
    public void WriteLineStringBuilderWritesToBufferWithNewLine()
    {
        var writer = new BufferTextWriter();
        var sb = new StringBuilder();
        sb.Append("Test");
        writer.WriteLine(sb);
        Assert.Equal("Test" + Environment.NewLine, writer.ToString());
    }

    [Fact]
    public void WriteLineUIntWritesToBufferWithNewLine()
    {
        var writer = new BufferTextWriter();
        writer.WriteLine(12345u);
        Assert.Equal("12345" + Environment.NewLine, writer.ToString());
    }

    [Fact]
    public void WriteLineULongWritesToBufferWithNewLine()
    {
        var writer = new BufferTextWriter();
        writer.WriteLine(12345ul);
        Assert.Equal("12345" + Environment.NewLine, writer.ToString());
    }

    [Fact]
    public void MultipleWritesAppendToBuffer()
    {
        var writer = new BufferTextWriter();
        writer.Write("Hello");
        writer.Write(" ");
        writer.Write("World");
        Assert.Equal("Hello World", writer.ToString());
    }

    [Fact]
    public void FormattingWithIFormattable()
    {
        var writer = new BufferTextWriter();
        var dateTime = new DateTime(2023, 1, 1);
        writer.Write("{0:yyyy-MM-dd}", dateTime);
        Assert.Equal("2023-01-01", writer.ToString());
    }

    [Fact]
    public void FormattingWithISpanFormattable()
    {
        var writer = new BufferTextWriter();
        var guid = Guid.Parse("00000000-0000-0000-0000-000000000000");
        writer.Write("{0:N}", guid);
        Assert.Equal("00000000000000000000000000000000", writer.ToString());
    }

    [Fact]
    public void WriteWithReadOnlySpanArgs()
    {
        var writer = new BufferTextWriter();
        ReadOnlySpan<object> args = ["World"];
        writer.Write("Hello, {0}!", args);
        Assert.Equal("Hello, World!", writer.ToString());
    }

    [Fact]
    public void WriteLineWithReadOnlySpanArgs()
    {
        var writer = new BufferTextWriter();
        ReadOnlySpan<object> args = ["World"];
        writer.WriteLine("Hello, {0}!", args);
        Assert.Equal("Hello, World!" + Environment.NewLine, writer.ToString());
    }

    [Fact]
    public void WriteWithMultipleFormats()
    {
        var writer = new BufferTextWriter();
        writer.Write("Position: {0}, Name: {1}, Value: {2:N2}", 1, "Item", 123.456);
        Assert.Equal("Position: 1, Name: Item, Value: 123.46", writer.ToString());
    }

    [Fact]
    public void WriteWithRepeatedFormats()
    {
        var writer = new BufferTextWriter();
        writer.Write("First: {0}, Repeat: {0}, Third: {2}, Repeat First: {0}", "One", "Two", "Three");
        Assert.Equal("First: One, Repeat: One, Third: Three, Repeat First: One", writer.ToString());
    }

    [Fact]
    public void WriteWithLargeArgument()
    {
        var writer = new BufferTextWriter();
        var largeString = new string('A', 1000);
        writer.Write("Prefix {0} Suffix", largeString);
        Assert.Equal("Prefix " + largeString + " Suffix", writer.ToString());
    }

    [Fact]
    public void WriteWithLargeFormattedArgument()
    {
        var writer = new BufferTextWriter();
        var largeDouble = 1.23456789E+100;
        writer.Write("Value: {0:E}", largeDouble);
        Assert.Equal($"Value: {largeDouble:E}", writer.ToString());
    }

    [Fact]
    public void WriteVeryLargeSpanFormattable()
    {
        var writer = new BufferTextWriter(10);
        // Insanely unreasonable, but under most circumstances, most smaller SpanFormattables will end up not throwing due to convenient buffer allocations or by FAR enough scratch space
        var largeSpanFormattable = new LargeSpanFormattable(Configuration.MaxStackallocSize + 1);

        writer.Write("Value: {0}", largeSpanFormattable);

        Assert.Equal("Value: " + new string('X', Configuration.MaxStackallocSize + 1), writer.Span);
    }

    [Fact]
    public void CanWriteMaxBufferSize()
    {
        var writer = new BufferTextWriter();
        var largeString = new string('X', 2000);
        writer.Write(largeString);
        Assert.Equal(largeString, writer.ToString());
    }

    [Fact]
    public void WriteCharArrayPortionCorrectly()
    {
        var writer = new BufferTextWriter();
        var buffer = new char[] { 'H', 'e', 'l', 'l', 'o', ' ', 'W', 'o', 'r', 'l', 'd' };
        writer.Write(buffer, 6, 5);
        Assert.Equal("World", writer.ToString());
    }

    [Fact]
    public void WriteMultipleInterpolatedParts()
    {
        var writer = new BufferTextWriter();
        writer.Write("Name: {0}, Age: {1}, {2}: {3}", "John", 30, "Occupation", "Developer");
        Assert.Equal("Name: John, Age: 30, Occupation: Developer", writer.ToString());
    }

    [Fact]
    public void WriteWithAlignmentSpecifier()
    {
        var writer = new BufferTextWriter();
        writer.Write("Value: {0,10}", 123);
        Assert.Equal("Value:        123", writer.ToString());
    }

    [Fact]
    public void WriteWithFormatSpecifier()
    {
        var writer = new BufferTextWriter();
        writer.Write("Value: {0:X}", 255);
        Assert.Equal("Value: FF", writer.ToString());
    }

    [Fact]
    public void WritesToEmptyStringBuilder()
    {
        var writer = new BufferTextWriter();
        writer.Write(new StringBuilder());
        Assert.Equal(string.Empty, writer.ToString());
    }

    [Fact]
    public void WriteToStringBuilderWithMultipleChunks()
    {
        var writer = new BufferTextWriter();
        var sb = new StringBuilder();
        sb.Append("First");
        sb.Append("Second");
        sb.Append("Third");
        writer.Write(sb);
        Assert.Equal("FirstSecondThird", writer.ToString());
    }

    private class TestObject
    {
        public override string ToString() => "TestObject";
    }

    private class LargeSpanFormattable(int size) : ISpanFormattable
    {
        private readonly int _size = size;

        public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider provider)
        {
            if (destination.Length < _size)
            {
                charsWritten = 0;
                return false;
            }

            for (var i = 0; i < _size; i++)
            {
                destination[i] = 'X';
            }

            charsWritten = _size;
            return true;
        }

        public string ToString(string format, IFormatProvider formatProvider) => new string('X', _size);

        public override string ToString() => new string('X', _size);
    }
}