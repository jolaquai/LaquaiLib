using System.Text.Json.Serialization;

namespace LaquaiLib.Text.Json;

/// <summary>
/// Implements a <see cref="JsonConverter{T}"/> that is able to serialize and deserialize <see cref="TimeSpan"/> values as seconds.
/// Values are subject to the (im)precision of <see cref="double"/> (accurate to the nearest millisecond).
/// </summary>
public class TimeSpanFromSecondsConverter : JsonConverter<TimeSpan>
{
    /// <summary>
    /// Reads a single value of type <see cref="TimeSpan"/> from a JSON number representing the total seconds, or a JSON string containing a <see cref="TimeSpan"/> representation or a number of seconds.
    /// </summary>
    /// <param name="reader">The <see cref="Utf8JsonReader"/> to read from.</param>
    /// <param name="typeToConvert">The type of the value to convert.</param>
    /// <param name="options">The serialization options to use.</param>
    /// <returns>The read (or parsed) <see cref="TimeSpan"/> value.</returns>
    /// <exception cref="JsonException">Thrown when the JSON token type is not a number or a string.</exception>
    public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.Number:
            {
                var seconds = reader.GetDouble();
                return TimeSpan.FromSeconds(seconds);
            }
            case JsonTokenType.String:
            {
                var stringValue = reader.GetString();
                if (stringValue is null)
                    return default;
                if (TimeSpan.TryParse(stringValue, out var timeSpan))
                    return timeSpan;
                if (double.TryParse(stringValue, out var seconds))
                    return TimeSpan.FromSeconds(seconds);
                return default;
            }
            default:
                throw new JsonException();
        }
    }
    /// <summary>
    /// Writes a single <see cref="TimeSpan"/> value as a JSON number representing the total seconds.
    /// </summary>
    /// <param name="writer">The <see cref="Utf8JsonWriter"/> to write to.</param>
    /// <param name="value">The <see cref="TimeSpan"/> value to write.</param>
    /// <param name="options">The <see cref="JsonSerializerOptions"/> to use.</param>
    public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options)
    {
        var seconds = value.TotalSeconds;
        writer.WriteNumberValue(seconds);
    }
}