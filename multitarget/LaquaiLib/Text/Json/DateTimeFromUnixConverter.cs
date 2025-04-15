using System.Text.Json;
using System.Text.Json.Serialization;

namespace LaquaiLib.Text.Json;

/// <summary>
/// Implements a <see cref="JsonConverter{T}"/> that is able to read and write <see cref="DateTime"/> values as Unix timestamps.
/// </summary>
public class DateTimeFromUnixConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Number)
        {
            var unixTime = reader.GetInt64();
            return DateTimeOffset.FromUnixTimeSeconds(unixTime).DateTime;
        }
        throw new JsonException();
    }
    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        var unixTime = new DateTimeOffset(value).ToUnixTimeSeconds();
        writer.WriteNumberValue(unixTime);
    }
}
