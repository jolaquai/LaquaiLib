using System.Buffers;
using System.Globalization;
using System.Reflection;
using System.Text.Json.Serialization;

namespace LaquaiLib.Text.Json;

/// <summary>
/// Implements a <see cref="JsonConverter{T}"/> that is able to serialize and deserialize most numeric or other values that are either <see cref="IUtf8SpanParsable{TSelf}"/>, <see cref="ISpanParsable{TSelf}"/>, <see cref="IUtf8SpanFormattable"/>, <see cref="ISpanFormattable"/>, <see cref="IParsable{TSelf}"/> or <see cref="IConvertible"/> values.
/// </summary>
/// <typeparam name="T">The type of the value to convert.</typeparam>
public class FlexibleUnmanagedTypeConverter<T> : JsonConverter<T>
{
    private delegate bool Utf8ParseDelegate(ReadOnlySpan<byte> utf8Text, IFormatProvider provider, out T result);
    private delegate bool SpanParseDelegate(ReadOnlySpan<char> text, IFormatProvider provider, out T result);
    private delegate bool StringParseDelegate(string text, IFormatProvider provider, out T result);

    // Static interface members can't be invoked without constraining T to the exact interface, so bind them once via reflection instead.
    private static readonly Utf8ParseDelegate _utf8Parse = CreateParseDelegate<Utf8ParseDelegate>([typeof(ReadOnlySpan<byte>), typeof(IFormatProvider), typeof(T).MakeByRefType()]);
    private static readonly SpanParseDelegate _spanParse = CreateParseDelegate<SpanParseDelegate>([typeof(ReadOnlySpan<char>), typeof(IFormatProvider), typeof(T).MakeByRefType()]);
    private static readonly StringParseDelegate _stringParse = CreateParseDelegate<StringParseDelegate>([typeof(string), typeof(IFormatProvider), typeof(T).MakeByRefType()]);

    private static TDelegate CreateParseDelegate<TDelegate>(Type[] parameterTypes) where TDelegate : Delegate
    {
        var method = typeof(T).GetMethod("TryParse", BindingFlags.Public | BindingFlags.Static, null, parameterTypes, null);
        return method is null ? null : (TDelegate)Delegate.CreateDelegate(typeof(TDelegate), method);
    }

    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.Number:
                // Numbers can't contain escapes, so the raw UTF-8 span is always the exact number text; this also avoids the precision loss of routing through double.
                if (_utf8Parse is not null && _utf8Parse(reader.ValueSpan, CultureInfo.InvariantCulture, out var utf8Result))
                    return utf8Result;
                return (T)Convert.ChangeType(reader.GetDouble(), typeof(T), CultureInfo.InvariantCulture);

            case JsonTokenType.String:
            {
                var stringValue = reader.GetString();
                if (stringValue is null)
                    return default;

                if (_spanParse is not null && _spanParse(stringValue, CultureInfo.InvariantCulture, out var spanResult))
                    return spanResult;
                if (_stringParse is not null && _stringParse(stringValue, CultureInfo.InvariantCulture, out var stringResult))
                    return stringResult;
                if (double.TryParse(stringValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var doubleResult))
                    return (T)Convert.ChangeType(doubleResult, typeof(T), CultureInfo.InvariantCulture);
                return default;
            }

            default:
                return default;
        }
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        if (value is IUtf8SpanFormattable utf8Formattable)
        {
            ArrayPool<byte> pool = null;
            byte[] arr = null;
            Span<byte> buffer = stackalloc byte[64];

            int written;
            while (!utf8Formattable.TryFormat(buffer, out written, default, CultureInfo.InvariantCulture))
            {
                if (pool is null)
                    pool = ArrayPool<byte>.Shared;
                else if (arr is not null)
                    pool.Return(arr);

                arr = pool.Rent(buffer.Length * 2);
                buffer = arr;
            }

            writer.WriteStringValue(buffer[..written]);
        }
        else if (value is ISpanFormattable spanFormattable)
        {
            ArrayPool<char> pool = null;
            char[] arr = null;
            Span<char> buffer = stackalloc char[64];

            int written;
            while (!spanFormattable.TryFormat(buffer, out written, default, CultureInfo.InvariantCulture))
            {
                if (pool is null)
                    pool = ArrayPool<char>.Shared;
                else if (arr is not null)
                    pool.Return(arr);

                arr = pool.Rent(buffer.Length * 2);
                buffer = arr;
            }
            writer.WriteStringValue(buffer[..written]);
        }
        else if (value is IConvertible convertible)
            writer.WriteStringValue(convertible.ToString(CultureInfo.InvariantCulture));
        else
            writer.WriteStringValue(value?.ToString());
    }
}
