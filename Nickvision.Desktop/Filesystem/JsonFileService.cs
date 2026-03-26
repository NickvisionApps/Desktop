using Microsoft.Extensions.Logging;
using Nickvision.Desktop.Application;
using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;

namespace Nickvision.Desktop.Filesystem;

public class JsonFileService : IJsonFileService
{
    private readonly ILogger<JsonFileService> _logger;
    private readonly string _directory;

    public event EventHandler<JsonFileSavedEventArgs>? Saved;

    public JsonFileService(ILogger<JsonFileService> logger, AppInfo appInfo) : this(logger, appInfo.IsPortable ? System.Environment.ExecutingDirectory : Path.Combine(UserDirectories.Config, appInfo.Name))
    {

    }

    private JsonFileService(ILogger<JsonFileService> logger, string directory)
    {
        _logger = logger;
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
        _directory = directory;
    }

    public T Load<T>(JsonTypeInfo<T> jsonTypeInfo, string? name = null) where T : new()
    {
        var path = Path.Combine(_directory, $"{(string.IsNullOrEmpty(name) ? typeof(T).Name.ToLower() : name)}.json");
        _logger.LogInformation($"Loading {typeof(T).Name} from {path}...");
        if (!File.Exists(path))
        {
            _logger.LogWarning($"{path} not found, returning default {typeof(T).Name}.");
            return new T();
        }
        try
        {
            var text = File.ReadAllText(path);
            var obj = JsonSerializer.Deserialize(text, jsonTypeInfo);
            _logger.LogInformation($"Loaded {typeof(T).Name} successfully.");
            return obj ?? new T();
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, $"Failed to load {path}, returning default {typeof(T).Name}.");
            return new T();
        }
    }

    public async Task<T> LoadAsync<T>(JsonTypeInfo<T> jsonTypeInfo, string? name = null) where T : new()
    {
        var path = Path.Combine(_directory, $"{(string.IsNullOrEmpty(name) ? typeof(T).Name.ToLower() : name)}.json");
        _logger.LogInformation($"Loading {typeof(T).Name} from {path}...");
        if (!File.Exists(path))
        {
            _logger.LogWarning($"{path} not found, returning default {typeof(T).Name}.");
            return new T();
        }
        try
        {
            var text = await File.ReadAllTextAsync(path);
            var obj = JsonSerializer.Deserialize(text, jsonTypeInfo);
            _logger.LogInformation($"Loaded {typeof(T).Name} successfully.");
            return obj ?? new T();
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, $"Failed to load {path}, returning default {typeof(T).Name}.");
            return new T();
        }
    }

    public bool Save<T>(T obj, JsonTypeInfo<T> jsonTypeInfo, string? name = null)
    {
        if (obj is null)
        {
            return false;
        }
        name = string.IsNullOrEmpty(name) ? typeof(T).Name.ToLower() : name;
        var path = Path.Combine(_directory, $"{name}.json");
        _logger.LogInformation($"Saving {typeof(T).Name} to {path}...");
        try
        {
            var text = JsonSerializer.Serialize(obj, jsonTypeInfo);
            File.WriteAllText(path, text);
            Saved?.Invoke(this, new JsonFileSavedEventArgs(obj, typeof(T), name));
            _logger.LogInformation($"Saved {path} successfully.");
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError($"Failed to save {path}: {e}");
            return false;
        }
    }

    public async Task<bool> SaveAsync<T>(T obj, JsonTypeInfo<T> jsonTypeInfo, string? name = null)
    {
        if (obj is null)
        {
            return false;
        }
        name = string.IsNullOrEmpty(name) ? typeof(T).Name.ToLower() : name;
        var path = Path.Combine(_directory, $"{name}.json");
        _logger.LogInformation($"Saving {typeof(T).Name} to {path}...");
        try
        {
            var text = JsonSerializer.Serialize(obj, jsonTypeInfo);
            await File.WriteAllTextAsync(path, text);
            Saved?.Invoke(this, new JsonFileSavedEventArgs(obj, typeof(T), name));
            _logger.LogInformation($"Saved {path} successfully.");
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError($"Failed to save {path}: {e}");
            return false;
        }
    }
}