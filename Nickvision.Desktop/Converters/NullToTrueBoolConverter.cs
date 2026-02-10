using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nickvision.Desktop.Converters;

public class NullToTrueBoolConverter : JsonConverter<bool>
{
    public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return true;
        }
        return JsonSerializer.Deserialize<bool>(ref reader, options);
    }

    public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options) => JsonSerializer.Serialize(writer, value, options);
}