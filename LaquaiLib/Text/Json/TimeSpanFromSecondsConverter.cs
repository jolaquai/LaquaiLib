using System.Text.Json;
using System.Text.Json.Serialization;

namespace LaquaiLib.Text.Json;

/// <summary>
/// Implements a <see cref="JsonConverter{T}"/> that is able to read and write <see cref="TimeSpan"/> values as seconds.
/// Values are subject to the (im)precision of <see cref="double"/> (accurate to the nearest millisecond).
/// </summary>
public class TimeSpanFromSecondsConverter : JsonConverter<TimeSpan>
{
    public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Number)
        {
            var seconds = reader.GetDouble();
            return TimeSpan.FromSeconds(seconds);
        }
        throw new JsonException();
    }
    public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options)
    {
        var seconds = value.TotalSeconds;
        writer.WriteNumberValue(seconds);
    }
}