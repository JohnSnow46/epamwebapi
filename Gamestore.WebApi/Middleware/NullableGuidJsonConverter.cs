using System.Text.Json;
using System.Text.Json.Serialization;

namespace Gamestore.WebApi.Middleware;

public class NullableGuidJsonConverter : JsonConverter<Guid?>
{
    public override Guid? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        if (reader.TokenType == JsonTokenType.String)
        {
            var stringValue = reader.GetString() ?? string.Empty;

            // Handle empty/whitespace strings as null
            if (string.IsNullOrWhiteSpace(stringValue))
            {
                return null;
            }

            // Try to parse the GUID
            if (Guid.TryParse(stringValue, out var guid))
            {
                return guid;
            }

            // If parsing failed, throw exception
            throw new JsonException($"Unable to convert '{stringValue}' to Guid.");
        }

        throw new JsonException($"Unexpected token type: {reader.TokenType}");
    }

    public override void Write(Utf8JsonWriter writer, Guid? value, JsonSerializerOptions options)
    {
        if (value.HasValue)
        {
            writer.WriteStringValue(value.Value.ToString());
        }
        else
        {
            writer.WriteNullValue();
        }
    }
}

public class EmptyStringToNullGuidConverter : JsonConverter<Guid?>
{
    public override Guid? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.Null:
                return null;

            case JsonTokenType.String:
                var stringValue = reader.GetString() ?? string.Empty;

                if (string.IsNullOrWhiteSpace(stringValue) ||
                    stringValue.Equals("null", StringComparison.OrdinalIgnoreCase) ||
                    stringValue.Equals("undefined", StringComparison.OrdinalIgnoreCase))
                {
                    return null;
                }

                if (Guid.TryParse(stringValue, out var guid))
                {
                    return guid;
                }

                return null;

            case JsonTokenType.Number:
                var number = reader.GetInt32();
                return number == 0 ? null : throw new JsonException($"Cannot convert number {number} to Guid");

            case JsonTokenType.StartObject:
            case JsonTokenType.EndObject:
            case JsonTokenType.StartArray:
            case JsonTokenType.EndArray:
            case JsonTokenType.PropertyName:
            case JsonTokenType.Comment:
            case JsonTokenType.True:
            case JsonTokenType.False:
            case JsonTokenType.None:
                throw new JsonException($"Cannot convert {reader.TokenType} to Guid");

            default:
                throw new JsonException($"Unexpected token type: {reader.TokenType}");
        }
    }

    public override void Write(Utf8JsonWriter writer, Guid? value, JsonSerializerOptions options)
    {
        if (value.HasValue)
        {
            writer.WriteStringValue(value.Value.ToString());
        }
        else
        {
            writer.WriteNullValue();
        }
    }
}