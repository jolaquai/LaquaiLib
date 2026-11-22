using System.Globalization;

using LaquaiLib.Extensions;

namespace LaquaiLib.UnitTests.Extensions;

public class TextWriterExtensionsTests
{
    private static StringWriter Create(IFormatProvider provider = null) => new StringWriter(provider ?? CultureInfo.InvariantCulture);

    private sealed class PlainObject(string text)
    {
        public override string ToString() => text;
    }

    private readonly struct FormattableOnly(string text) : IFormattable
    {
        public string ToString(string format, IFormatProvider formatProvider) => $"{text}|{format}|{formatProvider?.GetType().Name}";
    }

    private readonly struct SpanFormattableLong(int length) : ISpanFormattable
    {
        public string ToString(string format, IFormatProvider formatProvider) => new string('x', length);
        public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider provider)
        {
            if (destination.Length < length)
            {
                charsWritten = 0;
                return false;
            }
            destination[..length].Fill('x');
            charsWritten = length;
            return true;
        }
        public override string ToString() => ToString(null, null);
    }

    private readonly struct SpanFormattableNeverFits : ISpanFormattable
    {
        public string ToString(string format, IFormatProvider formatProvider) => "fallback";
        public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider provider)
        {
            charsWritten = 0;
            return false;
        }
    }

    private readonly struct SpanFormattableEchoFormat : ISpanFormattable
    {
        public string ToString(string format, IFormatProvider formatProvider) => format ?? "";
        public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider provider)
        {
            charsWritten = format.Length;
            return format.TryCopyTo(destination);
        }
    }

    [Fact]
    public void WriteInterpolatedWritesLiteralOnly()
    {
        using var tw = Create();
        tw.WriteInterpolated($"hello");
        Assert.Equal("hello", tw.ToString());
    }

    [Fact]
    public void WriteInterpolatedWritesEmptyString()
    {
        using var tw = Create();
        tw.WriteInterpolated($"");
        Assert.Equal("", tw.ToString());
    }

    [Fact]
    public void WriteInterpolatedDoesNotAppendNewLine()
    {
        using var tw = Create();
        tw.WriteInterpolated($"a{1}b");
        Assert.DoesNotContain('\n', tw.ToString());
    }

    [Fact]
    public void WriteLineInterpolatedAppendsNewLine()
    {
        using var tw = Create();
        tw.WriteLineInterpolated($"a{1}b");
        Assert.Equal("a1b" + tw.NewLine, tw.ToString());
    }

    [Fact]
    public void WriteLineInterpolatedWithEmptyStringWritesOnlyNewLine()
    {
        using var tw = Create();
        tw.WriteLineInterpolated($"");
        Assert.Equal(tw.NewLine, tw.ToString());
    }

    [Fact]
    public void ConsecutiveCallsAppend()
    {
        using var tw = Create();
        tw.WriteInterpolated($"a");
        tw.WriteInterpolated($"{2}");
        tw.WriteLineInterpolated($"c");
        tw.WriteInterpolated($"d");
        Assert.Equal("a2c" + tw.NewLine + "d", tw.ToString());
    }

    [Fact]
    public void IntHoleIsFormatted()
    {
        using var tw = Create();
        tw.WriteInterpolated($"[{42}]");
        Assert.Equal("[42]", tw.ToString());
    }

    [Fact]
    public void NegativeIntHoleIsFormatted()
    {
        using var tw = Create();
        tw.WriteInterpolated($"[{-42}]");
        Assert.Equal("[-42]", tw.ToString());
    }

    [Fact]
    public void IntHoleHonorsFormat()
    {
        using var tw = Create();
        tw.WriteInterpolated($"[{255:x}]");
        Assert.Equal("[ff]", tw.ToString());
    }

    [Fact]
    public void IntHoleHonorsUpperCaseFormat()
    {
        using var tw = Create();
        tw.WriteInterpolated($"[{255:X4}]");
        Assert.Equal("[00FF]", tw.ToString());
    }

    [Fact]
    public void PositiveAlignmentPadsLeft()
    {
        using var tw = Create();
        tw.WriteInterpolated($"[{42,6}]");
        Assert.Equal("[    42]", tw.ToString());
    }

    [Fact]
    public void NegativeAlignmentPadsRight()
    {
        using var tw = Create();
        tw.WriteInterpolated($"[{42,-6}]");
        Assert.Equal("[42    ]", tw.ToString());
    }

    [Fact]
    public void AlignmentAndFormatCombine()
    {
        using var tw = Create();
        tw.WriteInterpolated($"[{255,6:x}]");
        Assert.Equal("[    ff]", tw.ToString());
    }

    [Fact]
    public void NegativeAlignmentAndFormatCombine()
    {
        using var tw = Create();
        tw.WriteInterpolated($"[{255,-6:x}]");
        Assert.Equal("[ff    ]", tw.ToString());
    }

    [Fact]
    public void AlignmentSmallerThanValueDoesNotPadOrTruncate()
    {
        using var tw = Create();
        tw.WriteInterpolated($"[{123456,3}]");
        Assert.Equal("[123456]", tw.ToString());
    }

    [Fact]
    public void AlignmentEqualToValueLengthDoesNotPad()
    {
        using var tw = Create();
        tw.WriteInterpolated($"[{123,3}][{123,-3}]");
        Assert.Equal("[123][123]", tw.ToString());
    }

    [Fact]
    public void ZeroAlignmentDoesNotPad()
    {
        using var tw = Create();
        tw.WriteInterpolated($"[{7,0}]");
        Assert.Equal("[7]", tw.ToString());
    }

    [Theory]
    [InlineData(1)]
    [InlineData(16)]
    [InlineData(17)]
    [InlineData(100)]
    [InlineData(511)]
    [InlineData(512)]
    [InlineData(513)]
    [InlineData(5000)]
    public void PositiveAlignmentPadsAcrossBufferStrategies(int alignment)
    {
        using var tw = Create();
        new TextWriterExtensions.TextWriterInterpolatedStringHandler(0, 1, tw).AppendFormatted(9, alignment);
        Assert.Equal(new string(' ', alignment - 1) + "9", tw.ToString());
    }

    [Theory]
    [InlineData(1)]
    [InlineData(16)]
    [InlineData(17)]
    [InlineData(100)]
    [InlineData(511)]
    [InlineData(512)]
    [InlineData(513)]
    [InlineData(5000)]
    public void NegativeAlignmentPadsAcrossBufferStrategies(int alignment)
    {
        using var tw = Create();
        new TextWriterExtensions.TextWriterInterpolatedStringHandler(0, 1, tw).AppendFormatted(9, -alignment);
        Assert.Equal("9" + new string(' ', alignment - 1), tw.ToString());
    }

    [Fact]
    public void StringHoleIsWritten()
    {
        using var tw = Create();
        var s = "abc";
        tw.WriteInterpolated($"[{s}]");
        Assert.Equal("[abc]", tw.ToString());
    }

    [Fact]
    public void StringHoleHonorsAlignment()
    {
        using var tw = Create();
        var s = "abc";
        tw.WriteInterpolated($"[{s,6}][{s,-6}]");
        Assert.Equal("[   abc][abc   ]", tw.ToString());
    }

    [Fact]
    public void EmptyStringHoleWithAlignmentIsAllPadding()
    {
        using var tw = Create();
        var s = "";
        tw.WriteInterpolated($"[{s,4}]");
        Assert.Equal("[    ]", tw.ToString());
    }

    [Fact]
    public void NullStringHoleWritesNothing()
    {
        using var tw = Create();
        string s = null;
        tw.WriteInterpolated($"[{s}]");
        Assert.Equal("[]", tw.ToString());
    }

    [Fact]
    public void NullStringHoleStillPads()
    {
        using var tw = Create();
        string s = null;
        tw.WriteInterpolated($"[{s,3}][{s,-3}]");
        Assert.Equal("[   ][   ]", tw.ToString());
    }

    [Fact]
    public void StringHoleIgnoresFormat()
    {
        using var tw = Create();
        var s = "abc";
        tw.WriteInterpolated($"[{s:x}]");
        Assert.Equal("[abc]", tw.ToString());
    }

    [Fact]
    public void SpanHoleIsWritten()
    {
        using var tw = Create();
        var span = "abc".AsSpan();
        tw.WriteInterpolated($"[{span}]");
        Assert.Equal("[abc]", tw.ToString());
    }

    [Fact]
    public void SpanHoleHonorsAlignment()
    {
        using var tw = Create();
        var span = "abc".AsSpan();
        tw.WriteInterpolated($"[{span,5}][{span,-5}]");
        Assert.Equal("[  abc][abc  ]", tw.ToString());
    }

    [Fact]
    public void EmptySpanHoleWritesNothing()
    {
        using var tw = Create();
        var span = ReadOnlySpan<char>.Empty;
        tw.WriteInterpolated($"[{span}]");
        Assert.Equal("[]", tw.ToString());
    }

    [Fact]
    public void NullObjectHoleWritesNothing()
    {
        using var tw = Create();
        object o = null;
        tw.WriteInterpolated($"[{o}]");
        Assert.Equal("[]", tw.ToString());
    }

    [Fact]
    public void NullObjectHoleStillPads()
    {
        using var tw = Create();
        object o = null;
        tw.WriteInterpolated($"[{o,3}][{o,-3}]");
        Assert.Equal("[   ][   ]", tw.ToString());
    }

    [Fact]
    public void NullNullableHoleWritesNothing()
    {
        using var tw = Create();
        int? n = null;
        tw.WriteInterpolated($"[{n}]");
        Assert.Equal("[]", tw.ToString());
    }

    [Fact]
    public void NonNullNullableHoleIsFormatted()
    {
        using var tw = Create();
        int? n = 5;
        tw.WriteInterpolated($"[{n,3}]");
        Assert.Equal("[  5]", tw.ToString());
    }

    [Fact]
    public void PlainObjectHoleUsesToString()
    {
        using var tw = Create();
        var o = new PlainObject("obj");
        tw.WriteInterpolated($"[{o}]");
        Assert.Equal("[obj]", tw.ToString());
    }

    [Fact]
    public void PlainObjectHoleHonorsAlignment()
    {
        using var tw = Create();
        var o = new PlainObject("obj");
        tw.WriteInterpolated($"[{o,5}][{o,-5}]");
        Assert.Equal("[  obj][obj  ]", tw.ToString());
    }

    [Fact]
    public void PlainObjectWithNullToStringWritesNothing()
    {
        using var tw = Create();
        var o = new PlainObject(null);
        tw.WriteInterpolated($"[{o}]");
        Assert.Equal("[]", tw.ToString());
    }

    [Fact]
    public void PlainObjectWithNullToStringStillPads()
    {
        using var tw = Create();
        var o = new PlainObject(null);
        tw.WriteInterpolated($"[{o,2}]");
        Assert.Equal("[  ]", tw.ToString());
    }

    [Fact]
    public void FormattableOnlyHoleReceivesFormatAndProvider()
    {
        using var tw = Create();
        var f = new FormattableOnly("v");
        tw.WriteInterpolated($"{f:abc}");
        Assert.Equal("v|abc|CultureInfo", tw.ToString());
    }

    [Fact]
    public void FormattableOnlyHoleWithoutFormatReceivesNullFormat()
    {
        using var tw = Create();
        var f = new FormattableOnly("v");
        tw.WriteInterpolated($"{f}");
        Assert.Equal("v||CultureInfo", tw.ToString());
    }

    [Fact]
    public void FormattableOnlyHoleHonorsAlignment()
    {
        using var tw = Create();
        var f = new FormattableOnly("v");
        tw.WriteInterpolated($"[{f,20}][{f,-20}]");
        Assert.Equal("[      v||CultureInfo][v||CultureInfo      ]", tw.ToString());
    }

    [Fact]
    public void BoxedFormattableHoleIsFormatted()
    {
        using var tw = Create();
        IFormattable f = new FormattableOnly("v");
        tw.WriteInterpolated($"{f:q}");
        Assert.Equal("v|q|CultureInfo", tw.ToString());
    }

    [Fact]
    public void SpanFormattableHoleBeyondStartSizeIsWrittenInFull()
    {
        using var tw = Create();
        var s = new SpanFormattableLong(1000);
        tw.WriteInterpolated($"{s}");
        Assert.Equal(new string('x', 1000), tw.ToString());
    }

    [Fact]
    public void SpanFormattableHoleNearRetryCapIsWrittenInFull()
    {
        using var tw = Create();
        var s = new SpanFormattableLong(60000);
        tw.WriteInterpolated($"{s}");
        Assert.Equal(new string('x', 60000), tw.ToString());
    }

    [Fact]
    public void SpanFormattableHoleHonorsAlignmentAfterRetry()
    {
        using var tw = Create();
        var s = new SpanFormattableLong(40);
        tw.WriteInterpolated($"[{s,45}][{s,-45}]");
        var x = new string('x', 40);
        Assert.Equal($"[{new string(' ', 5)}{x}][{x}{new string(' ', 5)}]", tw.ToString());
    }

    [Fact]
    public void SpanFormattableHoleThatNeverFitsFallsBackToToString()
    {
        using var tw = Create();
        var s = new SpanFormattableNeverFits();
        tw.WriteInterpolated($"[{s}]");
        Assert.Equal("[fallback]", tw.ToString());
    }

    [Fact]
    public void SpanFormattableHoleThatNeverFitsStillPads()
    {
        using var tw = Create();
        var s = new SpanFormattableNeverFits();
        tw.WriteInterpolated($"[{s,10}]");
        Assert.Equal("[  fallback]", tw.ToString());
    }

    [Fact]
    public void SpanFormattableHoleReceivesFormat()
    {
        using var tw = Create();
        var s = new SpanFormattableEchoFormat();
        tw.WriteInterpolated($"[{s:hello}]");
        Assert.Equal("[hello]", tw.ToString());
    }

    [Fact]
    public void SpanFormattableHoleWithoutFormatWritesNothing()
    {
        using var tw = Create();
        var s = new SpanFormattableEchoFormat();
        tw.WriteInterpolated($"[{s}]");
        Assert.Equal("[]", tw.ToString());
    }

    [Fact]
    public void FormatProviderOfWriterIsHonored()
    {
        using var tw = Create(CultureInfo.GetCultureInfo("de-DE"));
        tw.WriteInterpolated($"{1.5}");
        Assert.Equal("1,5", tw.ToString());
    }

    [Fact]
    public void InvariantFormatProviderOfWriterIsHonored()
    {
        using var tw = Create(CultureInfo.InvariantCulture);
        tw.WriteInterpolated($"{1.5}");
        Assert.Equal("1.5", tw.ToString());
    }

    [Fact]
    public void FormatProviderIsHonoredForFormattableOnlyHoles()
    {
        var culture = CultureInfo.GetCultureInfo("de-DE");
        using var tw = Create(culture);
        var d = new DateTime(2024, 3, 5);
        tw.WriteInterpolated($"{d:d}");
        Assert.Equal(d.ToString("d", culture), tw.ToString());
    }

    [Fact]
    public void DateTimeHoleHonorsFormat()
    {
        using var tw = Create();
        var d = new DateTime(2024, 3, 5);
        tw.WriteInterpolated($"{d:yyyy-MM-dd}");
        Assert.Equal("2024-03-05", tw.ToString());
    }

    [Fact]
    public void GuidHoleHonorsFormat()
    {
        using var tw = Create();
        var g = Guid.Parse("11111111-2222-3333-4444-555555555555");
        tw.WriteInterpolated($"{g:N}");
        Assert.Equal("11111111222233334444555555555555", tw.ToString());
    }

    [Fact]
    public void BoolHoleIsFormatted()
    {
        using var tw = Create();
        tw.WriteInterpolated($"{true}/{false}");
        Assert.Equal("True/False", tw.ToString());
    }

    [Fact]
    public void CharHoleIsFormatted()
    {
        using var tw = Create();
        tw.WriteInterpolated($"{'z',3}");
        Assert.Equal("  z", tw.ToString());
    }

    [Fact]
    public void EnumHoleIsFormatted()
    {
        using var tw = Create();
        tw.WriteInterpolated($"{DayOfWeek.Friday}|{DayOfWeek.Friday:D}");
        Assert.Equal("Friday|5", tw.ToString());
    }

    [Fact]
    public void LongNumericHoleBeyondStartSizeIsWrittenInFull()
    {
        using var tw = Create();
        var big = System.Numerics.BigInteger.Pow(10, 200);
        tw.WriteInterpolated($"{big}");
        Assert.Equal("1" + new string('0', 200), tw.ToString());
    }

    [Fact]
    public void MixedHolesAreWrittenInOrder()
    {
        using var tw = Create();
        var s = "str";
        var span = "sp".AsSpan();
        object o = null;
        tw.WriteInterpolated($"a{1,3}b{s,-4}c{span,3}d{o,2}e{255:x}f{new PlainObject("p")}");
        Assert.Equal("a  1bstr c spd  efffp", tw.ToString());
    }

    [Fact]
    public void LiteralsBetweenHolesAreUnaltered()
    {
        using var tw = Create();
        tw.WriteInterpolated($"  {1}  {2}  ");
        Assert.Equal("  1  2  ", tw.ToString());
    }

    [Fact]
    public void EscapedBracesAreWrittenLiterally()
    {
        using var tw = Create();
        tw.WriteInterpolated($"{{{1}}}");
        Assert.Equal("{1}", tw.ToString());
    }

    [Fact]
    public void LiteralWithNewLinesIsUnaltered()
    {
        using var tw = Create();
        tw.WriteInterpolated($"a\r\nb\n{1}");
        Assert.Equal("a\r\nb\n1", tw.ToString());
    }

    [Fact]
    public void LargeLiteralIsWrittenInFull()
    {
        using var tw = Create();
        var literal = new string('q', 10000);
        tw.WriteInterpolated($"{literal}");
        Assert.Equal(literal, tw.ToString());
    }

    [Fact]
    public void LargeStringHoleIsWrittenInFull()
    {
        using var tw = Create();
        var big = new string('q', 200000);
        tw.WriteInterpolated($"{big,200005}");
        Assert.Equal(new string(' ', 5) + big, tw.ToString());
    }

    [Fact]
    public void HoleExpressionsAreEvaluatedOnce()
    {
        using var tw = Create();
        var count = 0;
        int Next() => ++count;
        tw.WriteInterpolated($"{Next()}{Next()}{Next()}");
        Assert.Equal("123", tw.ToString());
        Assert.Equal(3, count);
    }

    [Fact]
    public void ThrowingHoleLeavesEarlierOutputWritten()
    {
        using var tw = Create();
        static int Throw() => throw new InvalidOperationException();
        Assert.Throws<InvalidOperationException>(() => tw.WriteInterpolated($"a{1}b{Throw()}c"));
        Assert.Equal("a1b", tw.ToString());
    }

    [Fact]
    public void WorksWithNonStringWriterTextWriter()
    {
        using var ms = new MemoryStream();
        using (var sw = new StreamWriter(ms, leaveOpen: true))
        {
            sw.WriteInterpolated($"x{12,4}y");
            sw.WriteLineInterpolated($"z");
        }
        Assert.Equal("x  12yz" + Environment.NewLine, System.Text.Encoding.UTF8.GetString(ms.ToArray()));
    }

    [Fact]
    public void WorksThroughTextWriterBaseReference()
    {
        using var sw = Create();
        TextWriter tw = sw;
        tw.WriteInterpolated($"{3}");
        Assert.Equal("3", sw.ToString());
    }

    [Fact]
    public void WorksOnSynchronizedWriter()
    {
        using var sw = Create();
        using var tw = TextWriter.Synchronized(sw);
        tw.WriteInterpolated($"{3,2}");
        tw.Flush();
        Assert.Equal(" 3", sw.ToString());
    }

    [Fact]
    public void HandlerWritesDirectlyToWriterDuringConstruction()
    {
        using var tw = Create();
        var handler = new TextWriterExtensions.TextWriterInterpolatedStringHandler(3, 1, tw);
        handler.AppendLiteral("abc");
        handler.AppendFormatted(5, 3);
        Assert.Equal("abc  5", tw.ToString());
    }

    [Fact]
    public void HandlerAppendFormattedGenericNullWritesPadding()
    {
        using var tw = Create();
        var handler = new TextWriterExtensions.TextWriterInterpolatedStringHandler(0, 1, tw);
        handler.AppendFormatted<object>(null, 3);
        Assert.Equal("   ", tw.ToString());
    }

    [Fact]
    public void HandlerAppendFormattedIFormattableOverloadUsesProvider()
    {
        using var tw = Create(CultureInfo.GetCultureInfo("de-DE"));
        var handler = new TextWriterExtensions.TextWriterInterpolatedStringHandler(0, 1, tw);
        IFormattable f = 1.5;
        handler.AppendFormatted(f);
        Assert.Equal("1,5", tw.ToString());
    }
}
