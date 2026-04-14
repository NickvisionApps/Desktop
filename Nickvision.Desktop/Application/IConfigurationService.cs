using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;

namespace Nickvision.Desktop.Application;

public interface IConfigurationService
{
    event EventHandler<ConfigurationSavedEventArgs>? Saved;

    SqliteTransaction CreateTransation();
    Task<SqliteTransaction> CreateTransationAsync();
    Dictionary<string, string> GetAllRaw();
    Task<Dictionary<string, string>> GetAllRawAsync();
    T Get<T>(string name, T defaultValue) where T : notnull;
    T Get<T>(string name, T defaultValue, JsonTypeInfo<T> info) where T : notnull, new();
    Task<T> GetAsync<T>(string name, T defaultValue) where T : notnull;
    Task<T> GetAsync<T>(string name, T defaultValue, JsonTypeInfo<T> info) where T : notnull, new();
    Task<int> ImportFromJsonFileAsync(string path);
    void Set<T>(string name, T value) where T : notnull;
    void Set<T>(string name, T value, JsonTypeInfo<T> info) where T : notnull, new();
    Task SetAsync<T>(string name, T value) where T : notnull;
    Task SetAsync<T>(string name, T value, JsonTypeInfo<T> info) where T : notnull, new();
}
