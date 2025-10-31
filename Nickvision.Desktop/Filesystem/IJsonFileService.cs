using System.Threading.Tasks;

namespace Nickvision.Desktop.Filesystem;

public interface IJsonFileService : IService
{
    T Load<T>(string? name = null);

    Task<T> LoadAsync<T>(string? name = null);

    bool Save<T>(T obj, string? name = null);

    Task<bool> SaveAsync<T>(T obj, string? name = null);
}
