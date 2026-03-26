using System.Threading.Tasks;

namespace Nickvision.Desktop.System;

public interface ISecretService
{
    Task<bool> AddAsync(Secret secret);
    Task<Secret?> CreateAsync(string name);
    Task<bool> DeleteAsync(string name);
    Task<Secret?> GetAsync(string name);
    Task<bool> UpdateAsync(Secret secret);
}
