using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;

using LaquaiLib.Core;
using LaquaiLib.Extensions;

namespace LaquaiLib.Text;

/// <summary>
/// Implements a <see cref="TextWriter"/> that uses an <see cref="ArrayBufferWriter{T}"/> to buffer the written characters.
/// </summary>
/// <param name="capacity">A starting capacity for the internal buffer.</param>
/// <param name="encoding">The encoding to use for the <see cref="TextWriter"/>. Defaults to <see cref="Encoding.Default"/>.</param>
public class BufferTextWriter(int capacity = 2048) : TextWriter
{
    private const string ArgumentNullException_AttemptedNullStringNullWrite = "Cannot write a null value when NullString is itself null.";
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
    /// Always returns <see langword="null"/>. <see cref="System.Text.Encoding"/> is not supported when writing <see langword="char"/>s.
    /// </summary>
    public override Encoding Encoding { get; } = null;
    public override IFormatProvider FormatProvider => CultureInfo.CurrentCulture;
    public override string NewLine { get; set; } = Environment.NewLine;
    /// <summary>
    /// Gets or sets the <see langword="string"/> that is written to the buffer when a <see cref="null"/> value is written.
    /// Defaults to <c>"null"</c>. If explicitly set to <see langword="null"/>, an exception is thrown when attempting to write a <see langword="null"/> value.
    /// </summary>
    public string NullString { get; set; } = "null";

    public override void Flush() { }
    public override Task FlushAsync() => Task.CompletedTask;
    public override Task FlushAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    public override string ToString() => new string(_buffer.WrittenSpan);
    public override void Write(bool value)
    {
        value.TryFormat(_buffer.GetSpan(5), out var written);
        _buffer.Advance(written);
    }
    public override void Write(char value)
    {
        _buffer.GetSpan(1)[0] = value;
        _buffer.Advance(1);
    }
    public override void Write(char[] buffer)
    {
        buffer.CopyTo(_buffer.GetSpan(buffer.Length));
        _buffer.Advance(buffer.Length);
    }
    public override void Write(char[] buffer, int index, int count)
    {
        buffer.AsSpan(index, count).CopyTo(_buffer.GetSpan(count));
        _buffer.Advance(count);
    }
    public override void Write(decimal value)
    {
        value.TryFormat(_buffer.GetSpan(128), out var written);
        _buffer.Advance(written);
    }
    public override void Write(double value)
    {
        value.TryFormat(_buffer.GetSpan(50), out var written);
        _buffer.Advance(written);
    }
    public override void Write(int value)
    {
        value.TryFormat(_buffer.GetSpan(20), out var written);
        _buffer.Advance(written);
    }
    public override void Write(long value)
    {
        value.TryFormat(_buffer.GetSpan(30), out var written);
        _buffer.Advance(written);
    }
    public override void Write(object value)
    {
        if (value is null)
        {
            var nullString = NullString ?? throw new ArgumentNullException(nameof(value), ArgumentNullException_AttemptedNullStringNullWrite);
            nullString.CopyTo(_buffer.GetSpan(nullString.Length));
            _buffer.Advance(nullString.Length);
        }
        else if (value is string str)
        {
            Write(str);
        }
        else if (value is ReadOnlyMemory<char> rom)
        {
            Write(rom);
        }
        else
        {
            str = value.ToString();
            str.CopyTo(_buffer.GetSpan(str.Length));
            _buffer.Advance(str.Length);
        }
    }
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
    public override void Write(float value)
    {
        value.TryFormat(_buffer.GetSpan(25), out var written);
        _buffer.Advance(written);
    }
    public override void Write(string value)
    {
        value.CopyTo(_buffer.GetSpan(value.Length));
        _buffer.Advance(value.Length);
    }
    public override void Write([StringSyntax("CompositeFormat")] string format, object arg0) => Write(format, [arg0]);
    public override void Write([StringSyntax("CompositeFormat")] string format, object arg0, object arg1) => Write(format, [arg0, arg1]);
    public override void Write([StringSyntax("CompositeFormat")] string format, object arg0, object arg1, object arg2) => Write(format, [arg0, arg1, arg2]);
    public override void Write([StringSyntax("CompositeFormat")] string format, params object[] arg) => Write(format, (ReadOnlySpan<object>)arg);
    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_segments")]
    private static extern ref (string Literal, int ArgIndex, int Alignment, string Format)[] FormatSegments(CompositeFormat comp);
    public override unsafe void Write([StringSyntax("CompositeFormat")] string format, params scoped ReadOnlySpan<object> arg)
    {
        var comp = CompositeFormat.Parse(format);
        var segments = FormatSegments(comp);

        // Prepare some scratch space
        var temp = stackalloc char[Configuration.MaxStackallocSize];

        for (var i = 0; i < segments.Length; i++)
        {
            var (Literal, ArgIndex, Alignment, ArgFormat) = segments[i];

            if (Literal is not null)
            {
                if (Literal.Length > 0)
                {
                    Literal.CopyTo(_buffer.GetSpan(Literal.Length));
                    _buffer.Advance(Literal.Length);
                }
            }
            else if (Alignment <= 0) // This branch lets us skip intermediate allocations
            {
                var advanced = 0;

                var argument = arg[ArgIndex];
                if (argument is IFormattable formattable)
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
                }
                else
                {
                    var str = argument?.ToString() ?? NullString ?? throw new ArgumentNullException($"arg[{ArgIndex}]", ArgumentNullException_AttemptedNullStringNullWrite);
                    str.CopyTo(_buffer.GetSpan(str.Length));
                    _buffer.Advance(str.Length);
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

                ReadOnlySpan<char> buffer = default;
                var argument = arg[ArgIndex];
                if (argument is IFormattable formattable)
                {
                    if (formattable is ISpanFormattable spanFormattable)
                    {
                        var tempSpan = new Span<char>(temp, Configuration.MaxStackallocSize);
                        // To facilitate semi-efficient alignment, we'll try to write into the buffer first
                        // Ideally, we'd have enough space to write the entire thing twice + the alignment, but we can't guarantee that, so we'll have to try and hope
                        if (spanFormattable.TryFormat(tempSpan, out toWrite, ArgFormat, FormatProvider))
                        {
                            buffer = new Span<char>(temp, toWrite);
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
                }
                else
                {
                    buffer = argument?.ToString() ?? NullString ?? throw new ArgumentNullException($"arg[{ArgIndex}]", ArgumentNullException_AttemptedNullStringNullWrite);
                }

                var total = int.Max(buffer.Length, Alignment);
                var destination = _buffer.GetSpan(total);
                if (Alignment - buffer.Length is var sep and > 0)
                {
                    destination[..sep].Fill(' ');
                    buffer.CopyTo(destination[sep..]);
                }
                else
                {
                    buffer.CopyTo(destination);
                }
                _buffer.Advance(total);
            }
        }
    }
    public override void Write(StringBuilder value)
    {
        var chunks = value.GetChunks();
        foreach (var chunk in chunks)
        {
            chunk.CopyTo(_buffer.GetMemory(chunk.Length));
            _buffer.Advance(chunk.Length);
        }
    }
    public override void Write(uint value)
    {
        value.TryFormat(_buffer.GetSpan(14), out var written);
        _buffer.Advance(written);
    }
    public override void Write(ulong value)
    {
        value.TryFormat(_buffer.GetSpan(30), out var written);
        _buffer.Advance(written);
    }
    public override void WriteLine()
    {
        NewLine.CopyTo(_buffer.GetSpan(NewLine.Length));
        _buffer.Advance(NewLine.Length);
    }
    public override void WriteLine(bool value)
    {
        Write(value);
        WriteLine();
    }
    public override void WriteLine(char value)
    {
        Write(value);
        WriteLine();
    }
    public override void WriteLine(char[] buffer)
    {
        Write(buffer);
        WriteLine();
    }
    public override void WriteLine(char[] buffer, int index, int count)
    {
        Write(buffer, index, count);
        WriteLine();
    }
    public override void WriteLine(decimal value)
    {
        Write(value);
        WriteLine();
    }
    public override void WriteLine(double value)
    {
        Write(value);
        WriteLine();
    }
    public override void WriteLine(int value)
    {
        Write(value);
        WriteLine();
    }
    public override void WriteLine(long value)
    {
        Write(value);
        WriteLine();
    }
    public override void WriteLine(object value)
    {
        Write(value);
        WriteLine();
    }
    public override void WriteLine(ReadOnlySpan<char> buffer)
    {
        Write(buffer);
        WriteLine();
    }
    public override void WriteLine(float value)
    {
        Write(value);
        WriteLine();
    }
    public override void WriteLine(string value)
    {
        Write(value);
        WriteLine();
    }
    public override void WriteLine([StringSyntax("CompositeFormat")] string format, object arg0) => WriteLine(format, [arg0]);
    public override void WriteLine([StringSyntax("CompositeFormat")] string format, object arg0, object arg1) => WriteLine(format, [arg0, arg1]);
    public override void WriteLine([StringSyntax("CompositeFormat")] string format, object arg0, object arg1, object arg2) => WriteLine(format, [arg0, arg1, arg2]);
    public override void WriteLine([StringSyntax("CompositeFormat")] string format, params object[] arg) => WriteLine(format, (ReadOnlySpan<object>)arg);
    public override void WriteLine([StringSyntax("CompositeFormat")] string format, params scoped ReadOnlySpan<object> arg)
    {
        Write(format, arg);
        WriteLine();
    }
    public override void WriteLine(StringBuilder value)
    {
        Write(value);
        WriteLine();
    }
    public override void WriteLine(uint value)
    {
        Write(value);
        WriteLine();
    }
    public override void WriteLine(ulong value)
    {
        Write(value);
        WriteLine();
    }
    #endregion
}