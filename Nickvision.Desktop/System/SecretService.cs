using Microsoft.Extensions.Logging;
using Nickvision.Desktop.FreeDesktop;
using Nickvision.Desktop.Keyring;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Security.Credentials;

namespace Nickvision.Desktop.System;

public class SecretService : ISecretService
{
    private readonly ILogger<SecretService> _logger;

    public SecretService(ILogger<SecretService> logger)
    {
        _logger = logger;
    }

    public async Task<bool> AddAsync(Secret secret)
    {
        _logger.LogDebug($"Adding system secret ({secret.Name}).");
        if (secret.Empty)
        {
            _logger.LogError($"Unable to add system secret ({secret.Name}) as it is empty.");
            return false;
        }
        if (await GetAsync(secret.Name) is not null)
        {
            _logger.LogError($"Unable to add system secret ({secret.Name}) as it already exists.");
            return false;
        }
        if (OperatingSystem.IsWindows())
        {
            var (res, errorCode) = await Task.Run<(bool, int)>(() =>
            {
                unsafe
                {
                    var blob = Encoding.Unicode.GetBytes(secret.Value);
                    bool r;
                    int code;
                    fixed (char* targetName = secret.Name)
                    fixed (char* userName = "default")
                    fixed (byte* blobPtr = blob)
                    {
                        var cred = new CREDENTIALW
                        {
                            Type = CRED_TYPE.CRED_TYPE_GENERIC,
                            TargetName = new PWSTR(targetName),
                            CredentialBlobSize = (uint)blob.Length,
                            CredentialBlob = blobPtr,
                            Persist = CRED_PERSIST.CRED_PERSIST_LOCAL_MACHINE,
                            UserName = new PWSTR(userName),
                        };
#pragma warning disable CA1416
                        r = PInvoke.CredWrite(in cred, 0);
#pragma warning restore CA1416
                        code = r ? 0 : Marshal.GetLastWin32Error();
                    }
                    return (r, code);
                }
            });
            if (res)
            {
                _logger.LogDebug($"Added system secret ({secret.Name}) successfully.");
            }
            else
            {
                _logger.LogError($"Failed to add system secret ({secret.Name}): Win32 error {errorCode}");
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
                _logger.LogDebug($"Added system secret ({secret.Name}) successfully.");
            }
            else
            {
                _logger.LogError($"Failed to add system secret ({secret.Name}): {await process.StandardOutput.ReadToEndAsync()}\n{await process.StandardError.ReadToEndAsync()}");
            }
            return process.ExitCode == 0;
        }
        else if (OperatingSystem.IsLinux())
        {
            using var svc = await SecretServiceProxy.ConnectAsync();
            if (svc is null)
            {
                _logger.LogError($"Failed to add system secret ({secret.Name}): unable to connect to secrets service.");
                return false;
            }
            var collPath = await svc.GetDefaultCollectionPathAsync();
            if (string.IsNullOrEmpty(collPath) || collPath == "/")
            {
                collPath = await svc.CreateCollectionAsync("Default keyring", "default");
            }
            if (string.IsNullOrEmpty(collPath) || collPath == "/")
            {
                _logger.LogError($"Failed to add system secret ({secret.Name}) as the keyring collection could not be accessed.");
                return false;
            }
            await svc.UnlockAsync(collPath);
            var itemPath = await svc.CreateItemAsync(collPath, secret.Name, new Dictionary<string, string> { { "application", secret.Name } }, secret.Value);
            var res = !string.IsNullOrEmpty(itemPath);
            if (res)
            {
                _logger.LogDebug($"Added system secret ({secret.Name}) successfully.");
            }
            else
            {
                _logger.LogError($"Failed to add system secret ({secret.Name}).");
            }
            return res;
        }
        else
        {
            _logger.LogError($"Unable to add system secret. The OS is unsupported.");
            return false;
        }
    }

    public async Task<Secret?> CreateAsync(string name)
    {
        _logger.LogDebug($"Creating system secret ({name}).");
        if (string.IsNullOrEmpty(name))
        {
            _logger.LogError("Unable to create system secret as the name is null or empty.");
            return null;
        }
        var secret = new Secret(name, new PasswordGenerator().Next(64));
        var result = await AddAsync(secret) ? secret : null;
        if (result is null)
        {
            _logger.LogError($"Failed to create system secret ({name}).");
        }
        else
        {
            _logger.LogDebug($"Created system secret ({name}) successfully.");
        }
        return result;
    }

    public async Task<bool> DeleteAsync(string name)
    {
        _logger.LogDebug($"Deleting system secret ({name}).");
        if (string.IsNullOrEmpty(name))
        {
            _logger.LogError("Unable to delete system secret as the name is null or empty.");
            return false;
        }
        if (OperatingSystem.IsWindows())
        {
            var (res, errorCode) = await Task.Run(() =>
            {
#pragma warning disable CA1416
                var r = PInvoke.CredDelete(name, CRED_TYPE.CRED_TYPE_GENERIC);
#pragma warning restore CA1416
                return (r, r ? 0 : Marshal.GetLastWin32Error());
            });
            if (res)
            {
                _logger.LogDebug($"Deleted system secret ({name}) successfully.");
            }
            else
            {
                _logger.LogError($"Failed to delete system secret ({name}): Win32 error {errorCode}");
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
                _logger.LogDebug($"Deleted system secret ({name}) successfully.");
            }
            else
            {
                _logger.LogError($"Failed to delete system secret ({name}): {await process.StandardOutput.ReadToEndAsync()}\n{await process.StandardError.ReadToEndAsync()}");
            }
            return process.ExitCode == 0;
        }
        else if (OperatingSystem.IsLinux())
        {
            using var svc = await SecretServiceProxy.ConnectAsync();
            if (svc is null)
            {
                _logger.LogError($"Failed to delete system secret ({name}): unable to connect to secrets service.");
                return false;
            }
            var (unlocked, locked) = await svc.SearchItemsAsync(new Dictionary<string, string> { { "application", name } });
            var itemPath = unlocked.Length > 0 ? unlocked[0] : locked.Length > 0 ? locked[0] : null;
            if (itemPath is null)
            {
                _logger.LogWarning($"System secret ({name}) not found.");
                return false;
            }
            if (locked.Length > 0 && !await svc.UnlockAsync(itemPath))
            {
                _logger.LogError($"Failed to delete system secret ({name}): the user dismissed the unlock prompt.");
                return false;
            }
            if (!await svc.DeleteItemAsync(itemPath))
            {
                _logger.LogError($"Failed to delete system secret ({name}): the user dismissed the deletion prompt.");
                return false;
            }
            _logger.LogDebug($"Deleted system secret ({name}) successfully.");
            return true;
        }
        else
        {
            _logger.LogError($"Unable to delete system secret. The OS is unsupported.");
            return false;
        }
    }

    public async Task<Secret?> GetAsync(string name)
    {
        _logger.LogDebug($"Getting system secret ({name}).");
        if (string.IsNullOrEmpty(name))
        {
            _logger.LogError("Unable to get system secret as the name is null or empty.");
            return null;
        }
        if (OperatingSystem.IsWindows())
        {
            var value = await Task.Run<string?>(() =>
            {
                unsafe
                {
#pragma warning disable CA1416
                    if (!PInvoke.CredRead(name, CRED_TYPE.CRED_TYPE_GENERIC, out var credential))
#pragma warning restore CA1416
                    {
                        return null;
                    }
                    try
                    {
                        if (credential->CredentialBlob == null || credential->CredentialBlobSize == 0)
                        {
                            return null;
                        }
                        return Encoding.Unicode.GetString(new ReadOnlySpan<byte>(credential->CredentialBlob, (int)credential->CredentialBlobSize));
                    }
                    finally
                    {
#pragma warning disable CA1416
                        PInvoke.CredFree(credential);
#pragma warning restore CA1416
                    }
                }
            });
            if (value is null)
            {
                _logger.LogDebug($"System secret ({name}) not found.");
                return null;
            }
            return new Secret(name, value);
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
                _logger.LogDebug($"System secret ({name}) not found.");
                return null;
            }
            var stdout = process.StandardOutput.ReadToEnd();
            return new Secret(name, stdout.Split('\n')[0]);
        }
        else if (OperatingSystem.IsLinux())
        {
            using var svc = await SecretServiceProxy.ConnectAsync();
            if (svc is null)
            {
                _logger.LogError($"Failed to get system secret ({name}): unable to connect to secrets service.");
                return null;
            }
            var (unlocked, locked) = await svc.SearchItemsAsync(new Dictionary<string, string> { { "application", name } });
            string? itemPath = null;
            if (unlocked.Length > 0)
            {
                itemPath = unlocked[0];
            }
            else if (locked.Length > 0)
            {
                if (!await svc.UnlockAsync(locked[0]))
                {
                    _logger.LogError($"Failed to get system secret ({name}): the user dismissed the unlock prompt.");
                    return null;
                }
                itemPath = locked[0];
            }
            if (itemPath is null)
            {
                _logger.LogDebug($"System secret ({name}) not found.");
                return null;
            }
            var value = await svc.GetSecretAsync(itemPath);
            return value is null ? null : new Secret(name, value);
        }
        else
        {
            _logger.LogError($"Unable to get system secret. The OS is unsupported.");
            return null;
        }
    }

    public async Task<bool> UpdateAsync(Secret secret)
    {
        _logger.LogDebug($"Updating system secret ({secret.Name}).");
        if (secret.Empty)
        {
            _logger.LogError($"Unable to update system secret ({secret.Name}) as it is empty.");
            return false;
        }
        if (await GetAsync(secret.Name) is null)
        {
            _logger.LogError($"Unable to update system secret ({secret.Name}) as it does not exist.");
            return false;
        }
        if (OperatingSystem.IsWindows())
        {
            var (res, errorCode) = await Task.Run<(bool, int)>(() =>
            {
                unsafe
                {
                    var blob = Encoding.Unicode.GetBytes(secret.Value);
                    bool r;
                    int code;
                    fixed (char* targetName = secret.Name)
                    fixed (char* userName = "default")
                    fixed (byte* blobPtr = blob)
                    {
                        var cred = new CREDENTIALW
                        {
                            Type = CRED_TYPE.CRED_TYPE_GENERIC,
                            TargetName = new PWSTR(targetName),
                            CredentialBlobSize = (uint)blob.Length,
                            CredentialBlob = blobPtr,
                            Persist = CRED_PERSIST.CRED_PERSIST_LOCAL_MACHINE,
                            UserName = new PWSTR(userName),
                        };
#pragma warning disable CA1416
                        r = PInvoke.CredWrite(in cred, 0);
#pragma warning restore CA1416
                        code = r ? 0 : Marshal.GetLastWin32Error();
                    }
                    return (r, code);
                }
            });
            if (res)
            {
                _logger.LogDebug($"Updated system secret ({secret.Name}) successfully.");
            }
            else
            {
                _logger.LogError($"Failed to update system secret ({secret.Name}): Win32 error {errorCode}");
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
                _logger.LogDebug($"Updated system secret ({secret.Name}) successfully.");
            }
            else
            {
                _logger.LogError($"Failed to update system secret ({secret.Name}): {await process.StandardOutput.ReadToEndAsync()}\n{await process.StandardError.ReadToEndAsync()}");
            }
            return process.ExitCode == 0;
        }
        else if (OperatingSystem.IsLinux())
        {
            using var svc = await SecretServiceProxy.ConnectAsync();
            if (svc is null)
            {
                _logger.LogError($"Failed to update system secret ({secret.Name}): unable to connect to secrets service.");
                return false;
            }
            var (unlocked, locked) = await svc.SearchItemsAsync(new Dictionary<string, string> { { "application", secret.Name } });
            string? itemPath = null;
            if (unlocked.Length > 0)
            {
                itemPath = unlocked[0];
            }
            else if (locked.Length > 0)
            {
                if (!await svc.UnlockAsync(locked[0]))
                {
                    _logger.LogError($"Failed to update system secret ({secret.Name}): the user dismissed the unlock prompt.");
                    return false;
                }
                itemPath = locked[0];
            }
            if (itemPath is null)
            {
                _logger.LogError($"Failed to update system secret ({secret.Name}).");
                return false;
            }
            await svc.SetSecretAsync(itemPath, secret.Value);
            _logger.LogDebug($"Updated system secret ({secret.Name}) successfully.");
            return true;
        }
        else
        {
            _logger.LogError($"Unable to update system secret. The OS is unsupported.");
            return false;
        }
    }
}
