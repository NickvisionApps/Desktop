using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Nickvision.Desktop.Helpers;

public static class ObjectExtensions
{
    extension<T>(T obj)
    {
        public T DeepCopy(JsonTypeInfo<T> jsonTypeInfo) => JsonSerializer.Deserialize(JsonSerializer.Serialize(obj, jsonTypeInfo), jsonTypeInfo)!;
    }
}

