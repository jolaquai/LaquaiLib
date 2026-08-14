using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

using LaquaiLib.Extensions;

namespace LaquaiLib.Text;

/// <summary>
/// Implements a <see cref="TextWriter"/> that uses an <see cref="ArrayBufferWriter{T}"/> to buffer the written characters.
/// </summary>
/// <param name="capacity">A starting capacity for the internal buffer.</param>
/// <param name="encoding">The <see cref="System.Text.Encoding"/> to use when encoding the <see langword="char"/>s written to this <see cref="TextWriter"/> to <see langword="byte"/>s.</param>
public class BufferTextWriter(int capacity = 2048, Encoding encoding = null) : TextWriter
{
    private const string ArgumentNullException_AttemptedNullStringNullWrite = "Cannot write a null value when NullString is itself null.";
    private const string ArgumentNullException_NoFallbackEncoding = "No encoding was specified for this call and the instance's encoding was also null.";
    private const string InvalidOperationException_BufferMutatedWhileEncoding = "The instance was written to while encoding data.";

    private readonly ArrayBufferWriter<char> _buffer = new ArrayBufferWriter<char>(capacity);

    /// <summary>
    /// Gets a <see cref="ReadOnlySpan{T}"/> of <see langword="char"/> around the characters written so far.
    /// </summary>
    /// <returns>The <see cref="ReadOnlySpan{T}"/> of <see langword="char"/>.</returns>
    public ReadOnlySpan<char> Span => _buffer.WrittenSpan;
    /// <summary>
    /// Gets a <see cref="ReadOnlyMemory{T}"/> of <see langword="char"/> around the characters written so far.
    /// </summary>
    /// <returns>The <see cref="ReadOnlyMemory{T}"/> of <see langword="char"/>.</returns>
    public ReadOnlyMemory<char> Memory => _buffer.WrittenMemory;

    #region TextWriter
    /// <summary>
    /// Gets the <see cref="System.Text.Encoding"/> to use when encoding the <see langword="char"/>s written to this <see cref="TextWriter"/> to <see langword="byte"/>s.
    /// This has no effect when writing to this <see cref="TextWriter"/> directly.
    /// </summary>
    public override Encoding Encoding { get; } = encoding;
    /// <inheritdoc/>
    public override IFormatProvider FormatProvider => CultureInfo.CurrentCulture;
    /// <inheritdoc/>
    public override string NewLine { get; set; } = Environment.NewLine;
    /// <summary>
    /// Gets or sets the <see langword="string"/> that is written to the buffer when a <see langword="null"/> value is written.
    /// Defaults to <c>"null"</c>. If explicitly set to <see langword="null"/>, an exception is thrown when attempting to write a <see langword="null"/> value.
    /// </summary>
    public string NullString { get; set; } = "null";

    /// <inheritdoc/>
    public override void Flush() { }
    /// <inheritdoc/>
    public override Task FlushAsync() => Task.CompletedTask;
    /// <inheritdoc/>
    public override Task FlushAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    /// <inheritdoc/>
    public override string ToString() => new string(_buffer.WrittenSpan);
    /// <inheritdoc/>
    public override void Write(bool value)
    {
        _ = value.TryFormat(_buffer.GetSpan(5), out var written);
        _buffer.Advance(written);
    }
    /// <inheritdoc/>
    public override void Write(char value)
    {
        _buffer.GetSpan(1)[0] = value;
        _buffer.Advance(1);
    }
    /// <inheritdoc/>
    public override void Write(char[] buffer)
    {
        buffer.CopyTo(_buffer.GetSpan(buffer.Length));
        _buffer.Advance(buffer.Length);
    }
    /// <inheritdoc/>
    public override void Write(char[] buffer, int index, int count)
    {
        buffer.AsSpan(index, count).CopyTo(_buffer.GetSpan(count));
        _buffer.Advance(count);
    }
    /// <inheritdoc/>
    public override void Write(decimal value)
    {
        _ = value.TryFormat(_buffer.GetSpan(128), out var written);
        _buffer.Advance(written);
    }
    /// <inheritdoc/>
    public override void Write(double value)
    {
        _ = value.TryFormat(_buffer.GetSpan(50), out var written);
        _buffer.Advance(written);
    }
    /// <inheritdoc/>
    public override void Write(int value)
    {
        _ = value.TryFormat(_buffer.GetSpan(20), out var written);
        _buffer.Advance(written);
    }
    /// <inheritdoc/>
    public override void Write(long value)
    {
        _ = value.TryFormat(_buffer.GetSpan(30), out var written);
        _buffer.Advance(written);
    }
    /// <inheritdoc/>
    public override void Write(object value)
    {
        switch (value)
        {
            case null:
            {
                var nullString = NullString ?? throw new ArgumentNullException(nameof(value), ArgumentNullException_AttemptedNullStringNullWrite);
                nullString.CopyTo(_buffer.GetSpan(nullString.Length));
                _buffer.Advance(nullString.Length);
                break;
            }

            case string str:
                Write(str);
                break;
            case ReadOnlyMemory<char> rom:
                Write(rom);
                break;
            case ISpanFormattable spanFormattable:
            {
                if (spanFormattable.TryFormat(_buffer.GetSpan(Config.MaxStackallocSize), out var written, format: null, FormatProvider))
                {
                    _buffer.Advance(written);
                }
                else
                {
                    var formatted = spanFormattable.ToString(null, FormatProvider);
                    formatted.CopyTo(_buffer.GetSpan(formatted.Length));
                    _buffer.Advance(formatted.Length);
                }
                break;
            }
            default:
                var s = value.ToString();
                s.CopyTo(_buffer.GetSpan(s.Length));
                _buffer.Advance(s.Length);
                break;
        }
    }
    /// <inheritdoc/>
    public override void Write(ReadOnlySpan<char> buffer)
    {
        buffer.CopyTo(_buffer.GetSpan(buffer.Length));
        _buffer.Advance(buffer.Length);
    }
    /// <inheritdoc cref="Write(ReadOnlySpan{char})"/>
    public void Write(ReadOnlyMemory<char> buffer)
    {
        buffer.Span.CopyTo(_buffer.GetSpan(buffer.Length));
        _buffer.Advance(buffer.Length);
    }
    /// <inheritdoc/>
    public override void Write(float value)
    {
        _ = value.TryFormat(_buffer.GetSpan(25), out var written);
        _buffer.Advance(written);
    }
    /// <inheritdoc/>
    public override void Write(string value)
    {
        value.CopyTo(_buffer.GetSpan(value.Length));
        _buffer.Advance(value.Length);
    }
    /// <inheritdoc/>
    public override void Write([StringSyntax(StringSyntaxAttribute.CompositeFormat)] string format, object arg0) => Write(format, [arg0]);
    /// <inheritdoc/>
    public override void Write([StringSyntax(StringSyntaxAttribute.CompositeFormat)] string format, object arg0, object arg1) => Write(format, [arg0, arg1]);
    /// <inheritdoc/>
    public override void Write([StringSyntax(StringSyntaxAttribute.CompositeFormat)] string format, object arg0, object arg1, object arg2) => Write(format, [arg0, arg1, arg2]);
    /// <inheritdoc/>
    public override void Write([StringSyntax(StringSyntaxAttribute.CompositeFormat)] string format, params object[] arg) => Write(format, (ReadOnlySpan<object>)arg);
    /// <inheritdoc/>
    public override unsafe void Write([StringSyntax(StringSyntaxAttribute.CompositeFormat)] string format, params scoped ReadOnlySpan<object> arg)
    {
        var comp = CompositeFormat.Parse(format);
        var segments = UnsafeUtils.Accessors.CompositeFormatAccessors._segments(comp);

        scoped Span<char> temp = stackalloc char[Config.MaxStackallocSize / 2];
        for (var i = 0; i < segments.Length; i++)
        {
            var (Literal, ArgIndex, Alignment, ArgFormat) = segments[i];

            switch (Literal)
            {
                case not null:
                    if (Literal.Length > 0)
                    {
                        Literal.CopyTo(_buffer.GetSpan(Literal.Length));
                        _buffer.Advance(Literal.Length);
                    }
                    break;
                default:
                    if (Alignment <= 0) // This branch lets us skip intermediate allocations
                    {
                        var advanced = 0;

                        var argument = arg[ArgIndex];
                        switch (argument)
                        {
                            case IFormattable formattable:
                            {
                                if (formattable is ISpanFormattable spanFormattable)
                                {
                                    // Try the ISpanFormattable first, since it's the most efficient

                                    // The performance of this branch largely depends on the state of _buffer
                                    // If we were preceded by stupid writes that requested large buffers, but then actually wrote small sequences, the request for the final 2048 might result in a much larger span than that, which means TryFormat will succeed
                                    // For example, if the buffer has a FreeCapacity of 2047 and we request 2048, the backing store resizes to 4095 and we're given that entire buffer to write into
                                    if (spanFormattable.TryFormat(_buffer.GetSpan(_buffer.FreeCapacity), out advanced, ArgFormat, FormatProvider))
                                    {
                                        _buffer.Advance(advanced);
                                    }
                                    else
                                    {
                                        advanced = -1;
                                    }
                                }

                                if (advanced == -1)
                                {
                                    var formatted = formattable.ToString(ArgFormat, FormatProvider);
                                    formatted.CopyTo(_buffer.GetSpan(formatted.Length));
                                    _buffer.Advance(formatted.Length);
                                }

                                break;
                            }

                            default:
                            {
                                var str = argument?.ToString() ?? NullString ?? throw new ArgumentNullException($"arg[{ArgIndex}]", ArgumentNullException_AttemptedNullStringNullWrite);
                                str.CopyTo(_buffer.GetSpan(str.Length));
                                _buffer.Advance(str.Length);
                                break;
                            }
                        }

                        // Negative alignment means left-align
                        if (Alignment < 0 && (Alignment += advanced) > 0)
                        {
                            // Writing nothing means we just stick that many spaces into the buffer
                            Alignment = Math.Abs(Alignment);
                            // Explicit slice since the argument is only a size hint
                            _buffer.GetSpan(Alignment)[..Alignment].Fill(' ');
                            _buffer.Advance(Alignment);
                        }
                    }
                    else // Alignment > 0
                    {
                        var toWrite = 0;

                        scoped ReadOnlySpan<char> buffer = default;
                        var argument = arg[ArgIndex];
                        switch (argument)
                        {
                            case IFormattable formattable:
                            {
                                if (formattable is ISpanFormattable spanFormattable)
                                {
                                    // To facilitate semi-efficient alignment, we'll try to write into the buffer first
                                    // Ideally, we'd have enough space to write the entire thing twice + the alignment, but we can't guarantee that, so we'll have to try and hope
                                    if (spanFormattable.TryFormat(temp, out toWrite, ArgFormat, FormatProvider))
                                    {
                                        buffer = temp[..toWrite];
                                    }
                                    else
                                    {
                                        toWrite = -1;
                                    }
                                }

                                // If we wrote nothing, use the IFormattable approach instead
                                if (toWrite == -1)
                                {
                                    buffer = formattable.ToString(ArgFormat, FormatProvider);
                                }

                                break;
                            }

                            default:
                                buffer = argument?.ToString() ?? NullString ?? throw new ArgumentNullException($"arg[{ArgIndex}]", ArgumentNullException_AttemptedNullStringNullWrite);
                                break;
                        }

                        var total = int.Max(buffer.Length, Alignment);
                        var destination = _buffer.GetSpan(total);
                        switch (Alignment - buffer.Length)
                        {
                            case var sep and > 0:
                                destination[..sep].Fill(' ');
                                buffer.CopyTo(destination[sep..]);
                                break;
                            default:
                                buffer.CopyTo(destination);
                                break;
                        }
                        _buffer.Advance(total);
                    }

                    break;
            }
        }
    }
    /// <inheritdoc/>
    public override void Write(StringBuilder value)
    {
        var chunks = value.GetChunks();
        foreach (var chunk in chunks)
        {
            chunk.CopyTo(_buffer.GetMemory(chunk.Length));
            _buffer.Advance(chunk.Length);
        }
    }
    /// <inheritdoc/>
    public override void Write(uint value)
    {
        _ = value.TryFormat(_buffer.GetSpan(14), out var written);
        _buffer.Advance(written);
    }
    /// <inheritdoc/>
    public override void Write(ulong value)
    {
        _ = value.TryFormat(_buffer.GetSpan(30), out var written);
        _buffer.Advance(written);
    }
    /// <inheritdoc/>
    public override void WriteLine()
    {
        NewLine.CopyTo(_buffer.GetSpan(NewLine.Length));
        _buffer.Advance(NewLine.Length);
    }
    /// <inheritdoc/>
    public override void WriteLine(bool value)
    {
        Write(value);
        WriteLine();
    }
    /// <inheritdoc/>
    public override void WriteLine(char value)
    {
        Write(value);
        WriteLine();
    }
    /// <inheritdoc/>
    public override void WriteLine(char[] buffer)
    {
        Write(buffer);
        WriteLine();
    }
    /// <inheritdoc/>
    public override void WriteLine(char[] buffer, int index, int count)
    {
        Write(buffer, index, count);
        WriteLine();
    }
    /// <inheritdoc/>
    public override void WriteLine(decimal value)
    {
        Write(value);
        WriteLine();
    }
    /// <inheritdoc/>
    public override void WriteLine(double value)
    {
        Write(value);
        WriteLine();
    }
    /// <inheritdoc/>
    public override void WriteLine(int value)
    {
        Write(value);
        WriteLine();
    }
    /// <inheritdoc/>
    public override void WriteLine(long value)
    {
        Write(value);
        WriteLine();
    }
    /// <inheritdoc/>
    public override void WriteLine(object value)
    {
        Write(value);
        WriteLine();
    }
    /// <inheritdoc/>
    public override void WriteLine(ReadOnlySpan<char> buffer)
    {
        Write(buffer);
        WriteLine();
    }
    /// <inheritdoc/>
    public override void WriteLine(float value)
    {
        Write(value);
        WriteLine();
    }
    /// <inheritdoc/>
    public override void WriteLine(string value)
    {
        Write(value);
        WriteLine();
    }
    /// <inheritdoc/>
    public override void WriteLine([StringSyntax(StringSyntaxAttribute.CompositeFormat)] string format, object arg0) => WriteLine(format, [arg0]);
    /// <inheritdoc/>
    public override void WriteLine([StringSyntax(StringSyntaxAttribute.CompositeFormat)] string format, object arg0, object arg1) => WriteLine(format, [arg0, arg1]);
    /// <inheritdoc/>
    public override void WriteLine([StringSyntax(StringSyntaxAttribute.CompositeFormat)] string format, object arg0, object arg1, object arg2) => WriteLine(format, [arg0, arg1, arg2]);
    /// <inheritdoc/>
    public override void WriteLine([StringSyntax(StringSyntaxAttribute.CompositeFormat)] string format, params object[] arg) => WriteLine(format, (ReadOnlySpan<object>)arg);
    /// <inheritdoc/>
    public override void WriteLine([StringSyntax(StringSyntaxAttribute.CompositeFormat)] string format, params scoped ReadOnlySpan<object> arg)
    {
        Write(format, arg);
        WriteLine();
    }
    /// <inheritdoc/>
    public override void WriteLine(StringBuilder value)
    {
        Write(value);
        WriteLine();
    }
    /// <inheritdoc/>
    public override void WriteLine(uint value)
    {
        Write(value);
        WriteLine();
    }
    /// <inheritdoc/>
    public override void WriteLine(ulong value)
    {
        Write(value);
        WriteLine();
    }
    #endregion

    /// <summary>
    /// Encodes the characters written to this <see cref="TextWriter"/> to <see langword="byte"/>s and writes them to the specified <paramref name="stream"/>.
    /// </summary>
    /// <param name="stream">The <see cref="Stream"/> to write the encoded <see langword="byte"/>s to.</param>
    /// <param name="encoding">The <see cref="System.Text.Encoding"/> to use when encoding. If <see langword="null"/>, <see cref="Encoding"/> is used instead. If that is also <see langword="null"/>, an <see cref="ArgumentNullException"/> is thrown.</param>
    /// <param name="bufferSize">The size of the buffer to use when writing to the <paramref name="stream"/>. If <c>-1</c>, the default buffer size is used.</param>
    /// <returns>The number of <see langword="byte"/>s written to the <paramref name="stream"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="encoding"/> is <see langword="null"/> and the instance's <see cref="Encoding"/> is also <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the instance is written to while this method is encoding data.</exception>
    public int WriteTo(Stream stream, Encoding encoding = null, int bufferSize = -1)
    {
        var encoder = (encoding ?? Encoding ?? throw new ArgumentNullException(nameof(encoding), ArgumentNullException_NoFallbackEncoding)).GetEncoder();

        var bytesWritten = 0;
        var charsConsumed = 0;
        var chars = _buffer.WrittenSpan;
        var charLength = chars.Length;

        // Ignore bufferSize if it's smaller than what we can afford to stackalloc, otherwise obey the user
        var bufferActual = bufferSize <= Config.MaxStackallocSize ? Config.MaxStackallocSize : Math.Clamp(bufferSize, Config.MaxStackallocSize, bufferSize);
        Span<byte> scratch = bufferActual == Config.MaxStackallocSize ? stackalloc byte[bufferActual] : new byte[bufferActual];
        bool completed;
        var flush = false;
        do
        {
            if (charLength != _buffer.WrittenCount)
            {
                throw new InvalidOperationException(InvalidOperationException_BufferMutatedWhileEncoding);
            }

            encoder.Convert(chars[charsConsumed..], scratch, flush, out var charsConsumedLocal, out var bytesWrittenLocal, out completed);
            switch (bytesWrittenLocal)
            {
                case 0 when !flush:
                    // No more characters to write
                    flush = true;
                    continue;
                case > 0:
                    stream.Write(scratch[..bytesWrittenLocal]);
                    break;
            }
            bytesWritten += bytesWrittenLocal;
            charsConsumed += charsConsumedLocal;

            flush = charsConsumed >= charLength;
        } while (!completed);

        return bytesWritten;
    }
}