using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nickvision.Desktop.Keyring;

public interface IKeyringService
{
    Task<bool> AddCredentialAsync(Credential credential);
    Task<bool> DeleteCredentialAsync(Credential credential);
    Task<IEnumerable<Credential>> GetAllCredentialAsync();
    Task<bool> UpdateCredentialAsync(Credential credential);
}
