using System.Threading.Tasks;

namespace Nickvision.Desktop.System;

/// <summary>
/// An interface of a service for managing secrets.
/// </summary>
public interface ISecretService : IService
{
    /// <summary>
    /// Adds a secret.
    /// </summary>
    /// <param name="secret">The secret to add</param>
    /// <returns>True if the secret was added successfully, else false</returns>
    bool Add(Secret secret);

    /// <summary>
    /// Adds a secret asynchronously.
    /// </summary>
    /// <param name="secret">The secret to add</param>
    /// <returns>True if the secret was added successfully, else false</returns>
    Task<bool> AddAsync(Secret secret);

    /// <summary>
    /// Creates a secret with a random but secure value.
    /// </summary>
    /// <param name="name">The name of the secret to create</param>
    /// <returns>The created secret if successful, else null</returns>
    Secret? Create(string name);

    /// <summary>
    /// Creates a secret asynchronously with a random but secure value.
    /// </summary>
    /// <param name="name">The name of the secret to create</param>
    /// <returns>The created secret if successful, else null</returns>
    Task<Secret?> CreateAsync(string name);

    /// <summary>
    /// Deletes a secret.
    /// </summary>
    /// <param name="name">The name of the secret to delete</param>
    /// <returns>True if the secret was deleted successfully, else false</returns>
    bool Delete(string name);

    /// <summary>
    /// Deletes a secret asynchronously.
    /// </summary>
    /// <param name="name">The name of the secret to delete</param>
    /// <returns>True if the secret was deleted successfully, else false</returns>
    Task<bool> DeleteAsync(string name);

    /// <summary>
    /// Gets a secret.
    /// </summary>
    /// <param name="name">The name of the secret to find</param>
    /// <returns>The secret if found, else null</returns>
    Secret? Get(string name);

    /// <summary>
    /// Gets a secret asynchronously.
    /// </summary>
    /// <param name="name">The name of the secret to find</param>
    /// <returns>The secret if found, else null</returns>
    Task<Secret?> GetAsync(string name);

    /// <summary>
    /// Updates a secret.
    /// </summary>
    /// <param name="secret">The secret to update</param>
    /// <returns>True if the secret was updated successfully, else false</returns>
    bool Update(Secret secret);

    /// <summary>
    /// Updates a secret asynchronously.
    /// </summary>
    /// <param name="secret">The secret to update</param>
    /// <returns>True if the secret was updated successfully, else false</returns>
    Task<bool> UpdateAsync(Secret secret);
}
