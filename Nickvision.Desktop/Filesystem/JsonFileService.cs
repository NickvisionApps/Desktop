using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace Nickvision.Desktop.Filesystem;

public class JsonFileService : IJsonFileService
{
    private static readonly JsonSerializerOptions JsonOptions;
    private readonly string _directory;

    static JsonFileService()
    {
        JsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true
        };
    }

    public JsonFileService(string directory)
    {
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
        _directory = directory;
    }

    public T Load<T>(string? name = null)
    {
        var path = Path.Combine(_directory, $"{(string.IsNullOrEmpty(name) ? typeof(T).Name : name)}.json");
        if (!File.Exists(path))
        {
            return Activator.CreateInstance<T>();
        }
        var text = File.ReadAllText(path);
        var obj = JsonSerializer.Deserialize<T>(text);
        return obj ?? Activator.CreateInstance<T>();
    }

    public async Task<T> LoadAsync<T>(string? name = null)
    {
        var path = Path.Combine(_directory, $"{(string.IsNullOrEmpty(name) ? typeof(T).Name : name)}.json");
        if (!File.Exists(path))
        {
            return Activator.CreateInstance<T>();
        }
        var text = await File.ReadAllTextAsync(path);
        var obj = JsonSerializer.Deserialize<T>(text);
        return obj ?? Activator.CreateInstance<T>();
    }

    public bool Save<T>(T obj, string? name = null)
    {
        try
        {
            var text = JsonSerializer.Serialize(obj, JsonOptions);
            File.WriteAllText(Path.Combine(_directory, $"{(string.IsNullOrEmpty(name) ? typeof(T).Name : name)}.json"),
                text);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> SaveAsync<T>(T obj, string? name = null)
    {
        try
        {
            var text = JsonSerializer.Serialize(obj, JsonOptions);
            await File.WriteAllTextAsync(
                Path.Combine(_directory, $"{(string.IsNullOrEmpty(name) ? typeof(T).Name : name)}.json"), text);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
