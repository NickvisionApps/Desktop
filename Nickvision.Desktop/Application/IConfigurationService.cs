using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;

namespace Nickvision.Desktop.Application;

public interface IConfigurationService
{
    bool GetBool(string name, bool defaultValue = false);
    Task<bool> GetBoolAsync(string name, bool defaultValue = false);
    double GetDouble(string name, double defaultValue = 0.0);
    Task<double> GetDoubleAsync(string name, double defaultValue = 0.0);
    int GetInt(string name, int defaultValue = 0);
    Task<int> GetIntAsync(string name, int defaultValue = 0);
    T GetObject<T>(string name, T defaultValue, JsonTypeInfo<T> info) where T : notnull;
    Task<T> GetObjectAsync<T>(string name, T defaultValue, JsonTypeInfo<T> info) where T : notnull;
    string GetString(string name, string defaultValue = "");
    Task<string> GetStringAsync(string name, string defaultValue = "");
    void Set(string name, bool value);
    void Set(string name, double value);
    void Set(string name, int value);
    void Set(string name, string value);
    void Set<T>(string name, T value, JsonTypeInfo<T> info) where T : notnull;
    Task SetAsync(string name, bool value);
    Task SetAsync(string name, double value);
    Task SetAsync(string name, int value);
    Task SetAsync(string name, string value);
    void SetAsync<T>(string name, T value, JsonTypeInfo<T> info) where T : notnull;
}
