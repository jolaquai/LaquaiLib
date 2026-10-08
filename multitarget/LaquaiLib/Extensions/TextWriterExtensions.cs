using System.Buffers;

using LaquaiLib.Text;

namespace LaquaiLib.Extensions;

/// <summary>
/// Provides extensions for the <see cref="TextWriter"/> type.
/// </summary>
public static class TextWriterExtensions
{
    /// <summary>
    /// Represents an interpolated string handler that writes directly to a <see cref="TextWriter"/>.
    /// </summary>
    [InterpolatedStringHandler]
    public readonly struct TextWriterInterpolatedStringHandler(int literalLength, int formattedCount, TextWriter tw)
    {
        private void WriteSpaces(int count)
        {
            if (count <= 0)
                return;
            if (count <= 16)
            {
                for (var i = 0; i < count; i++)
                    tw.Write(' ');
                return;
            }

            char[] spacesBuffer = null;
            var spaces = count < 512 ? stackalloc char[count] : (spacesBuffer = ArrayPool<char>.Shared.Rent(count)).AsSpan(0, count);
            try
            {
                spaces.Fill(' ');
                tw.Write(spaces);
            }
            finally
            {
                if (spacesBuffer != null)
                    ArrayPool<char>.Shared.Return(spacesBuffer);
            }
        }
        private void WriteValueCore(ReadOnlySpan<char> s, int alignment = 0)
        {
            WriteSpaces(alignment - s.Length);
            tw.Write(s);
            WriteSpaces(-alignment - s.Length);
        }
        /// <summary>
        /// Writes a literal <see langword="string"/> to the <see cref="TextWriter"/>.
        /// </summary>
        public void AppendLiteral(string s) => WriteValueCore(s);
        /// <summary>
        /// Writes a literal <see langword="ReadOnlySpan{char}"/> to the <see cref="TextWriter"/>.
        /// </summary>
        public void AppendLiteral(ReadOnlySpan<char> s) => WriteValueCore(s);
        /// <summary>
        /// Formats a <see langword="string"/> into the <see cref="TextWriter"/>.
        /// </summary>
        public void AppendFormatted(string s, int alignment = 0, string format = null) => WriteValueCore(s.AsSpan(), alignment);
        /// <summary>
        /// Formats a <see langword="ReadOnlySpan{char}"/> into the <see cref="TextWriter"/>.
        /// </summary>
        public void AppendFormatted(ReadOnlySpan<char> s, int alignment = 0, string format = null) => WriteValueCore(s, alignment);
        /// <summary>
        /// Formats an instance of <typeparamref name="T"/> into the <see cref="TextWriter"/>.
        /// Arguments whose type <typeparamref name="T"/> implements <see cref="ISpanFormattable"/> or <see cref="IFormattable"/> will use those interface implementations to format the value.
        /// </summary>
        public void AppendFormatted<T>(T t, int alignment = 0, string format = null)
        {
            if (t is null)
            {
                WriteValueCore("", alignment);
                return;
            }

            // Avoid pattern matching (which boxes value types)
#pragma warning disable IDE0038 // Use pattern matching
            if (t is ISpanFormattable)
#pragma warning restore IDE0038 // Use pattern matching
            {
                var size = FormattingHelpers.RentStartSize;
                var pool = ArrayPool<byte>.Shared;
                var arr = pool.Rent(size, out Span<char> buf);
                try
                {
                    for (var i = 0; i < FormattingHelpers.MaxRetries; i++)
                    {
                        if (!((ISpanFormattable)t).TryFormat(buf, out var written, format, tw.FormatProvider))
                        {
                            pool.Return(arr);
                            arr = null;
                            arr = pool.Rent(size <<= 1, out buf);
                        }
                        else
                        {
                            WriteValueCore(buf[..written], alignment);
                            return;
                        }
                    }
                }
                finally
                {
                    if (arr != null)
                        pool.Return(arr);
                }
            }

            if (t is IFormattable formattable)
            {
                var s = formattable.ToString(format, tw.FormatProvider);
                WriteValueCore(s, alignment);
                return;
            }

            WriteValueCore(t.ToString() ?? "", alignment);
        }
    }

    extension(TextWriter tw)
    {
        /// <summary>
        /// Writes an interpolated string to the <see cref="TextWriter"/> using the <see cref="TextWriterInterpolatedStringHandler"/>.
        /// </summary>
        /// <param name="handler">The <see cref="TextWriterInterpolatedStringHandler"/> that formats the interpolated string. Compiler use only.</param>
        public void WriteInterpolated([InterpolatedStringHandlerArgument("tw")] ref TextWriterInterpolatedStringHandler handler) => GC.KeepAlive(tw);
        /// <summary>
        /// Writes an interpolated string followed by a line terminator to the <see cref="TextWriter"/> using the <see cref="TextWriterInterpolatedStringHandler"/>.
        /// </summary>
        /// <param name="handler">The <see cref="TextWriterInterpolatedStringHandler"/> that formats the interpolated string. Compiler use only.</param>
        public void WriteLineInterpolated([InterpolatedStringHandlerArgument("tw")] ref TextWriterInterpolatedStringHandler handler) => tw.WriteLine();
    }
}
