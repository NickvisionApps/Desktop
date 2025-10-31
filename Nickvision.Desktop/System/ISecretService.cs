using System.Threading.Tasks;

namespace Nickvision.Desktop.System;

public interface ISecretService
{
    bool Add(Secret secret);

    Task<bool> AddAsync(Secret secret);

    Secret? Create(string name);

    Task<Secret?> CreateAsync(string name);

    bool Delete(string name);

    Task<bool> DeleteAsync(string name);

    Secret? Get(string name);

    Task<Secret?> GetAsync(string name);

    bool Update(Secret secret);

    Task<bool> UpdateAsync(Secret secret);
}
