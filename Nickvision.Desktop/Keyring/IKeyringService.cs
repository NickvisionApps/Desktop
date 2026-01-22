using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nickvision.Desktop.Keyring;

/// <summary>
/// An interface of a service for managing credentials in a keyring.
/// </summary>
public interface IKeyringService : IService
{
    /// <summary>
    /// The list of credentials in the keyring.
    /// </summary>
    IEnumerable<Credential> Credentials { get; }
    /// <summary>
    /// Whether the keyring is currently saving to disk.
    /// </summary>
    bool IsSavingToDisk { get; }

    /// <summary>
    /// Adds a credential to the keyring.
    /// </summary>
    /// <param name="credential">The credential to add</param>
    /// <returns>True if the keyring was successfully added, else false</returns>
    Task<bool> AddCredentialAsync(Credential credential);

    /// <summary>
    /// Destroys the keyring and all its credentials.
    /// </summary>
    /// <returns>True if the keyring was successfully added, else false</returns>
    Task<bool> DestroyAsync();

    /// <summary>
    /// Removes a credential from the keyring.
    /// </summary>
    /// <param name="credential">The credential to remove</param>
    /// <returns>True if the keyring was successfully removed, else false</returns>
    Task<bool> RemoveCredentialAsync(Credential credential);

    /// <summary>
    /// Updates a credential in the keyring.
    /// </summary>
    /// <param name="credential">The credential to update</param>
    /// <returns>True if the keyring was successfully updated, else false</returns>
    Task<bool> UpdateCredentialAsync(Credential credential);
}
