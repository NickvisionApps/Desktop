using Microsoft.Extensions.Logging;
using Nickvision.Desktop.Keyring;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Vanara.InteropServices;
using Vanara.PInvoke;

namespace Nickvision.Desktop.System;

/// <summary>
/// A service for managing secrets using the system's secret storage.
/// </summary>
public class SecretService : ISecretService
{
    private readonly ILogger<SecretService> _logger;

    /// <summary>
    /// Constructs a SecretService.
    /// </summary>
    /// <param name="logger">Logger for the service</param>
    public SecretService(ILogger<SecretService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Adds a secret.
    /// </summary>
    /// <param name="secret">The secret to add</param>
    /// <returns>True if the secret was added successfully, else false</returns>
    public bool Add(Secret secret)
    {
        _logger.LogInformation($"Adding system secret ({secret.Name}).");
        if (secret.Empty)
        {
            _logger.LogError($"Unable to add system secret ({secret.Name}) as it is empty.");
            return false;
        }
        if (Get(secret.Name) is not null)
        {
            _logger.LogError($"Unable to add system secret ({secret.Name}) as it already exists.");
            return false;
        }
        if (OperatingSystem.IsWindows())
        {
            var stringPtr = Marshal.StringToHGlobalUni(secret.Value);
            var res = AdvApi32.CredWrite(new AdvApi32.CREDENTIAL
            {
                AttributeCount = 0,
                Attributes = nint.Zero,
                Type = AdvApi32.CRED_TYPE.CRED_TYPE_GENERIC,
                Persist = AdvApi32.CRED_PERSIST.CRED_PERSIST_LOCAL_MACHINE,
                TargetName = new StrPtrAuto(secret.Name),
                UserName = new StrPtrAuto("default"),
                CredentialBlobSize = (uint)Encoding.Unicode.GetByteCount(secret.Value),
                CredentialBlob = stringPtr
            }, 0);
            Marshal.FreeHGlobal(stringPtr);
            if (res)
            {
                _logger.LogInformation($"Added system secret ({secret.Name}) successfully.");
            }
            else
            {
                _logger.LogError($"Failed to add system secret ({secret.Name}): {Win32Error.GetLastError().GetException()}");
            }
            return res;
        }
        else if (OperatingSystem.IsMacOS())
        {
            using var process = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = "security",
                    Arguments = $"add-generic-password -a default -s {secret.Name} -w \"{secret.Value}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            process.Start();
            process.WaitForExit();
            if (process.ExitCode == 0)
            {
                _logger.LogInformation($"Added system secret ({secret.Name}) successfully.");
            }
            else
            {
                _logger.LogError($"Failed to add system secret ({secret.Name}): {process.StandardOutput.ReadToEnd()}\n{process.StandardError.ReadToEnd()}");
            }
            return process.ExitCode == 0;
        }
        else if (OperatingSystem.IsLinux())
        {
            using var process = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = "secret-tool",
                    Arguments = $"store --label={secret.Name} schema Nickvision.Desktop application {secret.Name}",
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            process.Start();
            var stdin = process.StandardInput;
            stdin.Write(secret.Value);
            stdin.Close();
            process.WaitForExit();
            if (process.ExitCode == 0)
            {
                _logger.LogInformation($"Added system secret ({secret.Name}) successfully.");
            }
            else
            {
                _logger.LogError($"Failed to add system secret ({secret.Name}): {process.StandardOutput.ReadToEnd()}\n{process.StandardError.ReadToEnd()}");
            }
            return process.ExitCode == 0;
        }
        else
        {
            _logger.LogError($"Unable to add system secret. The OS is unsupported.");
            return false;
        }
    }

    /// <summary>
    /// Adds a secret asynchronously.
    /// </summary>
    /// <param name="secret">The secret to add</param>
    /// <returns>True if the secret was added successfully, else false</returns>
    public async Task<bool> AddAsync(Secret secret)
    {
        _logger.LogInformation($"Adding secret {secret.Name}...");
        if (secret.Empty)
        {
            _logger.LogError($"Unable to add secret {secret.Name} as it is empty.");
            return false;
        }
        if (await GetAsync(secret.Name) is not null)
        {
            _logger.LogError($"Secret {secret.Name} already exists.");
            return false;
        }
        if (OperatingSystem.IsWindows())
        {
            var stringPtr = Marshal.StringToHGlobalUni(secret.Value);
            var res = await Task.Run(() => AdvApi32.CredWrite(new AdvApi32.CREDENTIAL
            {
                AttributeCount = 0,
                Attributes = nint.Zero,
                Type = AdvApi32.CRED_TYPE.CRED_TYPE_GENERIC,
                Persist = AdvApi32.CRED_PERSIST.CRED_PERSIST_LOCAL_MACHINE,
                TargetName = new StrPtrAuto(secret.Name),
                UserName = new StrPtrAuto("default"),
                CredentialBlobSize = (uint)Encoding.Unicode.GetByteCount(secret.Value),
                CredentialBlob = stringPtr
            },
                0));
            Marshal.FreeHGlobal(stringPtr);
            if (res)
            {
                _logger.LogInformation($"Added secret {secret.Name} successfully.");
            }
            else
            {
                _logger.LogError($"Failed to add secret {secret.Name} on Windows.");
            }
            return res;
        }
        else if (OperatingSystem.IsMacOS())
        {
            using var process = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = "security",
                    Arguments = $"add-generic-password -a default -s {secret.Name} -w \"{secret.Value}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            process.Start();
            await process.WaitForExitAsync();
            if (process.ExitCode == 0)
            {
                _logger.LogInformation($"Added secret {secret.Name} successfully.");
            }
            else
            {
                _logger.LogError($"Failed to add secret {secret.Name} on macOS.");
            }
            return process.ExitCode == 0;
        }
        else if (OperatingSystem.IsLinux())
        {
            using var process = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = "secret-tool",
                    Arguments = $"store --label={secret.Name} schema Nickvision.Desktop application {secret.Name}",
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            process.Start();
            var stdin = process.StandardInput;
            await stdin.WriteAsync(secret.Value);
            stdin.Close();
            await process.WaitForExitAsync();
            if (process.ExitCode == 0)
            {
                _logger.LogInformation($"Added secret {secret.Name} successfully.");
            }
            else
            {
                _logger.LogError($"Failed to add secret {secret.Name} on Linux.");
            }
            return process.ExitCode == 0;
        }
        else
        {
            _logger.LogError($"Unable to add secret {secret.Name} as the operating system is not supported.");
            return false;
        }
    }

    /// <summary>
    /// Creates a secret with a random but secure value.
    /// </summary>
    /// <param name="name">The name of the secret to create</param>
    /// <returns>The created secret if successful, else null</returns>
    public Secret? Create(string name)
    {
        _logger.LogInformation($"Creating secret {name}...");
        if (string.IsNullOrEmpty(name))
        {
            _logger.LogWarning("Unable to create secret as the name is null or empty.");
            return null;
        }
        if (Get(name) is not null)
        {
            _logger.LogWarning($"Secret {name} already exists.");
            return null;
        }
        var secret = new Secret(name, new PasswordGenerator().Next(64));
        var result = Add(secret) ? secret : null;
        if (result is null)
        {
            _logger.LogError($"Failed to create secret {name}.");
        }
        else
        {
            _logger.LogInformation($"Created secret {name} successfully.");
        }
        return result;
    }

    /// <summary>
    /// Creates a secret asynchronously with a random but secure value.
    /// </summary>
    /// <param name="name">The name of the secret to create</param>
    /// <returns>The created secret if successful, else null</returns>
    public async Task<Secret?> CreateAsync(string name)
    {
        _logger.LogInformation($"Creating secret {name}...");
        if (string.IsNullOrEmpty(name))
        {
            _logger.LogWarning("Unable to create secret as the name is null or empty.");
            return null;
        }
        if (await GetAsync(name) is not null)
        {
            _logger.LogWarning($"Secret {name} already exists.");
            return null;
        }
        var secret = new Secret(name, new PasswordGenerator().Next(64));
        var result = await AddAsync(secret) ? secret : null;
        if (result is null)
        {
            _logger.LogError($"Failed to create secret {name}.");
        }
        else
        {
            _logger.LogInformation($"Created secret {name} successfully.");
        }
        return result;
    }

    /// <summary>
    /// Deletes a secret.
    /// </summary>
    /// <param name="name">The name of the secret to delete</param>
    /// <returns>True if the secret was deleted successfully, else false</returns>
    public bool Delete(string name)
    {
        _logger.LogInformation($"Deleting secret {name}...");
        if (string.IsNullOrEmpty(name))
        {
            _logger.LogError("Unable to delete secret as the name is null or empty.");
            return false;
        }
        if (OperatingSystem.IsWindows())
        {
            var res = AdvApi32.CredDelete(name, AdvApi32.CRED_TYPE.CRED_TYPE_GENERIC);
            if (res)
            {
                _logger.LogInformation($"Deleted secret {name} successfully.");
            }
            else
            {
                _logger.LogError($"Failed to delete secret {name} on Windows.");
            }
            return res;
        }
        else if (OperatingSystem.IsMacOS())
        {
            using var process = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = "security",
                    Arguments = $"delete-generic-password -a default -s {name}",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            process.Start();
            process.WaitForExit();
            if (process.ExitCode == 0)
            {
                _logger.LogInformation($"Deleted secret {name} successfully.");
            }
            else
            {
                _logger.LogError($"Failed to delete secret {name} on macOS.");
            }
            return process.ExitCode == 0;
        }
        else if (OperatingSystem.IsLinux())
        {
            using var process = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = "secret-tool",
                    Arguments = $"clear schema Nickvision.Desktop application {name}",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            process.Start();
            process.WaitForExit();
            if (process.ExitCode == 0)
            {
                _logger.LogInformation($"Deleted secret {name} successfully.");
            }
            else
            {
                _logger.LogError($"Failed to delete secret {name} on Linux.");
            }
            return process.ExitCode == 0;
        }
        else
        {
            _logger.LogError($"Unable to delete secret {name} as the operating system is not supported.");
            return false;
        }
    }

    /// <summary>
    /// Deletes a secret asynchronously.
    /// </summary>
    /// <param name="name">The name of the secret to delete</param>
    /// <returns>True if the secret was deleted successfully, else false</returns>
    public async Task<bool> DeleteAsync(string name)
    {
        _logger.LogInformation($"Deleting secret {name}...");
        if (string.IsNullOrEmpty(name))
        {
            _logger.LogError("Unable to delete secret as the name is null or empty.");
            return false;
        }
        if (OperatingSystem.IsWindows())
        {
            var res = await Task.Run(() => AdvApi32.CredDelete(name, AdvApi32.CRED_TYPE.CRED_TYPE_GENERIC));
            if (res)
            {
                _logger.LogInformation($"Deleted secret {name} successfully.");
            }
            else
            {
                _logger.LogError($"Failed to delete secret {name} on Windows.");
            }
            return res;
        }
        else if (OperatingSystem.IsMacOS())
        {
            using var process = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = "security",
                    Arguments = $"delete-generic-password -a default -s {name}",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            process.Start();
            await process.WaitForExitAsync();
            if (process.ExitCode == 0)
            {
                _logger.LogInformation($"Deleted secret {name} successfully.");
            }
            else
            {
                _logger.LogError($"Failed to delete secret {name} on macOS.");
            }
            return process.ExitCode == 0;
        }
        else if (OperatingSystem.IsLinux())
        {
            using var process = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = "secret-tool",
                    Arguments = $"clear schema Nickvision.Desktop application {name}",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            process.Start();
            await process.WaitForExitAsync();
            if (process.ExitCode == 0)
            {
                _logger.LogInformation($"Deleted secret {name} successfully.");
            }
            else
            {
                _logger.LogError($"Failed to delete secret {name} on Linux.");
            }
            return process.ExitCode == 0;
        }
        else
        {
            _logger.LogError($"Unable to delete secret {name} as the operating system is not supported.");
            return false;
        }
    }

    /// <summary>
    /// Gets a secret.
    /// </summary>
    /// <param name="name">The name of the secret to find</param>
    /// <returns>The secret if found, else null</returns>
    public Secret? Get(string name)
    {
        _logger.LogInformation($"Getting secret {name}...");
        if (string.IsNullOrEmpty(name))
        {
            _logger.LogWarning("Unable to get secret as the name is null or empty.");
            return null;
        }
        if (OperatingSystem.IsWindows())
        {
            if (!AdvApi32.CredRead(name, AdvApi32.CRED_TYPE.CRED_TYPE_GENERIC, out var credential))
            {
                _logger.LogInformation($"Secret {name} not found.");
                return null;
            }
            return credential.CredentialBlob is not null ? new Secret(name, Encoding.Unicode.GetString(credential.CredentialBlob)) : null;
        }
        else if (OperatingSystem.IsMacOS())
        {
            using var process = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = "security",
                    Arguments = $"find-generic-password -a default -s {name} -w",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            process.Start();
            process.WaitForExit();
            if (process.ExitCode != 0)
            {
                _logger.LogInformation($"Secret {name} not found.");
                return null;
            }
            var stdout = process.StandardOutput.ReadToEnd();
            return new Secret(name, stdout.Split('\n')[0]);
        }
        else if (OperatingSystem.IsLinux())
        {
            using var process = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = "secret-tool",
                    Arguments = $"lookup schema Nickvision.Desktop application {name}",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            process.Start();
            process.WaitForExit();
            if (process.ExitCode != 0)
            {
                _logger.LogInformation($"Secret {name} not found.");
                return null;
            }
            return new Secret(name, process.StandardOutput.ReadToEnd());
        }
        else
        {
            _logger.LogError($"Unable to get secret {name} as the operating system is not supported.");
            return null;
        }
    }

    /// <summary>
    /// Gets a secret asynchronously.
    /// </summary>
    /// <param name="name">The name of the secret to find</param>
    /// <returns>The secret if found, else null</returns>
    public async Task<Secret?> GetAsync(string name)
    {
        _logger.LogInformation($"Getting secret {name}...");
        if (string.IsNullOrEmpty(name))
        {
            _logger.LogWarning("Unable to get secret as the name is null or empty.");
            return null;
        }
        if (OperatingSystem.IsWindows())
        {
            var credential = await Task.Run<AdvApi32.CREDENTIAL_MGD?>(() =>
            {
                if (AdvApi32.CredRead(name, AdvApi32.CRED_TYPE.CRED_TYPE_GENERIC, out var c))
                {
                    return c;
                }
                return null;
            });
            if (credential is null)
            {
                _logger.LogInformation($"Secret {name} not found.");
            }
            return credential?.CredentialBlob is not null ? new Secret(name, Encoding.Unicode.GetString(credential.Value.CredentialBlob)) : null;
        }
        else if (OperatingSystem.IsMacOS())
        {
            using var process = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = "security",
                    Arguments = $"find-generic-password -a default -s {name} -w",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            process.Start();
            await process.WaitForExitAsync();
            if (process.ExitCode != 0)
            {
                _logger.LogInformation($"Secret {name} not found.");
                return null;
            }
            var stdout = process.StandardOutput.ReadToEnd();
            return new Secret(name, stdout.Split('\n')[0]);
        }
        else if (OperatingSystem.IsLinux())
        {
            using var process = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = "secret-tool",
                    Arguments = $"lookup schema Nickvision.Desktop application {name}",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            process.Start();
            await process.WaitForExitAsync();
            if (process.ExitCode != 0)
            {
                _logger.LogInformation($"Secret {name} not found.");
                return null;
            }
            return new Secret(name, await process.StandardOutput.ReadToEndAsync());
        }
        else
        {
            _logger.LogError($"Unable to get secret {name} as the operating system is not supported.");
            return null;
        }
    }

    /// <summary>
    /// Updates a secret.
    /// </summary>
    /// <param name="secret">The secret to update</param>
    /// <returns>True if the secret was updated successfully, else false</returns>
    public bool Update(Secret secret)
    {
        _logger.LogInformation($"Updating secret {secret.Name}...");
        if (secret.Empty)
        {
            _logger.LogError($"Unable to update secret {secret.Name} as it is empty.");
            return false;
        }
        if (OperatingSystem.IsWindows())
        {
            if (Get(secret.Name) is null)
            {
                _logger.LogError($"Secret {secret.Name} not found.");
                return false;
            }
            var stringPtr = Marshal.StringToHGlobalUni(secret.Value);
            var res = AdvApi32.CredWrite(new AdvApi32.CREDENTIAL
            {
                AttributeCount = 0,
                Attributes = nint.Zero,
                Type = AdvApi32.CRED_TYPE.CRED_TYPE_GENERIC,
                Persist = AdvApi32.CRED_PERSIST.CRED_PERSIST_LOCAL_MACHINE,
                TargetName = new StrPtrAuto(secret.Name),
                UserName = new StrPtrAuto("default"),
                CredentialBlobSize = (uint)Encoding.Unicode.GetByteCount(secret.Value),
                CredentialBlob = stringPtr
            },
                0);
            Marshal.FreeHGlobal(stringPtr);
            if (res)
            {
                _logger.LogInformation($"Updated secret {secret.Name} successfully.");
            }
            else
            {
                _logger.LogError($"Failed to update secret {secret.Name} on Windows.");
            }
            return res;
        }
        else if (OperatingSystem.IsMacOS())
        {
            using var process = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = "security",
                    Arguments = $"add-generic-password -a default -s {secret.Name} -w \"{secret.Value}\" -U",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            process.Start();
            process.WaitForExit();
            if (process.ExitCode == 0)
            {
                _logger.LogInformation($"Updated secret {secret.Name} successfully.");
            }
            else
            {
                _logger.LogError($"Failed to update secret {secret.Name} on macOS.");
            }
            return process.ExitCode == 0;
        }
        else if (OperatingSystem.IsLinux())
        {
            using var process = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = "secret-tool",
                    Arguments = $"store --label='{secret.Name}' schema Nickvision.Desktop application {secret.Name}",
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            process.Start();
            var stdin = process.StandardInput;
            stdin.Write(secret.Value);
            stdin.Close();
            process.WaitForExit();
            if (process.ExitCode == 0)
            {
                _logger.LogInformation($"Updated secret {secret.Name} successfully.");
            }
            else
            {
                _logger.LogError($"Failed to update secret {secret.Name} on Linux.");
            }
            return process.ExitCode == 0;
        }
        else
        {
            _logger.LogError($"Unable to update secret {secret.Name} as the operating system is not supported.");
            return false;
        }
    }

    /// <summary>
    /// Updates a secret asynchronously.
    /// </summary>
    /// <param name="secret">The secret to update</param>
    /// <returns>True if the secret was updated successfully, else false</returns>
    public async Task<bool> UpdateAsync(Secret secret)
    {
        _logger.LogInformation($"Updating secret {secret.Name}...");
        if (secret.Empty)
        {
            _logger.LogError($"Unable to update secret {secret.Name} as it is empty.");
            return false;
        }
        if (OperatingSystem.IsWindows())
        {
            if (await GetAsync(secret.Name) is null)
            {
                _logger.LogError($"Secret {secret.Name} not found.");
                return false;
            }
            var stringPtr = Marshal.StringToHGlobalUni(secret.Value);
            var res = await Task.Run(() => AdvApi32.CredWrite(new AdvApi32.CREDENTIAL
            {
                AttributeCount = 0,
                Attributes = nint.Zero,
                Type = AdvApi32.CRED_TYPE.CRED_TYPE_GENERIC,
                Persist = AdvApi32.CRED_PERSIST.CRED_PERSIST_LOCAL_MACHINE,
                TargetName = new StrPtrAuto(secret.Name),
                UserName = new StrPtrAuto("default"),
                CredentialBlobSize = (uint)Encoding.Unicode.GetByteCount(secret.Value),
                CredentialBlob = stringPtr
            },
                0));
            Marshal.FreeHGlobal(stringPtr);
            if (res)
            {
                _logger.LogInformation($"Updated secret {secret.Name} successfully.");
            }
            else
            {
                _logger.LogError($"Failed to update secret {secret.Name} on Windows.");
            }
            return res;
        }
        else if (OperatingSystem.IsMacOS())
        {
            using var process = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = "security",
                    Arguments = $"add-generic-password -a default -s {secret.Name} -w \"{secret.Value}\" -U",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            process.Start();
            await process.WaitForExitAsync();
            if (process.ExitCode == 0)
            {
                _logger.LogInformation($"Updated secret {secret.Name} successfully.");
            }
            else
            {
                _logger.LogError($"Failed to update secret {secret.Name} on macOS.");
            }
            return process.ExitCode == 0;
        }
        else if (OperatingSystem.IsLinux())
        {
            using var process = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = "secret-tool",
                    Arguments = $"store --label='{secret.Name}' schema Nickvision.Desktop application {secret.Name}",
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            process.Start();
            var stdin = process.StandardInput;
            await stdin.WriteAsync(secret.Value);
            stdin.Close();
            await process.WaitForExitAsync();
            if (process.ExitCode == 0)
            {
                _logger.LogInformation($"Updated secret {secret.Name} successfully.");
            }
            else
            {
                _logger.LogError($"Failed to update secret {secret.Name} on Linux.");
            }
            return process.ExitCode == 0;
        }
        else
        {
            _logger.LogError($"Unable to update secret {secret.Name} as the operating system is not supported.");
            return false;
        }
    }
}
