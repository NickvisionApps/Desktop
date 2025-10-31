using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nickvision.Desktop.Keyring;

public interface IKeyringService : IService
{
    bool IsSavingToDisk { get; }
    IEnumerable<Credential> Credentials { get; }

    Task<bool> AddCredentialAsync(Credential credential);

    Task<bool> DestroyAsync();

    Task<bool> RemoveCredentialAsync(Credential credential);

    Task<bool> UpdateCredentialAsync(Credential credential);
}
