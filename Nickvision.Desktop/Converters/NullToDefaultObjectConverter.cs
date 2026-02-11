using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nickvision.Desktop.Converters;

public class NullToDefaultObjectConverter<T> : JsonConverter<T>
{
    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return Activator.CreateInstance<T>();
        }
        return JsonSerializer.Deserialize<T>(ref reader, options) ?? Activator.CreateInstance<T>();
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options) => JsonSerializer.Serialize(writer, value, options);
}