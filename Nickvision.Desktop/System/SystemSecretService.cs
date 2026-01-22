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
///     A service for managing secrets using the system's secret storage.
/// </summary>
public class SystemSecretService : ISecretService
{
    /// <summary>
    ///     Adds a secret.
    /// </summary>
    /// <param name="secret">The secret to add</param>
    /// <returns>True if the secret was added successfully, else false</returns>
    public bool Add(Secret secret)
    {
        if (secret.Empty)
        {
            return false;
        }
        if (Get(secret.Name) is not null)
        {
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
            },
                0);
            Marshal.FreeHGlobal(stringPtr);
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
            return process.ExitCode == 0;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    ///     Adds a secret asynchronously.
    /// </summary>
    /// <param name="secret">The secret to add</param>
    /// <returns>True if the secret was added successfully, else false</returns>
    public async Task<bool> AddAsync(Secret secret)
    {
        if (secret.Empty)
        {
            return false;
        }
        if (await GetAsync(secret.Name) is not null)
        {
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
            return process.ExitCode == 0;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    ///     Creates a secret with a random but secure value.
    /// </summary>
    /// <param name="name">The name of the secret to create</param>
    /// <returns>The created secret if successful, else null</returns>
    public Secret? Create(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return null;
        }
        if (Get(name) is not null)
        {
            return null;
        }
        var secret = new Secret(name, new PasswordGenerator().Next(64));
        return Add(secret) ? secret : null;
    }

    /// <summary>
    ///     Creates a secret asynchronously with a random but secure value.
    /// </summary>
    /// <param name="name">The name of the secret to create</param>
    /// <returns>The created secret if successful, else null</returns>
    public async Task<Secret?> CreateAsync(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return null;
        }
        if (await GetAsync(name) is not null)
        {
            return null;
        }
        var secret = new Secret(name, new PasswordGenerator().Next(64));
        return await AddAsync(secret) ? secret : null;
    }

    /// <summary>
    ///     Deletes a secret.
    /// </summary>
    /// <param name="name">The name of the secret to delete</param>
    /// <returns>True if the secret was deleted successfully, else false</returns>
    public bool Delete(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return false;
        }
        if (OperatingSystem.IsWindows())
        {
            return AdvApi32.CredDelete(name, AdvApi32.CRED_TYPE.CRED_TYPE_GENERIC);
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
            return process.ExitCode == 0;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    ///     Deletes a secret asynchronously.
    /// </summary>
    /// <param name="name">The name of the secret to delete</param>
    /// <returns>True if the secret was deleted successfully, else false</returns>
    public async Task<bool> DeleteAsync(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return false;
        }
        if (OperatingSystem.IsWindows())
        {
            return await Task.Run(() => AdvApi32.CredDelete(name, AdvApi32.CRED_TYPE.CRED_TYPE_GENERIC));
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
            return process.ExitCode == 0;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    ///     Gets a secret.
    /// </summary>
    /// <param name="name">The name of the secret to find</param>
    /// <returns>The secret if found, else null</returns>
    public Secret? Get(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return null;
        }
        if (OperatingSystem.IsWindows())
        {
            if (!AdvApi32.CredRead(name, AdvApi32.CRED_TYPE.CRED_TYPE_GENERIC, out var credential))
            {
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
                return null;
            }
            return new Secret(name, process.StandardOutput.ReadToEnd());
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    ///     Gets a secret asynchronously.
    /// </summary>
    /// <param name="name">The name of the secret to find</param>
    /// <returns>The secret if found, else null</returns>
    public async Task<Secret?> GetAsync(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
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
                return null;
            }
            return new Secret(name, await process.StandardOutput.ReadToEndAsync());
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    ///     Updates a secret.
    /// </summary>
    /// <param name="secret">The secret to update</param>
    /// <returns>True if the secret was updated successfully, else false</returns>
    public bool Update(Secret secret)
    {
        if (secret.Empty)
        {
            return false;
        }
        if (OperatingSystem.IsWindows())
        {
            if (Get(secret.Name) is null)
            {
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
            return process.ExitCode == 0;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    ///     Updates a secret asynchronously.
    /// </summary>
    /// <param name="secret">The secret to update</param>
    /// <returns>True if the secret was updated successfully, else false</returns>
    public async Task<bool> UpdateAsync(Secret secret)
    {
        if (secret.Empty)
        {
            return false;
        }
        if (OperatingSystem.IsWindows())
        {
            if (await GetAsync(secret.Name) is null)
            {
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
            return process.ExitCode == 0;
        }
        else
        {
            return false;
        }
    }
}
