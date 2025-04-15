using System.Text.Json;
using System.Text.Json.Serialization;

namespace LaquaiLib.Text.Json;

/// <summary>
/// Implements a <see cref="JsonConverter{T}"/> that is able to read and write most numeric or otherwise <see cref="IParsable{TSelf}"/> and <see cref="IConvertible"/> values.
/// </summary>
/// <typeparam name="T">The type of the value to convert.</typeparam>
public class FlexibleUnmanagedTypeConverter<T> : JsonConverter<T> where T : IParsable<T>, IConvertible
{
    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var stringValue = reader.GetString();
            if (T.TryParse(stringValue, null, out var result))
            {
                return result;
            }

            // Handle empty strings or other special cases
            return default;
        }
        else if (reader.TokenType == JsonTokenType.Number)
        {
            return (T)Convert.ChangeType(reader.GetDouble(), typeof(T));
        }

        // Return default or throw custom exception
        return default;
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options) => writer.WriteNumberValue(Convert.ToDouble(value));
}
