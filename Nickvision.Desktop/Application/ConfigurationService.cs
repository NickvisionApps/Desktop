using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;

namespace Nickvision.Desktop.Application;

public class ConfigurationService : IConfigurationService
{
    private static readonly string TableName;

    private readonly ILogger<ConfigurationService> _logger;
    private readonly IDatabaseService _databaseService;
    private readonly Dictionary<string, object> _cache;
    private bool _tableEnsured;

    public event EventHandler<ConfigurationSavedEventArgs>? Saved;

    static ConfigurationService()
    {
        TableName = "configuration";
    }

    public ConfigurationService(ILogger<ConfigurationService> logger, IDatabaseService databaseService)
    {
        _logger = logger;
        _databaseService = databaseService;
        _cache = new Dictionary<string, object>();
        _tableEnsured = false;
    }

    public SqliteTransaction CreateTransation()
    {
        EnsureTable();
        return _databaseService.CreateTransation();
    }

    public async Task<SqliteTransaction> CreateTransationAsync()
    {
        await EnsureTableAsync();
        return await _databaseService.CreateTransationAsync();
    }

    public Dictionary<string, string> GetAllRaw()
    {
        _logger.LogInformation("Getting all raw configuration properties...");
        var dict = new Dictionary<string, string>();
        EnsureTable();
        using var command = _databaseService.SelectAllFromTable(TableName);
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            try
            {
                dict[reader.GetString(0)] = reader.GetString(1);
            }
            catch { }
        }
        _logger.LogInformation($"Found ({dict.Count}) raw configuration properties in database.");
        return dict;
    }

    public async Task<Dictionary<string, string>> GetAllRawAsync()
    {
        _logger.LogInformation("Getting all raw configuration properties...");
        await EnsureTableAsync();
        var dict = new Dictionary<string, string>();
        await using var command = await _databaseService.SelectAllFromTableAsync(TableName);
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            try
            {
                dict[reader.GetString(0)] = reader.GetString(1);
                _logger.LogInformation($"Found raw configuration property ({reader.GetString(0)}) in database with value ({reader.GetString(1)}).");
            }
            catch { }
        }
        _logger.LogInformation($"Found ({dict.Count}) raw configuration properties in database.");
        return dict;
    }

    public T Get<T>(string name, T defaultValue) where T : notnull
    {
        _logger.LogInformation($"Getting configuration property ({name})...");
        if (_cache.TryGetValue(name, out var value) && value is T t)
        {
            _logger.LogInformation($"Value ({value}) found for configuration property ({name}) in cache.");
            return t;
        }
        _cache[name] = defaultValue;
        EnsureTable();
        using var command = _databaseService.SelectFromTable(TableName, "name", name);
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            try
            {
                _cache[name] = typeof(T) switch
                {
                    var type when type == typeof(bool) => bool.Parse(reader.GetString(1)),
                    var type when type == typeof(double) => double.Parse(reader.GetString(1)),
                    var type when type == typeof(int) => int.Parse(reader.GetString(1)),
                    var type when type == typeof(string) => reader.GetString(1),
                    var type when type.IsEnum => Enum.ToObject(typeof(T), int.Parse(reader.GetString(1))),
                    _ => throw new NotSupportedException($"Generic Get<{typeof(T).Name}> is only supported for bool, double, int, string, and enum types. Use Get<T>(..., JsonTypeInfo<T>) for object values.")
                };
            }
            catch { }
        }
        _logger.LogInformation($"Value ({_cache[name]}) found for configuration property ({name}) in database.");
        return (T)_cache[name];
    }

    public T Get<T>(string name, T defaultValue, JsonTypeInfo<T> info) where T : notnull, new()
    {
        _logger.LogInformation($"Getting object configuration property ({name})...");
        if (_cache.TryGetValue(name, out var value) && value is T t)
        {
            _logger.LogInformation($"Value ({value}) found for configuration property ({name}) in cache.");
            return t;
        }
        _cache[name] = defaultValue;
        EnsureTable();
        using var command = _databaseService.SelectFromTable(TableName, "name", name);
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            try
            {
                _cache[name] = JsonSerializer.Deserialize(reader.GetString(1), info)!;
            }
            catch { }
        }
        _logger.LogInformation($"Value ({_cache[name]}) found for configuration property ({name}) in database.");
        return (T)_cache[name];
    }

    public async Task<T> GetAsync<T>(string name, T defaultValue) where T : notnull
    {
        _logger.LogInformation($"Getting configuration property ({name})...");
        if (_cache.TryGetValue(name, out var value) && value is T t)
        {
            _logger.LogInformation($"Value ({value}) found for configuration property ({name}) in cache.");
            return t;
        }
        _cache[name] = defaultValue;
        await EnsureTableAsync();
        await using var command = await _databaseService.SelectFromTableAsync(TableName, "name", name);
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            try
            {
                _cache[name] = typeof(T) switch
                {
                    var type when type == typeof(bool) => bool.Parse(reader.GetString(1)),
                    var type when type == typeof(double) => double.Parse(reader.GetString(1)),
                    var type when type == typeof(int) => int.Parse(reader.GetString(1)),
                    var type when type == typeof(string) => reader.GetString(1),
                    var type when type.IsEnum => Enum.ToObject(typeof(T), int.Parse(reader.GetString(1))),
                    _ => throw new NotSupportedException($"Generic GetAsync<{typeof(T).Name}> is only supported for bool, double, int, string, and enum types. Use GetAsync<T>(..., JsonTypeInfo<T>) for object values.")
                };
            }
            catch { }
        }
        _logger.LogInformation($"Value ({_cache[name]}) found for configuration property ({name}) in database.");
        return (T)_cache[name];
    }

    public async Task<T> GetAsync<T>(string name, T defaultValue, JsonTypeInfo<T> info) where T : notnull, new()
    {
        _logger.LogInformation($"Getting object configuration property ({name})...");
        if (_cache.TryGetValue(name, out var value) && value is T t)
        {
            _logger.LogInformation($"Value ({value}) found for configuration property ({name}) in cache.");
            return t;
        }
        _cache[name] = defaultValue;
        await EnsureTableAsync();
        await using var command = await _databaseService.SelectFromTableAsync(TableName, "name", name);
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            try
            {
                _cache[name] = JsonSerializer.Deserialize(reader.GetString(1), info)!;
            }
            catch { }
        }
        _logger.LogInformation($"Value ({_cache[name]}) found for configuration property ({name}) in database.");
        return (T)_cache[name];
    }

    public async Task<int> ImportFromJsonFileAsync(string path)
    {
        if (!File.Exists(path))
        {
            _logger.LogError($"Failed to import configuration properties from JSON file ({path}) because it does not exist.");
            return 0;
        }
        _logger.LogInformation($"Importing configuration properties from JSON file ({path})...");
        using var json = JsonDocument.Parse(await File.ReadAllTextAsync(path));
        await using var transaction = await CreateTransationAsync();
        var imported = 0;
        foreach (var property in json.RootElement.EnumerateObject())
        {
            if (property.Value.ValueKind == JsonValueKind.String)
            {
                await SetAsync(property.Name, property.Value.GetString()!);
            }
            else
            {
                await SetAsync(property.Name, property.Value.GetRawText());
            }
            _logger.LogInformation($"Found and imported configuration property ({property.Name}) in JSON file ({path}).");
            imported++;
        }
        await transaction.CommitAsync();
        _logger.LogInformation($"Imported {imported} configuration properties from JSON file ({path}).");
        return imported;
    }

    public void Set<T>(string name, T value) where T : notnull
    {
        _cache[name] = value;
        EnsureTable();
        switch (value)
        {
            case bool b:
                _logger.LogInformation($"Setting boolean configuration property ({name}) to value ({b})...");
                _databaseService.ReplaceIntoTable(TableName, new Dictionary<string, object>()
                {
                    { "name", name },
                    { "value", b.ToString() }
                });
                _logger.LogInformation($"Value ({b}) set for configuration property ({name}).");
                Saved?.Invoke(this, new ConfigurationSavedEventArgs(name, b, b.GetType()));
                return;
            case double d:
                _logger.LogInformation($"Setting double configuration property ({name}) to value ({d})...");
                _databaseService.ReplaceIntoTable(TableName, new Dictionary<string, object>()
                {
                    { "name", name },
                    { "value", d.ToString() }
                });
                _logger.LogInformation($"Value ({d}) set for configuration property ({name}).");
                Saved?.Invoke(this, new ConfigurationSavedEventArgs(name, d, d.GetType()));
                return;
            case int i:
                _logger.LogInformation($"Setting integer configuration property ({name}) to value ({i})...");
                _databaseService.ReplaceIntoTable(TableName, new Dictionary<string, object>()
                {
                    { "name", name },
                    { "value", i.ToString() }
                });
                _logger.LogInformation($"Value ({i}) set for configuration property ({name}).");
                Saved?.Invoke(this, new ConfigurationSavedEventArgs(name, i, i.GetType()));
                return;
            case string s:
                _logger.LogInformation($"Setting string configuration property ({name}) to value ({s})...");
                _databaseService.ReplaceIntoTable(TableName, new Dictionary<string, object>()
                {
                    { "name", name },
                    { "value", s }
                });
                _logger.LogInformation($"Value ({s}) set for configuration property ({name}).");
                Saved?.Invoke(this, new ConfigurationSavedEventArgs(name, s, s.GetType()));
                return;
            case T t when typeof(T).IsEnum:
                _logger.LogInformation($"Setting integer configuration property ({name}) to value ({Convert.ToInt32(t)})...");
                _databaseService.ReplaceIntoTable(TableName, new Dictionary<string, object>()
                {
                    { "name", name },
                    { "value", Convert.ToInt32(t).ToString() }
                });
                _logger.LogInformation($"Value ({Convert.ToInt32(t)}) set for configuration property ({name}).");
                Saved?.Invoke(this, new ConfigurationSavedEventArgs(name, t, t.GetType()));
                return;
            default:
                throw new NotSupportedException($"Generic Set<{typeof(T).Name}> is only supported for bool, double, int, string, and enum types. Use Set<T>(..., JsonTypeInfo<T>) for object values.");
        }
    }

    public async Task SetAsync<T>(string name, T value) where T : notnull
    {
        _cache[name] = value;
        await EnsureTableAsync();
        switch (value)
        {
            case bool b:
                _logger.LogInformation($"Setting boolean configuration property ({name}) to value ({b})...");
                await _databaseService.ReplaceIntoTableAsync(TableName, new Dictionary<string, object>()
                {
                    { "name", name },
                    { "value", b.ToString() }
                });
                _logger.LogInformation($"Value ({b}) set for configuration property ({name}).");
                Saved?.Invoke(this, new ConfigurationSavedEventArgs(name, b, b.GetType()));
                return;
            case double d:
                _logger.LogInformation($"Setting double configuration property ({name}) to value ({d})...");
                await _databaseService.ReplaceIntoTableAsync(TableName, new Dictionary<string, object>()
                {
                    { "name", name },
                    { "value", d.ToString() }
                });
                _logger.LogInformation($"Value ({d}) set for configuration property ({name}).");
                Saved?.Invoke(this, new ConfigurationSavedEventArgs(name, d, d.GetType()));
                return;
            case int i:
                _logger.LogInformation($"Setting integer configuration property ({name}) to value ({i})...");
                await _databaseService.ReplaceIntoTableAsync(TableName, new Dictionary<string, object>()
                {
                    { "name", name },
                    { "value", i.ToString() }
                });
                _logger.LogInformation($"Value ({i}) set for configuration property ({name}).");
                Saved?.Invoke(this, new ConfigurationSavedEventArgs(name, i, i.GetType()));
                return;
            case string s:
                _logger.LogInformation($"Setting string configuration property ({name}) to value ({s})...");
                await _databaseService.ReplaceIntoTableAsync(TableName, new Dictionary<string, object>()
                {
                    { "name", name },
                    { "value", s }
                });
                _logger.LogInformation($"Value ({s}) set for configuration property ({name}).");
                Saved?.Invoke(this, new ConfigurationSavedEventArgs(name, s, s.GetType()));
                return;
            case T t when typeof(T).IsEnum:
                _logger.LogInformation($"Setting integer configuration property ({name}) to value ({Convert.ToInt32(t)})...");
                await _databaseService.ReplaceIntoTableAsync(TableName, new Dictionary<string, object>()
                {
                    { "name", name },
                    { "value", Convert.ToInt32(t).ToString() }
                });
                _logger.LogInformation($"Value ({Convert.ToInt32(t)}) set for configuration property ({name}).");
                Saved?.Invoke(this, new ConfigurationSavedEventArgs(name, t, t.GetType()));
                return;
            default:
                throw new NotSupportedException($"Generic SetAsync<{typeof(T).Name}> is only supported for bool, double, int, string, and enum types. Use SetAsync<T>(..., JsonTypeInfo<T>) for object values.");
        }
    }

    public void Set<T>(string name, T value, JsonTypeInfo<T> info) where T : notnull, new()
    {
        _logger.LogInformation($"Setting object configuration property ({name}) to value ({value})...");
        _cache[name] = value;
        EnsureTable();
        _databaseService.ReplaceIntoTable(TableName, new Dictionary<string, object>()
        {
            { "name", name },
            { "value", JsonSerializer.Serialize(value, info) }
        });
        _logger.LogInformation($"Value ({value}) set for configuration property ({name}).");
        Saved?.Invoke(this, new ConfigurationSavedEventArgs(name, value, value.GetType()));
    }

    public async Task SetAsync<T>(string name, T value, JsonTypeInfo<T> info) where T : notnull, new()
    {
        _logger.LogInformation($"Setting object configuration property ({name}) to value ({value})...");
        _cache[name] = value;
        await EnsureTableAsync();
        await _databaseService.ReplaceIntoTableAsync(TableName, new Dictionary<string, object>()
        {
            { "name", name },
            { "value", JsonSerializer.Serialize(value, info) }
        });
        _logger.LogInformation($"Value ({value}) set for configuration property ({name}).");
        Saved?.Invoke(this, new ConfigurationSavedEventArgs(name, value, value.GetType()));
    }

    private void EnsureTable()
    {
        if (_tableEnsured)
        {
            return;
        }
        _databaseService.EnsureTableExists(TableName, "name TEXT PRIMARY KEY, value TEXT");
        _tableEnsured = true;
    }

    private async Task EnsureTableAsync()
    {
        if (_tableEnsured)
        {
            return;
        }
        await _databaseService.EnsureTableExistsAsync(TableName, "name TEXT PRIMARY KEY, value TEXT");
        _tableEnsured = true;
    }
}
