namespace LaquaiLib.Text;

/// <summary>
/// Specifies the type of MSBuild error.
/// None except the two members defined in this <see langword="enum"/> are valid as category values for MSBuild errors. Anything not defined in this <see langword="enum"/> will be treated the same as <see cref="Error"/>.
/// </summary>
public enum MSBuildErrorType
{
    /// <summary>
    /// Emits <c>error</c> as the category of the MSBuild error.
    /// </summary>
    Error,
    /// <summary>
    /// Emits <c>warning</c> as the category of the MSBuild error.
    /// </summary>
    Warning
}

// I will admit, this might be a bit overkill, but naming these gives an easy way to differentiate in method signatures and "free-ish" formatting was a nice little exercise

/// <summary>
/// Represents a 1-based line number.
/// </summary>
/// <param name="line">The line number.</param>
public readonly struct Line(long line) : ISpanFormattable, IUtf8SpanFormattable
{
    /// <summary>
    /// Formats this instance to a <see langword="string"/> so that it is suitable for use in an MSBuild error entry.
    /// </summary>
    public override readonly string ToString() => string.Concat('(', line, ')');
    /// <summary>
    /// Returns the result of <see cref="ToString()"/>.
    /// </summary>
    public string ToString(string format, IFormatProvider formatProvider) => ToString();
    /// <summary>
    /// Formats this instance into <paramref name="utf8Destination"/>.
    /// </summary>
    public bool TryFormat(Span<byte> utf8Destination, out int bytesWritten, ReadOnlySpan<char> format, IFormatProvider provider)
    {
        bytesWritten = 0;
        // 2 static chars means the counter starts 2...
        var acc = 2;
        using var result = FormattingHelpers.TryFormatBytes(in line, format, provider);
        if (!result.Success || utf8Destination.Length < (acc += result.Span.Length))
        {
            return false;
        }

        utf8Destination[0] = (byte)'(';
        result.Span.CopyTo(utf8Destination[1..^1]);
        utf8Destination[acc - 1] = (byte)')';
        bytesWritten = acc;
        return true;
    }
    /// <summary>
    /// Formats this instance into <paramref name="destination"/>.
    /// </summary>
    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider provider)
    {
        charsWritten = 0;
        // 2 static chars means the counter starts 2...
        var acc = 2;
        using var result = FormattingHelpers.TryFormatChars(in line, format, provider);
        if (!result.Success || destination.Length < (acc += result.Span.Length))
        {
            return false;
        }

        destination[0] = '(';
        result.Span.CopyTo(destination[1..^1]);
        destination[acc - 1] = ')';
        charsWritten = acc;
        return true;
    }
}
/// <summary>
/// Represents a range of lines, expressed as 1-based line numbers.
/// </summary>
/// <param name="start">The starting line number of the range (inclusive).</param>
/// <param name="end">The ending line number of the range (inclusive).</param>
public readonly struct LineRange(long start, long end) : ISpanFormattable, IUtf8SpanFormattable
{
    /// <summary>
    /// Formats this instance to a <see langword="string"/> so that it is suitable for use in an MSBuild error entry.
    /// </summary>
    public override readonly string ToString() => string.Concat('(', start, '-', end, ')');
    /// <summary>
    /// Returns the result of <see cref="ToString()"/>.
    /// </summary>
    public string ToString(string format, IFormatProvider formatProvider) => ToString();
    /// <summary>
    /// Formats this instance into <paramref name="utf8Destination"/>.
    /// </summary>
    public bool TryFormat(Span<byte> utf8Destination, out int bytesWritten, ReadOnlySpan<char> format, IFormatProvider provider)
    {
        bytesWritten = 0;
        // 3 static chars means the counter starts 3, you get the point...
        var acc = 3;
        using var startResult = FormattingHelpers.TryFormatBytes(in start, format, provider);
        if (!startResult.Success || utf8Destination.Length < (acc += startResult.Span.Length))
        {
            return false;
        }
        using var endResult = FormattingHelpers.TryFormatBytes(in end, format, provider);
        if (!endResult.Success || utf8Destination.Length < (acc += endResult.Span.Length))
        {
            return false;
        }

        var dashIdx = 1 + startResult.Span.Length;
        utf8Destination[0] = (byte)'(';
        startResult.Span.CopyTo(utf8Destination[1..]);
        utf8Destination[dashIdx] = (byte)'-';
        endResult.Span.CopyTo(utf8Destination[(dashIdx + 1)..^1]);
        utf8Destination[acc - 1] = (byte)')';
        bytesWritten = acc;
        return true;
    }
    /// <summary>
    /// Formats this instance into <paramref name="destination"/>.
    /// </summary>
    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider provider)
    {
        charsWritten = 0;
        // 3 static chars means the counter starts 3, you get the point...
        var acc = 3;
        using var startResult = FormattingHelpers.TryFormatChars(in start, format, provider);
        if (!startResult.Success || destination.Length < (acc += startResult.Span.Length))
        {
            return false;
        }
        using var endResult = FormattingHelpers.TryFormatChars(in end, format, provider);
        if (!endResult.Success || destination.Length < (acc += endResult.Span.Length))
        {
            return false;
        }

        var dashIdx = 1 + startResult.Span.Length;
        destination[0] = '(';
        startResult.Span.CopyTo(destination[1..]);
        destination[dashIdx] = '-';
        endResult.Span.CopyTo(destination[(dashIdx + 1)..^1]);
        destination[acc - 1] = ')';
        charsWritten = acc;
        return true;
    }
}
/// <summary>
/// Represents a pair of 1-based line and column numbers.
/// </summary>
/// <param name="line">The 1-based line number.</param>
/// <param name="column">The 1-based column number.</param>
public readonly struct LineAndColumn(long line, long column) : ISpanFormattable, IUtf8SpanFormattable
{
    /// <summary>
    /// Formats this instance to a <see langword="string"/> so that it is suitable for use in an MSBuild error entry.
    /// </summary>
    public override readonly string ToString() => string.Concat('(', line, ',', column, ')');
    /// <summary>
    /// Returns the result of <see cref="ToString()"/>.
    /// </summary>
    public string ToString(string format, IFormatProvider formatProvider) => ToString();
    /// <summary>
    /// Formats this instance into <paramref name="utf8Destination"/>.
    /// </summary>
    public bool TryFormat(Span<byte> utf8Destination, out int bytesWritten, ReadOnlySpan<char> format, IFormatProvider provider)
    {
        bytesWritten = 0;
        // 3 static chars means the counter starts 3, you get the point...
        var acc = 3;
        using var lineResult = FormattingHelpers.TryFormatBytes(in line, format, provider);
        if (!lineResult.Success || utf8Destination.Length < (acc += lineResult.Span.Length))
        {
            return false;
        }
        using var colResult = FormattingHelpers.TryFormatBytes(in column, format, provider);
        if (!colResult.Success || utf8Destination.Length < (acc += colResult.Span.Length))
        {
            return false;
        }

        var commaIdx = 1 + lineResult.Span.Length;
        utf8Destination[0] = (byte)'(';
        lineResult.Span.CopyTo(utf8Destination[1..]);
        utf8Destination[commaIdx] = (byte)',';
        colResult.Span.CopyTo(utf8Destination[(commaIdx + 1)..^1]);
        utf8Destination[acc - 1] = (byte)')';
        bytesWritten = acc;
        return true;
    }
    /// <summary>
    /// Formats this instance into <paramref name="destination"/>.
    /// </summary>
    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider provider)
    {
        charsWritten = 0;
        // 3 static chars means the counter starts 3, you get the point...
        var acc = 3;
        using var lineResult = FormattingHelpers.TryFormatChars(in line, format, provider);
        if (!lineResult.Success || destination.Length < (acc += lineResult.Span.Length))
        {
            return false;
        }
        using var colResult = FormattingHelpers.TryFormatChars(in column, format, provider);
        if (!colResult.Success || destination.Length < (acc += colResult.Span.Length))
        {
            return false;
        }

        var commaIdx = 1 + lineResult.Span.Length;
        destination[0] = '(';
        lineResult.Span.CopyTo(destination[1..]);
        destination[commaIdx] = ',';
        colResult.Span.CopyTo(destination[(commaIdx + 1)..^1]);
        destination[acc - 1] = ')';
        charsWritten = acc;
        return true;
    }
}
/// <summary>
/// Represents a 1-based line number and a range of 1-based column numbers.
/// </summary>
/// <param name="line">The 1-based line number.</param>
/// <param name="startColumn">The starting 1-based column number.</param>
/// <param name="endColumn">The ending 1-based column number.</param>
public readonly struct LineAndColumnRange(long line, long startColumn, long endColumn) : ISpanFormattable, IUtf8SpanFormattable
{
    /// <summary>
    /// Formats this instance to a <see langword="string"/> so that it is suitable for use in an MSBuild error entry.
    /// </summary>
    public override readonly string ToString() => string.Concat('(', line, ',', startColumn, '-', endColumn, ')');
    /// <summary>
    /// Returns the result of <see cref="ToString()"/>.
    /// </summary>
    public string ToString(string format, IFormatProvider formatProvider) => ToString();
    /// <summary>
    /// Formats this instance into <paramref name="utf8Destination"/>.
    /// </summary>
    public bool TryFormat(Span<byte> utf8Destination, out int bytesWritten, ReadOnlySpan<char> format, IFormatProvider provider)
    {
        bytesWritten = 0;
        var acc = 4;
        using var lineResult = FormattingHelpers.TryFormatBytes(in line, format, provider);
        if (!lineResult.Success || utf8Destination.Length < (acc += lineResult.Span.Length))
        {
            return false;
        }
        using var startColResult = FormattingHelpers.TryFormatBytes(in startColumn, format, provider);
        if (!startColResult.Success || utf8Destination.Length < (acc += startColResult.Span.Length))
        {
            return false;
        }
        using var endColResult = FormattingHelpers.TryFormatBytes(in endColumn, format, provider);
        if (!endColResult.Success || utf8Destination.Length < (acc += endColResult.Span.Length))
        {
            return false;
        }

        var commaIdx = 1 + lineResult.Span.Length;
        var dashIdx = commaIdx + 1 + startColResult.Span.Length;
        utf8Destination[0] = (byte)'(';
        lineResult.Span.CopyTo(utf8Destination[1..]);
        utf8Destination[commaIdx] = (byte)',';
        startColResult.Span.CopyTo(utf8Destination[(commaIdx + 1)..]);
        utf8Destination[dashIdx] = (byte)'-';
        endColResult.Span.CopyTo(utf8Destination[(dashIdx + 1)..^1]);
        utf8Destination[acc - 1] = (byte)')';
        bytesWritten = acc;
        return true;
    }
    /// <summary>
    /// Formats this instance into <paramref name="destination"/>.
    /// </summary>
    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider provider)
    {
        charsWritten = 0;
        var acc = 4;
        using var lineResult = FormattingHelpers.TryFormatChars(in line, format, provider);
        if (!lineResult.Success || destination.Length < (acc += lineResult.Span.Length))
        {
            return false;
        }
        using var startColResult = FormattingHelpers.TryFormatChars(in startColumn, format, provider);
        if (!startColResult.Success || destination.Length < (acc += startColResult.Span.Length))
        {
            return false;
        }
        using var endColResult = FormattingHelpers.TryFormatChars(in endColumn, format, provider);
        if (!endColResult.Success || destination.Length < (acc += endColResult.Span.Length))
        {
            return false;
        }

        var commaIdx = 1 + lineResult.Span.Length;
        var dashIdx = commaIdx + 1 + startColResult.Span.Length;
        destination[0] = '(';
        lineResult.Span.CopyTo(destination[1..]);
        destination[commaIdx] = ',';
        startColResult.Span.CopyTo(destination[(commaIdx + 1)..]);
        destination[dashIdx] = '-';
        endColResult.Span.CopyTo(destination[(dashIdx + 1)..^1]);
        destination[acc - 1] = ')';
        charsWritten = acc;
        return true;
    }
}
/// <summary>
/// Represents a range of 1-based line numbers and a range of 1-based column numbers.
/// </summary>
/// <param name="startLine">The starting 1-based line number.</param>
/// <param name="endLine">The ending 1-based line number.</param>
/// <param name="startColumn">The starting 1-based column number.</param>
/// <param name="endColumn">The ending 1-based column number.</param>
public readonly struct TextRange(long startLine, long endLine, long startColumn, long endColumn) : ISpanFormattable, IUtf8SpanFormattable
{
    /// <summary>
    /// Formats this instance to a <see langword="string"/> so that it is suitable for use in an MSBuild error entry.
    /// </summary>
    public override readonly string ToString() => string.Concat('(', startLine, ',', startColumn, ',', endLine, ',', endColumn, ')');
    /// <summary>
    /// Returns the result of <see cref="ToString()"/>.
    /// </summary>
    public string ToString(string format, IFormatProvider formatProvider) => ToString();
    /// <summary>
    /// Formats this instance into <paramref name="utf8Destination"/>.
    /// </summary>
    public bool TryFormat(Span<byte> utf8Destination, out int bytesWritten, ReadOnlySpan<char> format, IFormatProvider provider)
    {
        bytesWritten = 0;
        var acc = 5;
        using var startLineResult = FormattingHelpers.TryFormatBytes(in startLine, format, provider);
        if (!startLineResult.Success || utf8Destination.Length < (acc += startLineResult.Span.Length))
        {
            return false;
        }
        using var endLineResult = FormattingHelpers.TryFormatBytes(in endLine, format, provider);
        if (!endLineResult.Success || utf8Destination.Length < (acc += endLineResult.Span.Length))
        {
            return false;
        }
        using var startColResult = FormattingHelpers.TryFormatBytes(in startColumn, format, provider);
        if (!startColResult.Success || utf8Destination.Length < (acc += startColResult.Span.Length))
        {
            return false;
        }
        using var endColResult = FormattingHelpers.TryFormatBytes(in endColumn, format, provider);
        if (!endColResult.Success || utf8Destination.Length < (acc += endColResult.Span.Length))
        {
            return false;
        }

        var comma1Idx = 1 + startLineResult.Span.Length;
        var comma2Idx = comma1Idx + 1 + startColResult.Span.Length;
        var comma3Idx = comma2Idx + 1 + endLineResult.Span.Length;
        utf8Destination[0] = (byte)'(';
        startLineResult.Span.CopyTo(utf8Destination[1..]);
        utf8Destination[comma1Idx] = (byte)',';
        startColResult.Span.CopyTo(utf8Destination[(comma1Idx + 1)..]);
        utf8Destination[comma2Idx] = (byte)',';
        endLineResult.Span.CopyTo(utf8Destination[(comma2Idx + 1)..]);
        utf8Destination[comma3Idx] = (byte)',';
        endColResult.Span.CopyTo(utf8Destination[(comma3Idx + 1)..^1]);
        utf8Destination[acc - 1] = (byte)')';
        bytesWritten = acc;
        return true;
    }
    /// <summary>
    /// Formats this instance into <paramref name="destination"/>.
    /// </summary>
    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider provider)
    {
        charsWritten = 0;
        var acc = 5;
        using var startLineResult = FormattingHelpers.TryFormatChars(in startLine, format, provider);
        if (!startLineResult.Success || destination.Length < (acc += startLineResult.Span.Length))
        {
            return false;
        }
        using var endLineResult = FormattingHelpers.TryFormatChars(in endLine, format, provider);
        if (!endLineResult.Success || destination.Length < (acc += endLineResult.Span.Length))
        {
            return false;
        }
        using var startColResult = FormattingHelpers.TryFormatChars(in startColumn, format, provider);
        if (!startColResult.Success || destination.Length < (acc += startColResult.Span.Length))
        {
            return false;
        }
        using var endColResult = FormattingHelpers.TryFormatChars(in endColumn, format, provider);
        if (!endColResult.Success || destination.Length < (acc += endColResult.Span.Length))
        {
            return false;
        }

        var comma1Idx = 1 + startLineResult.Span.Length;
        var comma2Idx = comma1Idx + 1 + startColResult.Span.Length;
        var comma3Idx = comma2Idx + 1 + endLineResult.Span.Length;
        destination[0] = '(';
        startLineResult.Span.CopyTo(destination[1..]);
        destination[comma1Idx] = ',';
        startColResult.Span.CopyTo(destination[(comma1Idx + 1)..]);
        destination[comma2Idx] = ',';
        endLineResult.Span.CopyTo(destination[(comma2Idx + 1)..]);
        destination[comma3Idx] = ',';
        endColResult.Span.CopyTo(destination[(comma3Idx + 1)..^1]);
        destination[acc - 1] = ')';
        charsWritten = acc;
        return true;
    }
}

/// <summary>
/// Provides functionality related to MSBuild operations.
/// </summary>
public static class MSBuild
{
    public static string BuildError()
    {
        return BuildCore(MSBuildErrorType.Error);
    }
    private static string BuildCore(MSBuildErrorType type) => null;
}
