using Nickvision.Desktop.Application;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace Nickvision.Desktop.Filesystem;

/// <summary>
///     A service for working with json files on disk.
/// </summary>
public class JsonFileService : IJsonFileService
{
    private static readonly JsonSerializerOptions JsonOptions;
    private readonly string _directory;

    /// <summary>
    ///     Constructs a static JsonFileService.
    /// </summary>
    static JsonFileService()
    {
        JsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true
        };
    }

    /// <summary>
    ///     Constructs a JsonFileService.
    /// </summary>
    /// <param name="directory">The directory of where to load and save json files from</param>
    /// <remarks>The directory will be created if it doesn't exist</remarks>
    public JsonFileService(string directory)
    {
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
        _directory = directory;
    }

    /// <summary>
    ///     Constructs a JsonFileService.
    /// </summary>
    /// <param name="appInfo">The AppInfo object for the app</param>
    /// <remarks>
    ///     If the environment variable "{AppInfo.Id}.portable" is set, the current working directory will be used. Else,
    ///     a {AppInfo.Name} folder will be created and used inside the user's configuration directory.
    /// </remarks>
    public JsonFileService(AppInfo appInfo) : this(Environment.GetEnvironmentVariable($"{appInfo.Id}.portable") is not null ? Directory.GetCurrentDirectory() : Path.Combine(UserDirectories.Config, appInfo.Name))
    {
    }

    /// <summary>
    ///     The event for when json files are saved.
    /// </summary>
    public event EventHandler<JsonFileSavedEventArgs>? Saved;

    /// <summary>
    ///     Loads a json file and deserializes it into an object.
    /// </summary>
    /// <param name="name">The name of the json file (without the .json extension)</param>
    /// <typeparam name="T">The type of the object to deserialize to</typeparam>
    /// <returns>A deserialized object from the json file if successful, else a default constructed object</returns>
    public T Load<T>(string? name = null)
    {
        var path = Path.Combine(_directory, $"{(string.IsNullOrEmpty(name) ? typeof(T).Name.ToLower() : name)}.json");
        if (!File.Exists(path))
        {
            return Activator.CreateInstance<T>();
        }
        var text = File.ReadAllText(path);
        var obj = JsonSerializer.Deserialize<T>(text);
        return obj ?? Activator.CreateInstance<T>();
    }

    /// <summary>
    ///     Loads a json file and deserializes it into an object asynchronously.
    /// </summary>
    /// <param name="name">The name of the json file (without the .json extension)</param>
    /// <typeparam name="T">The type of the object to deserialize to</typeparam>
    /// <returns>A deserialized object from the json file if successful, else a default constructed object</returns>
    public async Task<T> LoadAsync<T>(string? name = null)
    {
        var path = Path.Combine(_directory, $"{(string.IsNullOrEmpty(name) ? typeof(T).Name.ToLower() : name)}.json");
        if (!File.Exists(path))
        {
            return Activator.CreateInstance<T>();
        }
        var text = await File.ReadAllTextAsync(path);
        var obj = JsonSerializer.Deserialize<T>(text);
        return obj ?? Activator.CreateInstance<T>();
    }

    /// <summary>
    ///     Saves an object by serializing it into a json file.
    /// </summary>
    /// <param name="obj">The object to serialize</param>
    /// <param name="name">The name of the json file (without the .json extension)</param>
    /// <typeparam name="T">The type of the object to serialize</typeparam>
    /// <returns>True if the file was saved successfully, else false</returns>
    public bool Save<T>(T obj, string? name = null)
    {
        if (obj is null)
        {
            return false;
        }
        name = string.IsNullOrEmpty(name) ? typeof(T).Name.ToLower() : name;
        try
        {
            var text = JsonSerializer.Serialize(obj, JsonOptions);
            File.WriteAllText(Path.Combine(_directory, $"{name}.json"), text);
            Saved?.Invoke(this, new JsonFileSavedEventArgs(obj, typeof(T), name));
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    ///     Saves an object by serializing it into a json file asynchronously.
    /// </summary>
    /// <param name="obj">The object to serialize</param>
    /// <param name="name">The name of the json file (without the .json extension)</param>
    /// <typeparam name="T">The type of the object to serialize</typeparam>
    /// <returns>True if the file was saved successfully, else false</returns>
    public async Task<bool> SaveAsync<T>(T obj, string? name = null)
    {
        if (obj is null)
        {
            return false;
        }
        name = string.IsNullOrEmpty(name) ? typeof(T).Name.ToLower() : name;
        try
        {
            var text = JsonSerializer.Serialize(obj, JsonOptions);
            await File.WriteAllTextAsync(Path.Combine(_directory, $"{name}.json"), text);
            Saved?.Invoke(this, new JsonFileSavedEventArgs(obj, typeof(T), name));
            return true;
        }
        catch
        {
            return false;
        }
    }
}
