using Microsoft.Extensions.Logging;
using Nickvision.Desktop.Keyring;
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
        if (_cache.TryGetValue(name, out var value) && value is bool t)
        {
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
        return (bool)_cache[name];
    }

    public async Task<bool> GetBoolAsync(string name, bool defaultValue = false)
    {
        if (_cache.TryGetValue(name, out var value) && value is bool t)
        {
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
        return (bool)_cache[name];
    }

    public double GetDouble(string name, double defaultValue = 0.0)
    {
        if (_cache.TryGetValue(name, out var value) && value is double t)
        {
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
        return (double)_cache[name];
    }

    public async Task<double> GetDoubleAsync(string name, double defaultValue = 0.0)
    {
        if (_cache.TryGetValue(name, out var value) && value is double t)
        {
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
        return (double)_cache[name];
    }

    public int GetInt(string name, int defaultValue = 0)
    {
        if (_cache.TryGetValue(name, out var value) && value is int t)
        {
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
        return (int)_cache[name];
    }

    public async Task<int> GetIntAsync(string name, int defaultValue = 0)
    {
        if (_cache.TryGetValue(name, out var value) && value is int t)
        {
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
        return (int)_cache[name];
    }

    public T GetObject<T>(string name, T defaultValue, JsonTypeInfo<T> info) where T : notnull
    {
        if (_cache.TryGetValue(name, out var value) && value is T t)
        {
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
        return (T)_cache[name];
    }

    public async Task<T> GetObjectAsync<T>(string name, T defaultValue, JsonTypeInfo<T> info) where T : notnull
    {
        if (_cache.TryGetValue(name, out var value) && value is T t)
        {
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
        return (T)_cache[name];
    }

    public string GetString(string name, string defaultValue = "")
    {
        if (_cache.TryGetValue(name, out var value) && value is string t)
        {
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
        return (string)_cache[name];
    }

    public async Task<string> GetStringAsync(string name, string defaultValue = "")
    {
        if (_cache.TryGetValue(name, out var value) && value is string t)
        {
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
        return (string)_cache[name];
    }

    public void Set(string name, bool value)
    {
        _cache[name] = value;
        EnsureTable();
        _databaseService.ReplaceIntoTable(TableName, new Dictionary<string, object>()
        {
            { "name", name },
            { "value", value.ToString() }
        });
    }

    public void Set(string name, double value)
    {
        _cache[name] = value;
        EnsureTable();
        _databaseService.ReplaceIntoTable(TableName, new Dictionary<string, object>()
        {
            { "name", name },
            { "value", value.ToString() }
        });
    }

    public void Set(string name, int value)
    {
        _cache[name] = value;
        EnsureTable();
        _databaseService.ReplaceIntoTable(TableName, new Dictionary<string, object>()
        {
            { "name", name },
            { "value", value.ToString() }
        });
    }

    public void Set(string name, string value)
    {
        _cache[name] = value;
        EnsureTable();
        _databaseService.ReplaceIntoTable(TableName, new Dictionary<string, object>()
        {
            { "name", name },
            { "value", value }
        });
    }

    public void Set<T>(string name, T value, JsonTypeInfo<T> info) where T : notnull
    {
        _cache[name] = value;
        EnsureTable();
        _databaseService.ReplaceIntoTable(TableName, new Dictionary<string, object>()
        {
            { "name", name },
            { "value", JsonSerializer.Serialize(value, info) }
        });
    }

    public async Task SetAsync(string name, bool value)
    {
        _cache[name] = value;
        await EnsureTableAsync();
        await _databaseService.ReplaceIntoTableAsync(TableName, new Dictionary<string, object>()
        {
            { "name", name },
            { "value", value.ToString() }
        });
    }

    public async Task SetAsync(string name, double value)
    {
        _cache[name] = value;
        await EnsureTableAsync();
        await _databaseService.ReplaceIntoTableAsync(TableName, new Dictionary<string, object>()
        {
            { "name", name },
            { "value", value.ToString() }
        });
    }

    public async Task SetAsync(string name, int value)
    {
        _cache[name] = value;
        await EnsureTableAsync();
        await _databaseService.ReplaceIntoTableAsync(TableName, new Dictionary<string, object>()
        {
            { "name", name },
            { "value", value.ToString() }
        });
    }

    public async Task SetAsync(string name, string value)
    {
        _cache[name] = value;
        await EnsureTableAsync();
        await _databaseService.ReplaceIntoTableAsync(TableName, new Dictionary<string, object>()
        {
            { "name", name },
            { "value", value }
        });
    }

    public async void SetAsync<T>(string name, T value, JsonTypeInfo<T> info) where T : notnull
    {
        _cache[name] = value;
        await EnsureTableAsync();
        await _databaseService.ReplaceIntoTableAsync(TableName, new Dictionary<string, object>()
        {
            { "name", name },
            { "value", JsonSerializer.Serialize(value, info) }
        });
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
