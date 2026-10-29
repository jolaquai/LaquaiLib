using System.Text.Json.Serialization;

namespace LaquaiLib.Text.Json;

/// <summary>
/// Implements a <see cref="JsonConverter{T}"/> that is able to serialize and deserialize <see cref="DateTime"/> values as Unix timestamps.
/// </summary>
public class DateTimeUnixConverter : JsonConverter<DateTime>
{
    /// <summary>
    /// Reads a single value of type <see cref="DateTime"/> from a JSON number representing a Unix timestamp, or a JSON string containing a Unix timestamp.
    /// </summary>
    /// <param name="reader">The <see cref="Utf8JsonReader"/> to read from.</param>
    /// <param name="typeToConvert">The type of the value to convert.</param>
    /// <param name="options">The serialization options to use.</param>
    /// <returns>The read (or parsed) <see cref="DateTime"/> value.</returns>
    /// <exception cref="JsonException">Thrown when the JSON token type is not a number or a string.</exception>
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.Number:
            {
                var unixTime = reader.GetInt64();
                return DateTimeOffset.FromUnixTimeSeconds(unixTime).DateTime;
            }
            case JsonTokenType.String:
            {
                var stringValue = reader.GetString();
                if (stringValue is null)
                    return default;
                if (long.TryParse(stringValue, out var unixTime))
                    return DateTimeOffset.FromUnixTimeSeconds(unixTime).DateTime;
                return default;
            }
            default:
            {
                throw new JsonException();
            }
        }
    }
    /// <summary>
    /// Writes a single <see cref="DateTime"/> value as a JSON number representing a Unix timestamp.
    /// </summary>
    /// <param name="writer">The <see cref="Utf8JsonWriter"/> to write to.</param>
    /// <param name="value">The <see cref="DateTime"/> value to write.</param>
    /// <param name="options">The <see cref="JsonSerializerOptions"/> to use.</param>
    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        var unixTime = new DateTimeOffset(value).ToUnixTimeSeconds();
        writer.WriteNumberValue(unixTime);
    }
}
