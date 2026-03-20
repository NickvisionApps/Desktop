using Microsoft.Extensions.Logging;
using Nickvision.Desktop.Application;
using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;

namespace Nickvision.Desktop.Filesystem;

/// <summary>
/// A service for working with json files on disk.
/// </summary>
public class JsonFileService : IJsonFileService
{
    private readonly ILogger<JsonFileService> _logger;
    private readonly string _directory;

    /// <summary>
    /// The event for when json files are saved.
    /// </summary>
    public event EventHandler<JsonFileSavedEventArgs>? Saved;

    /// <summary>
    /// Constructs a JsonFileService.
    /// </summary>
    /// <param name="logger">Logger for the service</param>
    /// <param name="appInfo">The AppInfo object for the app</param>
    public JsonFileService(ILogger<JsonFileService> logger, AppInfo appInfo) : this(logger, appInfo.IsPortable ? System.Environment.ExecutingDirectory : Path.Combine(UserDirectories.Config, appInfo.Name))
    {

    }

    /// <summary>
    /// Constructs a JsonFileService.
    /// </summary>
    /// <param name="logger">Logger for the service</param>
    /// <param name="directory">The directory of where to load and save json files from</param>
    /// <remarks>The directory will be created if it doesn't exist</remarks>
    private JsonFileService(ILogger<JsonFileService> logger, string directory)
    {
        _logger = logger;
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
        _directory = directory;
    }

    /// <summary>
    /// Loads a json file and deserializes it into an object.
    /// </summary>
    /// <param name="jsonTypeInfo">The JsonTypeInfo for T, obtained from a source-generated JsonSerializerContext</param>
    /// <param name="name">The name of the json file (without the .json extension)</param>
    /// <typeparam name="T">The type of the object to deserialize to</typeparam>
    /// <returns>A deserialized object from the json file if successful, else a default constructed object</returns>
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

    /// <summary>
    /// Loads a json file and deserializes it into an object asynchronously.
    /// </summary>
    /// <param name="jsonTypeInfo">The JsonTypeInfo for T, obtained from a source-generated JsonSerializerContext</param>
    /// <param name="name">The name of the json file (without the .json extension)</param>
    /// <typeparam name="T">The type of the object to deserialize to</typeparam>
    /// <returns>A deserialized object from the json file if successful, else a default constructed object</returns>
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

    /// <summary>
    /// Saves an object by serializing it into a json file.
    /// </summary>
    /// <param name="obj">The object to serialize</param>
    /// <param name="jsonTypeInfo">The JsonTypeInfo for T, obtained from a source-generated JsonSerializerContext</param>
    /// <param name="name">The name of the json file (without the .json extension)</param>
    /// <typeparam name="T">The type of the object to serialize</typeparam>
    /// <returns>True if the file was saved successfully, else false</returns>
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

    /// <summary>
    /// Saves an object by serializing it into a json file asynchronously.
    /// </summary>
    /// <param name="obj">The object to serialize</param>
    /// <param name="jsonTypeInfo">The JsonTypeInfo for T, obtained from a source-generated JsonSerializerContext</param>
    /// <param name="name">The name of the json file (without the .json extension)</param>
    /// <typeparam name="T">The type of the object to serialize</typeparam>
    /// <returns>True if the file was saved successfully, else false</returns>
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