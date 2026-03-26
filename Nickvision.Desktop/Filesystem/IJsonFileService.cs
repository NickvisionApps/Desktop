using System;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;

namespace Nickvision.Desktop.Filesystem;

public interface IJsonFileService
{
    event EventHandler<JsonFileSavedEventArgs>? Saved;

    T Load<T>(JsonTypeInfo<T> jsonTypeInfo, string? name = null) where T : new();
    Task<T> LoadAsync<T>(JsonTypeInfo<T> jsonTypeInfo, string? name = null) where T : new();
    bool Save<T>(T obj, JsonTypeInfo<T> jsonTypeInfo, string? name = null);
    Task<bool> SaveAsync<T>(T obj, JsonTypeInfo<T> jsonTypeInfo, string? name = null);
}
