using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Nickvision.Desktop.Helpers;

public static class ObjectExtensions
{
    extension<T>(T obj)
    {
        [RequiresUnreferencedCode("JSON serialization and deserialization might require types that cannot be statically analyzed. Use the overload that accepts a JsonTypeInfo<T> for NativeAOT compatibility.")]
        [RequiresDynamicCode("JSON serialization and deserialization might require types that cannot be statically analyzed. Use the overload that accepts a JsonTypeInfo<T> for NativeAOT compatibility.")]
        public T DeepCopy() => JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(obj))!;

        public T DeepCopy(JsonTypeInfo<T> jsonTypeInfo) => JsonSerializer.Deserialize(JsonSerializer.Serialize(obj, jsonTypeInfo), jsonTypeInfo)!;
    }
}

