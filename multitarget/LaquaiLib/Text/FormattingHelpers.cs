using System.Buffers;

namespace LaquaiLib.Text;

/// <summary>
/// Provides helpers for formatting <see cref="ISpanFormattable"/> and <see cref="IUtf8SpanFormattable"/> instances.
/// </summary>
public static class FormattingHelpers
{
    // starting small pays off for the common case where we're formatting BCL primitives
    // max rented size is (RentStartSize * (1 << MaxRetries))
    // for 256 and 9 or 16 and 13, that's 131,072
    // would love to stackalloc, but doing that would leave us with no way to actually give the result to the caller without copying anyway, so renting from the get-go is better
    private const int RentStartSize = 16;
    private const int MaxRetries = 13;

    public static ByteFormatResult<T> TryFormatBytes<T>(in T instance, ReadOnlySpan<char> format = default, IFormatProvider formatProvider = null) where T : IUtf8SpanFormattable
    {
        var size = RentStartSize;
        var pool = ArrayPool<byte>.Shared;
        var buf = pool.Rent(size);
        try
        {
            for (var i = 0; i < MaxRetries; i++)
            {
                if (!instance.TryFormat(buf, out var written, format, formatProvider))
                {
                    pool.Return(buf);
                    buf = pool.Rent(size <<= 1);
                }
                else
                {
                    return new ByteFormatResult<T>(instance, buf.AsSpan(0, written), buf, true);
                }
            }

            pool.Return(buf);
            return default;
        }
        catch
        {
            pool.Return(buf);
            throw;
        }
    }
    public static CharFormatResult<T> TryFormatChars<T>(in T instance, ReadOnlySpan<char> format = default, IFormatProvider formatProvider = null) where T : ISpanFormattable
    {
        var size = RentStartSize;
        var pool = ArrayPool<byte>.Shared;
        var arr = pool.Rent(size, out Span<char> buf);
        try
        {
            for (var i = 0; i < MaxRetries; i++)
            {
                if (!instance.TryFormat(buf, out var written, format, formatProvider))
                {
                    pool.Return(arr);
                    arr = pool.Rent(size <<= 1, out buf);
                }
                else
                {
                    return new CharFormatResult<T>(instance, buf[..written], arr, true);
                }
            }

            pool.Return(arr);
            return default;
        }
        catch
        {
            pool.Return(arr);
            throw;
        }
    }
}

/// <summary>
/// Encapsulates the result of a <see cref="FormattingHelpers.TryFormatChars{T}(in T, ReadOnlySpan{char}, IFormatProvider)"/> call.
/// </summary>
/// <typeparam name="T">The type of the instance that was formatted.</typeparam>
public ref struct CharFormatResult<T> : IDisposable
    where T : ISpanFormattable
{
    /// <summary>
    /// The value that was formatted.
    /// </summary>
    public readonly T Value;
    /// <summary>
    /// The chars that were written to the buffer.
    /// </summary>
    public readonly ReadOnlySpan<char> Span;
    /// <summary>
    /// Gets or sets the array that was rented from <see cref="ArrayPool{T}.Shared"/>.
    /// </summary>
    public byte[] RentedArray { get; private set; }
    /// <summary>
    /// Whether the formatting operation was successful.
    /// </summary>
    public readonly bool Success;

    internal CharFormatResult(T value, ReadOnlySpan<char> chars, byte[] rentedArray, bool success)
    {
        Value = value;
        Span = chars;
        RentedArray = rentedArray;
        Success = success;
    }

    // default leaves this as 0
    private int _live = 1;
    /// <inheritdoc/>
    public void Dispose()
    {
        if (Interlocked.Exchange(ref _live, 0) == 0)
            return;
        if (RentedArray is byte[] arr)
        {
            ArrayPool<byte>.Shared.Return(arr);
            RentedArray = null;
        }
    }
}
/// <summary>
/// Encapsulates the result of a <see cref="FormattingHelpers.TryFormatBytes{T}(in T, ReadOnlySpan{char}, IFormatProvider)"/> call.
/// </summary>
/// <typeparam name="T">The type of the instance that was formatted.</typeparam>
public ref struct ByteFormatResult<T> : IDisposable
    where T : IUtf8SpanFormattable
{
    /// <summary>
    /// The value that was formatted.
    /// </summary>
    public readonly T Value;
    /// <summary>
    /// The bytes that were written to the buffer.
    /// </summary>
    public readonly ReadOnlySpan<byte> Span;
    /// <summary>
    /// Gets or sets the array that was rented from <see cref="ArrayPool{T}.Shared"/>.
    /// </summary>
    public byte[] RentedArray { get; private set; }
    /// <summary>
    /// Whether the formatting operation was successful.
    /// </summary>
    public readonly bool Success;

    internal ByteFormatResult(T value, ReadOnlySpan<byte> bytes, byte[] rentedArray, bool success)
    {
        Value = value;
        Span = bytes;
        RentedArray = rentedArray;
        Success = success;
    }

    // default leaves this as 0
    private int _live = 1;
    /// <inheritdoc/>
    public void Dispose()
    {
        if (Interlocked.Exchange(ref _live, 0) == 0)
            return;
        if (RentedArray is byte[] arr)
        {
            ArrayPool<byte>.Shared.Return(arr);
            RentedArray = null;
        }
    }
}