using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

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
/// Defines a contract for types that can be formatted as MSBuild error locations, which can include line numbers, column numbers, and ranges thereof.
/// </summary>
public interface IMSBuildErrorLocation : ISpanFormattable, IUtf8SpanFormattable;

/// <summary>
/// Represents a 1-based line number.
/// </summary>
/// <param name="line">The line number.</param>
public readonly struct Line(long line) : IMSBuildErrorLocation
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
            return false;

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
            return false;

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
public readonly struct LineRange(long start, long end) : IMSBuildErrorLocation
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
            return false;
        using var endResult = FormattingHelpers.TryFormatBytes(in end, format, provider);
        if (!endResult.Success || utf8Destination.Length < (acc += endResult.Span.Length))
            return false;

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
            return false;
        using var endResult = FormattingHelpers.TryFormatChars(in end, format, provider);
        if (!endResult.Success || destination.Length < (acc += endResult.Span.Length))
            return false;

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
public readonly struct LineAndColumn(long line, long column) : IMSBuildErrorLocation
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
            return false;
        using var colResult = FormattingHelpers.TryFormatBytes(in column, format, provider);
        if (!colResult.Success || utf8Destination.Length < (acc += colResult.Span.Length))
            return false;

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
            return false;
        using var colResult = FormattingHelpers.TryFormatChars(in column, format, provider);
        if (!colResult.Success || destination.Length < (acc += colResult.Span.Length))
            return false;

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
public readonly struct LineAndColumnRange(long line, long startColumn, long endColumn) : IMSBuildErrorLocation
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
            return false;
        using var startColResult = FormattingHelpers.TryFormatBytes(in startColumn, format, provider);
        if (!startColResult.Success || utf8Destination.Length < (acc += startColResult.Span.Length))
            return false;
        using var endColResult = FormattingHelpers.TryFormatBytes(in endColumn, format, provider);
        if (!endColResult.Success || utf8Destination.Length < (acc += endColResult.Span.Length))
            return false;

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
            return false;
        using var startColResult = FormattingHelpers.TryFormatChars(in startColumn, format, provider);
        if (!startColResult.Success || destination.Length < (acc += startColResult.Span.Length))
            return false;
        using var endColResult = FormattingHelpers.TryFormatChars(in endColumn, format, provider);
        if (!endColResult.Success || destination.Length < (acc += endColResult.Span.Length))
            return false;

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
public readonly struct TextRange(long startLine, long endLine, long startColumn, long endColumn) : IMSBuildErrorLocation
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
            return false;
        using var endLineResult = FormattingHelpers.TryFormatBytes(in endLine, format, provider);
        if (!endLineResult.Success || utf8Destination.Length < (acc += endLineResult.Span.Length))
            return false;
        using var startColResult = FormattingHelpers.TryFormatBytes(in startColumn, format, provider);
        if (!startColResult.Success || utf8Destination.Length < (acc += startColResult.Span.Length))
            return false;
        using var endColResult = FormattingHelpers.TryFormatBytes(in endColumn, format, provider);
        if (!endColResult.Success || utf8Destination.Length < (acc += endColResult.Span.Length))
            return false;

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
            return false;
        using var endLineResult = FormattingHelpers.TryFormatChars(in endLine, format, provider);
        if (!endLineResult.Success || destination.Length < (acc += endLineResult.Span.Length))
            return false;
        using var startColResult = FormattingHelpers.TryFormatChars(in startColumn, format, provider);
        if (!startColResult.Success || destination.Length < (acc += startColResult.Span.Length))
            return false;
        using var endColResult = FormattingHelpers.TryFormatChars(in endColumn, format, provider);
        if (!endColResult.Success || destination.Length < (acc += endColResult.Span.Length))
            return false;

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
    // longest possible rendered location: "(-9223372036854775808,-9223372036854775808,-9223372036854775808,-9223372036854775808)"
    private const int MaxLocationChars = 96;
    private const int StackallocByteLimit = 2048;

    // universal "no location" filler so every core path can unconditionally TryFormat a TLocation instead of branching on whether one was supplied
    private readonly struct NoLocation : IMSBuildErrorLocation
    {
        public override string ToString() => "";
        public string ToString(string format, IFormatProvider formatProvider) => "";
        public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider provider)
        {
            charsWritten = 0;
            return true;
        }
        public bool TryFormat(Span<byte> utf8Destination, out int bytesWritten, ReadOnlySpan<char> format, IFormatProvider provider)
        {
            bytesWritten = 0;
            return true;
        }
    }

    #region BuildError / BuildWarning - string
    /// <summary>
    /// Builds an MSBuild-recognizable <c>error</c> entry with no location information, as a <see langword="string"/>.
    /// </summary>
    public static string BuildError(string text, string origin = null, string subcategory = null, string code = null)
        => BuildString(MSBuildErrorType.Error, origin, default(NoLocation), text, subcategory, code);
    /// <summary>
    /// Builds an MSBuild-recognizable <c>warning</c> entry with no location information, as a <see langword="string"/>.
    /// </summary>
    public static string BuildWarning(string text, string origin = null, string subcategory = null, string code = null)
        => BuildString(MSBuildErrorType.Warning, origin, default(NoLocation), text, subcategory, code);
    /// <summary>
    /// Builds an MSBuild-recognizable <c>error</c> entry that references <paramref name="path"/> at <paramref name="location"/>, as a <see langword="string"/>.
    /// </summary>
    public static string BuildError<TLocation>(string path, TLocation location, string text, string subcategory = null, string code = null)
        where TLocation : struct, IMSBuildErrorLocation
        => BuildString(MSBuildErrorType.Error, path, location, text, subcategory, code);
    /// <summary>
    /// Builds an MSBuild-recognizable <c>warning</c> entry that references <paramref name="path"/> at <paramref name="location"/>, as a <see langword="string"/>.
    /// </summary>
    public static string BuildWarning<TLocation>(string path, TLocation location, string text, string subcategory = null, string code = null)
        where TLocation : struct, IMSBuildErrorLocation
        => BuildString(MSBuildErrorType.Warning, path, location, text, subcategory, code);
    #endregion

    #region BuildError / BuildWarning - TextWriter
    /// <summary>
    /// Writes an MSBuild-recognizable <c>error</c> line with no location information to <paramref name="writer"/>, terminated using <paramref name="writer"/>'s own <see cref="TextWriter.NewLine"/>. The caller is responsible for ensuring <paramref name="writer"/> is positioned such that the written line will actually be recognized as process output by MSBuild/Visual Studio.
    /// </summary>
    public static void WriteError(TextWriter writer, string text, string origin = null, string subcategory = null, string code = null)
        => WriteTo(writer, MSBuildErrorType.Error, origin, default(NoLocation), text, subcategory, code);
    /// <summary>
    /// Writes an MSBuild-recognizable <c>warning</c> line with no location information to <paramref name="writer"/>, terminated using <paramref name="writer"/>'s own <see cref="TextWriter.NewLine"/>. The caller is responsible for ensuring <paramref name="writer"/> is positioned such that the written line will actually be recognized as process output by MSBuild/Visual Studio.
    /// </summary>
    public static void WriteWarning(TextWriter writer, string text, string origin = null, string subcategory = null, string code = null)
        => WriteTo(writer, MSBuildErrorType.Warning, origin, default(NoLocation), text, subcategory, code);
    /// <summary>
    /// Writes an MSBuild-recognizable <c>error</c> line that references <paramref name="path"/> at <paramref name="location"/> to <paramref name="writer"/>, terminated using <paramref name="writer"/>'s own <see cref="TextWriter.NewLine"/>. The caller is responsible for ensuring <paramref name="writer"/> is positioned such that the written line will actually be recognized as process output by MSBuild/Visual Studio.
    /// </summary>
    public static void WriteError<TLocation>(TextWriter writer, string path, TLocation location, string text, string subcategory = null, string code = null)
        where TLocation : struct, IMSBuildErrorLocation
        => WriteTo(writer, MSBuildErrorType.Error, path, location, text, subcategory, code);
    /// <summary>
    /// Writes an MSBuild-recognizable <c>warning</c> line that references <paramref name="path"/> at <paramref name="location"/> to <paramref name="writer"/>, terminated using <paramref name="writer"/>'s own <see cref="TextWriter.NewLine"/>. The caller is responsible for ensuring <paramref name="writer"/> is positioned such that the written line will actually be recognized as process output by MSBuild/Visual Studio.
    /// </summary>
    public static void WriteWarning<TLocation>(TextWriter writer, string path, TLocation location, string text, string subcategory = null, string code = null)
        where TLocation : struct, IMSBuildErrorLocation
        => WriteTo(writer, MSBuildErrorType.Warning, path, location, text, subcategory, code);
    #endregion

    #region BuildError / BuildWarning - Stream
    /// <summary>
    /// Writes an MSBuild-recognizable <c>error</c> line with no location information, UTF8-encoded, to <paramref name="stream"/>, terminated with CRLF. The caller is responsible for ensuring <paramref name="stream"/> is positioned such that the written line will actually be recognized as process output by MSBuild/Visual Studio.
    /// </summary>
    public static void WriteError(Stream stream, string text, string origin = null, string subcategory = null, string code = null)
        => WriteTo(stream, MSBuildErrorType.Error, origin, default(NoLocation), text, subcategory, code);
    /// <summary>
    /// Writes an MSBuild-recognizable <c>warning</c> line with no location information, UTF8-encoded, to <paramref name="stream"/>, terminated with CRLF. The caller is responsible for ensuring <paramref name="stream"/> is positioned such that the written line will actually be recognized as process output by MSBuild/Visual Studio.
    /// </summary>
    public static void WriteWarning(Stream stream, string text, string origin = null, string subcategory = null, string code = null)
        => WriteTo(stream, MSBuildErrorType.Warning, origin, default(NoLocation), text, subcategory, code);
    /// <summary>
    /// Writes an MSBuild-recognizable <c>error</c> line that references <paramref name="path"/> at <paramref name="location"/>, UTF8-encoded, to <paramref name="stream"/>, terminated with CRLF. The caller is responsible for ensuring <paramref name="stream"/> is positioned such that the written line will actually be recognized as process output by MSBuild/Visual Studio.
    /// </summary>
    public static void WriteError<TLocation>(Stream stream, string path, TLocation location, string text, string subcategory = null, string code = null)
        where TLocation : struct, IMSBuildErrorLocation
        => WriteTo(stream, MSBuildErrorType.Error, path, location, text, subcategory, code);
    /// <summary>
    /// Writes an MSBuild-recognizable <c>warning</c> line that references <paramref name="path"/> at <paramref name="location"/>, UTF8-encoded, to <paramref name="stream"/>, terminated with CRLF. The caller is responsible for ensuring <paramref name="stream"/> is positioned such that the written line will actually be recognized as process output by MSBuild/Visual Studio.
    /// </summary>
    public static void WriteWarning<TLocation>(Stream stream, string path, TLocation location, string text, string subcategory = null, string code = null)
        where TLocation : struct, IMSBuildErrorLocation
        => WriteTo(stream, MSBuildErrorType.Warning, path, location, text, subcategory, code);
    #endregion

    private static string BuildString<TLocation>(MSBuildErrorType type, string origin, TLocation location, string text, string subcategory, string code)
        where TLocation : struct, IMSBuildErrorLocation
    {
        ArgumentNullException.ThrowIfNull(text);
        scoped var originSpan = origin.AsSpan();
        scoped var subcategorySpan = subcategory.AsSpan();
        scoped var codeSpan = code.AsSpan();
        scoped var textSpan = text.AsSpan();
        ValidateParts(originSpan, subcategorySpan, codeSpan, textSpan);

        var maxChars = EstimateCharCount(originSpan, subcategorySpan, codeSpan, textSpan, 0);
        byte[] rented = null;
        scoped Span<char> buffer;
        if (maxChars * sizeof(char) <= StackallocByteLimit)
            buffer = stackalloc char[maxChars];
        else
        {
            rented = ArrayPool<byte>.Shared.Rent(maxChars, out buffer);
        }

        try
        {
            var written = FormatChars(buffer, type, originSpan, location, subcategorySpan, codeSpan, textSpan, default);
            return new string(buffer[..written]);
        }
        finally
        {
            if (rented is not null)
                ArrayPool<byte>.Shared.Return(rented);
        }
    }
    private static void WriteTo<TLocation>(TextWriter writer, MSBuildErrorType type, string origin, TLocation location, string text, string subcategory, string code)
        where TLocation : struct, IMSBuildErrorLocation
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(text);
        scoped var originSpan = origin.AsSpan();
        scoped var subcategorySpan = subcategory.AsSpan();
        scoped var codeSpan = code.AsSpan();
        scoped var textSpan = text.AsSpan();
        ValidateParts(originSpan, subcategorySpan, codeSpan, textSpan);

        ReadOnlySpan<char> newLine = writer.NewLine;
        var maxChars = EstimateCharCount(originSpan, subcategorySpan, codeSpan, textSpan, newLine.Length);
        byte[] rented = null;
        scoped Span<char> buffer;
        if (maxChars * sizeof(char) <= StackallocByteLimit)
            buffer = stackalloc char[maxChars];
        else
        {
            rented = ArrayPool<byte>.Shared.Rent(maxChars, out buffer);
        }

        try
        {
            var written = FormatChars(buffer, type, originSpan, location, subcategorySpan, codeSpan, textSpan, newLine);
            writer.Write(buffer[..written]);
        }
        finally
        {
            if (rented is not null)
                ArrayPool<byte>.Shared.Return(rented);
        }
    }
    private static void WriteTo<TLocation>(Stream stream, MSBuildErrorType type, string origin, TLocation location, string text, string subcategory, string code)
        where TLocation : struct, IMSBuildErrorLocation
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentNullException.ThrowIfNull(text);
        scoped var originSpan = origin.AsSpan();
        scoped var subcategorySpan = subcategory.AsSpan();
        scoped var codeSpan = code.AsSpan();
        scoped var textSpan = text.AsSpan();
        ValidateParts(originSpan, subcategorySpan, codeSpan, textSpan);

        scoped var newLine = "\r\n"u8;
        var maxChars = EstimateCharCount(originSpan, subcategorySpan, codeSpan, textSpan, newLine.Length);
        var maxBytes = Encoding.UTF8.GetMaxByteCount(maxChars);
        byte[] rented = null;
        scoped var buffer = maxBytes <= StackallocByteLimit ? stackalloc byte[maxBytes] : (rented = ArrayPool<byte>.Shared.Rent(maxBytes));
        try
        {
            var written = FormatBytes(buffer, type, originSpan, location, subcategorySpan, codeSpan, textSpan, newLine);
            stream.Write(buffer[..written]);
        }
        finally
        {
            if (rented is not null)
                ArrayPool<byte>.Shared.Return(rented);
        }
    }

    // upper bound only - location gets a fixed generous allowance rather than being measured, since none of these values are known without formatting the underlying longs
    private static int EstimateCharCount(ReadOnlySpan<char> origin, ReadOnlySpan<char> subcategory, ReadOnlySpan<char> code, ReadOnlySpan<char> text, int newLineLength)
        => origin.Length + MaxLocationChars + 2 // "origin(location): "
         + subcategory.Length + 1               // "subcategory "
         + 7                                     // "warning" (longest category word)
         + 1 + code.Length                       // " code"
         + 2                                     // ": "
         + text.Length
         + newLineLength;

    // guards the parts of the grammar that would otherwise silently corrupt MSBuild's/VS's own line-oriented, single-line parse
    private static void ValidateParts(ReadOnlySpan<char> origin, ReadOnlySpan<char> subcategory, ReadOnlySpan<char> code, ReadOnlySpan<char> text)
    {
        if (origin.IndexOfAny('\r', '\n') >= 0)
            ThrowOriginContainsLineBreaks();
        if (subcategory.IndexOfAny('\r', '\n') >= 0)
            ThrowSubcategoryContainsLineBreaks();
        if (code.IndexOfAny('\r', '\n') >= 0 || code.IndexOfAny(' ', ':') >= 0)
            ThrowCodeContainsInvalidChars();
        if (text.IndexOfAny('\r', '\n') >= 0)
            ThrowTextContainsLineBreaks();
    }
    [DoesNotReturn][MethodImpl(MethodImplOptions.NoInlining)] private static void ThrowOriginContainsLineBreaks() => throw new ArgumentException("Origin must not contain line breaks.", "origin");
    [DoesNotReturn][MethodImpl(MethodImplOptions.NoInlining)] private static void ThrowSubcategoryContainsLineBreaks() => throw new ArgumentException("Subcategory must not contain line breaks.", "subcategory");
    [DoesNotReturn][MethodImpl(MethodImplOptions.NoInlining)] private static void ThrowCodeContainsInvalidChars() => throw new ArgumentException("Code must not contain line breaks, spaces or colons.", "code");
    [DoesNotReturn][MethodImpl(MethodImplOptions.NoInlining)] private static void ThrowTextContainsLineBreaks() => throw new ArgumentException("Text must not contain line breaks.", "text");

    private static int FormatChars<TLocation>(Span<char> destination, MSBuildErrorType type, ReadOnlySpan<char> origin, TLocation location, ReadOnlySpan<char> subcategory, ReadOnlySpan<char> code, ReadOnlySpan<char> text, ReadOnlySpan<char> newLine)
        where TLocation : struct, IMSBuildErrorLocation
    {
        var pos = 0;
        if (!origin.IsEmpty)
        {
            origin.CopyTo(destination[pos..]);
            pos += origin.Length;
            var success = location.TryFormat(destination[pos..], out var locChars, default, null);
            Debug.Assert(success);
            pos += locChars;
            destination[pos++] = ':';
            destination[pos++] = ' ';
        }
        if (!subcategory.IsEmpty)
        {
            subcategory.CopyTo(destination[pos..]);
            pos += subcategory.Length;
            if (subcategory[^1] != ' ')
                destination[pos++] = ' ';
        }
        ReadOnlySpan<char> category = type == MSBuildErrorType.Warning ? "warning" : "error";
        category.CopyTo(destination[pos..]);
        pos += category.Length;
        if (!code.IsEmpty)
        {
            destination[pos++] = ' ';
            code.CopyTo(destination[pos..]);
            pos += code.Length;
        }
        destination[pos++] = ':';
        destination[pos++] = ' ';
        text.CopyTo(destination[pos..]);
        pos += text.Length;
        newLine.CopyTo(destination[pos..]);
        pos += newLine.Length;
        return pos;
    }
    private static int FormatBytes<TLocation>(Span<byte> destination, MSBuildErrorType type, ReadOnlySpan<char> origin, TLocation location, ReadOnlySpan<char> subcategory, ReadOnlySpan<char> code, ReadOnlySpan<char> text, ReadOnlySpan<byte> newLine)
        where TLocation : struct, IMSBuildErrorLocation
    {
        var pos = 0;
        if (!origin.IsEmpty)
        {
            pos += Encoding.UTF8.GetBytes(origin, destination[pos..]);
            var success = location.TryFormat(destination[pos..], out var locBytes, default, null);
            Debug.Assert(success);
            pos += locBytes;
            destination[pos++] = (byte)':';
            destination[pos++] = (byte)' ';
        }
        if (!subcategory.IsEmpty)
        {
            pos += Encoding.UTF8.GetBytes(subcategory, destination[pos..]);
            if (subcategory[^1] != ' ')
                destination[pos++] = (byte)' ';
        }
        ReadOnlySpan<byte> category = type == MSBuildErrorType.Warning ? "warning"u8 : "error"u8;
        category.CopyTo(destination[pos..]);
        pos += category.Length;
        if (!code.IsEmpty)
        {
            destination[pos++] = (byte)' ';
            pos += Encoding.UTF8.GetBytes(code, destination[pos..]);
        }
        destination[pos++] = (byte)':';
        destination[pos++] = (byte)' ';
        pos += Encoding.UTF8.GetBytes(text, destination[pos..]);
        newLine.CopyTo(destination[pos..]);
        pos += newLine.Length;
        return pos;
    }
}
