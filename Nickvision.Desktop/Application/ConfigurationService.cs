using Microsoft.Extensions.Logging;
using Nickvision.Desktop.Keyring;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;

namespace Nickvision.Desktop.Application;

public class ConfigurationService : IConfigurationService
{
    private static readonly string TableName;

    private readonly ILogger<KeyringService> _logger;
    private readonly IDatabaseService _databaseService;
    private readonly Dictionary<string, object> _cache;
    private bool _tableEnsured;

    public event EventHandler<ConfigurationSavedEventArgs>? Saved;

    static ConfigurationService()
    {
        TableName = "configuration";
    }

    public ConfigurationService(ILogger<KeyringService> logger, IDatabaseService databaseService)
    {
        _logger = logger;
        _databaseService = databaseService;
        _cache = new Dictionary<string, object>();
        _tableEnsured = false;
    }

    public bool GetBool(string name, bool defaultValue = false)
    {
        _logger.LogInformation($"Getting boolean configuration property ({name})...");
        if (_cache.TryGetValue(name, out var value) && value is bool t)
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
                _cache[name] = bool.Parse(reader.GetString(0));
            }
            catch { }
        }
        _logger.LogInformation($"Value ({_cache[name]}) found for configuration property ({name}) in database.");
        return (bool)_cache[name];
    }

    public async Task<bool> GetBoolAsync(string name, bool defaultValue = false)
    {
        _logger.LogInformation($"Getting boolean configuration property ({name})...");
        if (_cache.TryGetValue(name, out var value) && value is bool t)
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
                _cache[name] = bool.Parse(reader.GetString(0));
            }
            catch { }
        }
        _logger.LogInformation($"Value ({_cache[name]}) found for configuration property ({name}) in database.");
        return (bool)_cache[name];
    }

    public double GetDouble(string name, double defaultValue = 0.0)
    {
        _logger.LogInformation($"Getting double configuration property ({name})...");
        if (_cache.TryGetValue(name, out var value) && value is double t)
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
                _cache[name] = double.Parse(reader.GetString(0));
            }
            catch { }
        }
        _logger.LogInformation($"Value ({_cache[name]}) found for configuration property ({name}) in database.");
        return (double)_cache[name];
    }

    public async Task<double> GetDoubleAsync(string name, double defaultValue = 0.0)
    {
        _logger.LogInformation($"Getting double configuration property ({name})...");
        if (_cache.TryGetValue(name, out var value) && value is double t)
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
                _cache[name] = double.Parse(reader.GetString(0));
            }
            catch { }
        }
        _logger.LogInformation($"Value ({_cache[name]}) found for configuration property ({name}) in database.");
        return (double)_cache[name];
    }

    public int GetInt(string name, int defaultValue = 0)
    {
        _logger.LogInformation($"Getting integer configuration property ({name})...");
        if (_cache.TryGetValue(name, out var value) && value is int t)
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
                _cache[name] = int.Parse(reader.GetString(0));
            }
            catch { }
        }
        _logger.LogInformation($"Value ({_cache[name]}) found for configuration property ({name}) in database.");
        return (int)_cache[name];
    }

    public async Task<int> GetIntAsync(string name, int defaultValue = 0)
    {
        _logger.LogInformation($"Getting integer configuration property ({name})...");
        if (_cache.TryGetValue(name, out var value) && value is int t)
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
                _cache[name] = int.Parse(reader.GetString(0));
            }
            catch { }
        }
        _logger.LogInformation($"Value ({_cache[name]}) found for configuration property ({name}) in database.");
        return (int)_cache[name];
    }

    public T GetObject<T>(string name, T defaultValue, JsonTypeInfo<T> info) where T : notnull
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
                _cache[name] = JsonSerializer.Deserialize(reader.GetString(0), info)!;
            }
            catch { }
        }
        _logger.LogInformation($"Value ({_cache[name]}) found for configuration property ({name}) in database.");
        return (T)_cache[name];
    }

    public async Task<T> GetObjectAsync<T>(string name, T defaultValue, JsonTypeInfo<T> info) where T : notnull
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
                _cache[name] = JsonSerializer.Deserialize(reader.GetString(0), info)!;
            }
            catch { }
        }
        _logger.LogInformation($"Value ({_cache[name]}) found for configuration property ({name}) in database.");
        return (T)_cache[name];
    }

    public string GetString(string name, string defaultValue = "")
    {
        _logger.LogInformation($"Getting string configuration property ({name})...");
        if (_cache.TryGetValue(name, out var value) && value is string t)
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
                _cache[name] = reader.GetString(0);
            }
            catch { }
        }
        _logger.LogInformation($"Value ({_cache[name]}) found for configuration property ({name}) in database.");
        return (string)_cache[name];
    }

    public async Task<string> GetStringAsync(string name, string defaultValue = "")
    {
        _logger.LogInformation($"Getting string configuration property ({name})...");
        if (_cache.TryGetValue(name, out var value) && value is string t)
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
                _cache[name] = reader.GetString(0);
            }
            catch { }
        }
        _logger.LogInformation($"Value ({_cache[name]}) found for configuration property ({name}) in database.");
        return (string)_cache[name];
    }

    public void Set(string name, bool value)
    {
        _logger.LogInformation($"Setting boolean configuration property ({name}) to value ({value})...");
        _cache[name] = value;
        EnsureTable();
        _databaseService.ReplaceIntoTable(TableName, new Dictionary<string, object>()
        {
            { "name", name },
            { "value", value.ToString() }
        });
        _logger.LogInformation($"Value ({value}) set for configuration property ({name}).");
        Saved?.Invoke(this, new ConfigurationSavedEventArgs(name, value, value.GetType()));
    }

    public void Set(string name, double value)
    {
        _logger.LogInformation($"Setting double configuration property ({name}) to value ({value})...");
        _cache[name] = value;
        EnsureTable();
        _databaseService.ReplaceIntoTable(TableName, new Dictionary<string, object>()
        {
            { "name", name },
            { "value", value.ToString() }
        });
        _logger.LogInformation($"Value ({value}) set for configuration property ({name}).");
        Saved?.Invoke(this, new ConfigurationSavedEventArgs(name, value, value.GetType()));
    }

    public void Set(string name, int value)
    {
        _logger.LogInformation($"Setting integer configuration property ({name}) to value ({value})...");
        _cache[name] = value;
        EnsureTable();
        _databaseService.ReplaceIntoTable(TableName, new Dictionary<string, object>()
        {
            { "name", name },
            { "value", value.ToString() }
        });
        _logger.LogInformation($"Value ({value}) set for configuration property ({name}).");
        Saved?.Invoke(this, new ConfigurationSavedEventArgs(name, value, value.GetType()));
    }

    public void Set(string name, string value)
    {
        _logger.LogInformation($"Setting string configuration property ({name}) to value ({value})...");
        _cache[name] = value;
        EnsureTable();
        _databaseService.ReplaceIntoTable(TableName, new Dictionary<string, object>()
        {
            { "name", name },
            { "value", value }
        });
        _logger.LogInformation($"Value ({value}) set for configuration property ({name}).");
        Saved?.Invoke(this, new ConfigurationSavedEventArgs(name, value, value.GetType()));
    }

    public void Set<T>(string name, T value, JsonTypeInfo<T> info) where T : notnull
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

    public async Task SetAsync(string name, bool value)
    {
        _logger.LogInformation($"Setting boolean configuration property ({name}) to value ({value})...");
        _cache[name] = value;
        await EnsureTableAsync();
        await _databaseService.ReplaceIntoTableAsync(TableName, new Dictionary<string, object>()
        {
            { "name", name },
            { "value", value.ToString() }
        });
        _logger.LogInformation($"Value ({value}) set for configuration property ({name}).");
        Saved?.Invoke(this, new ConfigurationSavedEventArgs(name, value, value.GetType()));
    }

    public async Task SetAsync(string name, double value)
    {
        _logger.LogInformation($"Setting double configuration property ({name}) to value ({value})...");
        _cache[name] = value;
        await EnsureTableAsync();
        await _databaseService.ReplaceIntoTableAsync(TableName, new Dictionary<string, object>()
        {
            { "name", name },
            { "value", value.ToString() }
        });
        _logger.LogInformation($"Value ({value}) set for configuration property ({name}).");
        Saved?.Invoke(this, new ConfigurationSavedEventArgs(name, value, value.GetType()));
    }

    public async Task SetAsync(string name, int value)
    {
        _logger.LogInformation($"Setting integer configuration property ({name}) to value ({value})...");
        _cache[name] = value;
        await EnsureTableAsync();
        await _databaseService.ReplaceIntoTableAsync(TableName, new Dictionary<string, object>()
        {
            { "name", name },
            { "value", value.ToString() }
        });
        _logger.LogInformation($"Value ({value}) set for configuration property ({name}).");
        Saved?.Invoke(this, new ConfigurationSavedEventArgs(name, value, value.GetType()));
    }

    public async Task SetAsync(string name, string value)
    {
        _logger.LogInformation($"Setting string configuration property ({name}) to value ({value})...");
        _cache[name] = value;
        await EnsureTableAsync();
        await _databaseService.ReplaceIntoTableAsync(TableName, new Dictionary<string, object>()
        {
            { "name", name },
            { "value", value }
        });
        _logger.LogInformation($"Value ({value}) set for configuration property ({name}).");
        Saved?.Invoke(this, new ConfigurationSavedEventArgs(name, value, value.GetType()));
    }

    public async void SetAsync<T>(string name, T value, JsonTypeInfo<T> info) where T : notnull
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
        await _databaseService.EnsureTableExistsAsync(TableName, "name TEXT, value TEXT");
        _tableEnsured = true;
    }
}
