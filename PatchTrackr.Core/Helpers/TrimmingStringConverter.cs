
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PatchTrackr.Core.Helpers;

public class TrimmingStringConverter : JsonConverter<string>
{
    public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.String:
                return reader.GetString()?.Trim();

            case JsonTokenType.Number:
                // Handles both integers and floating-point values
                if (reader.TryGetInt64(out long longValue))
                    return longValue.ToString();
                else if (reader.TryGetDouble(out double doubleValue))
                    return doubleValue.ToString(System.Globalization.CultureInfo.InvariantCulture);
                else
                    return null;

            case JsonTokenType.True:
                return "true";

            case JsonTokenType.False:
                return "false";

            case JsonTokenType.Null:
                return null;

            default:
                return null; // fallback
        }
    }

    public override void Write(Utf8JsonWriter writer, string? value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value?.Trim());
    }
}
