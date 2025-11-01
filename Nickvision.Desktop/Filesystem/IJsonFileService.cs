using System.Threading.Tasks;

namespace Nickvision.Desktop.Filesystem;

/// <summary>
/// An interface of a service for working with json files.
/// </summary>
public interface IJsonFileService : IService
{
    /// <summary>
    /// Loads a json file and deserializes it into an object.
    /// </summary>
    /// <param name="name">The name of the json file (without the .json extension)</param>
    /// <typeparam name="T">The type of the object to deserialize to</typeparam>
    /// <returns>A deserialized object from the json file if successful, else a default constructed object</returns>
    T Load<T>(string? name = null);

    /// <summary>
    /// Loads a json file and deserializes it into an object asynchronously.
    /// </summary>
    /// <param name="name">The name of the json file (without the .json extension)</param>
    /// <typeparam name="T">The type of the object to deserialize to</typeparam>
    /// <returns>A deserialized object from the json file if successful, else a default constructed object</returns>
    Task<T> LoadAsync<T>(string? name = null);

    /// <summary>
    /// Saves an object by serializing it into a json file.
    /// </summary>
    /// <param name="obj">The object to serialize</param>
    /// <param name="name">The name of the json file (without the .json extension)</param>
    /// <typeparam name="T">The type of the object to serialize</typeparam>
    /// <returns>True if the file was saved successfully, else false</returns>
    bool Save<T>(T obj, string? name = null);

    /// <summary>
    /// Saves an object by serializing it into a json file asynchronously.
    /// </summary>
    /// <param name="obj">The object to serialize</param>
    /// <param name="name">The name of the json file (without the .json extension)</param>
    /// <typeparam name="T">The type of the object to serialize</typeparam>
    /// <returns>True if the file was saved successfully, else false</returns>
    Task<bool> SaveAsync<T>(T obj, string? name = null);
}
