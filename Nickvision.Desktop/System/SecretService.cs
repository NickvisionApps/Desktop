using Microsoft.Extensions.Logging;
using Nickvision.Desktop.FreeDesktop;
using Nickvision.Desktop.Keyring;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Security.Credentials;

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
    /// Adds a secret asynchronously.
    /// </summary>
    /// <param name="secret">The secret to add</param>
    /// <returns>True if the secret was added successfully, else false</returns>
    public async Task<bool> AddAsync(Secret secret)
    {
        _logger.LogInformation($"Adding system secret ({secret.Name}).");
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
            var (res, errorCode) = await Task.Run(() => WriteCredentialWindows(secret.Name, secret.Value));
            if (res)
            {
                _logger.LogInformation($"Added system secret ({secret.Name}) successfully.");
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
                _logger.LogInformation($"Added system secret ({secret.Name}) successfully.");
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
            if (string.IsNullOrEmpty(collPath))
            {
                _logger.LogError($"Failed to add system secret ({secret.Name}) as the keyring collection could not be accessed.");
                return false;
            }
            await svc.UnlockAsync(collPath);
            var itemPath = await svc.CreateItemAsync(collPath, secret.Name,
                new Dictionary<string, string> { { "application", secret.Name } },
                secret.Value);
            var res = !string.IsNullOrEmpty(itemPath);
            if (res)
            {
                _logger.LogInformation($"Added system secret ({secret.Name}) successfully.");
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

    /// <summary>
    /// Creates a secret asynchronously with a random but secure value.
    /// </summary>
    /// <param name="name">The name of the secret to create</param>
    /// <returns>The created secret if successful, else null</returns>
    public async Task<Secret?> CreateAsync(string name)
    {
        _logger.LogInformation($"Creating system secret ({name}).");
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
            _logger.LogInformation($"Created system secret ({name}) successfully.");
        }
        return result;
    }

    /// <summary>
    /// Deletes a secret asynchronously.
    /// </summary>
    /// <param name="name">The name of the secret to delete</param>
    /// <returns>True if the secret was deleted successfully, else false</returns>
    public async Task<bool> DeleteAsync(string name)
    {
        _logger.LogInformation($"Deleting system secret ({name}).");
        if (string.IsNullOrEmpty(name))
        {
            _logger.LogError("Unable to delete system secret as the name is null or empty.");
            return false;
        }
        if (OperatingSystem.IsWindows())
        {
            var (res, errorCode) = await Task.Run(() => DeleteCredentialWindows(name));
            if (res)
            {
                _logger.LogInformation($"Deleted system secret ({name}) successfully.");
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
                _logger.LogInformation($"Deleted system secret ({name}) successfully.");
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
            var collPath = await svc.GetDefaultCollectionPathAsync();
            if (string.IsNullOrEmpty(collPath) || collPath == "/")
            {
                collPath = await svc.CreateCollectionAsync("Default keyring", "default");
            }
            if (string.IsNullOrEmpty(collPath))
            {
                _logger.LogError($"Failed to delete system secret ({name}) as the keyring collection could not be accessed.");
                return false;
            }
            await svc.UnlockAsync(collPath);
            var items = await svc.SearchItemsAsync(collPath,
                new Dictionary<string, string> { { "application", name } });
            if (items.Length == 0)
            {
                _logger.LogWarning($"System secret ({name}) not found.");
            }
            else
            {
                await svc.DeleteItemAsync(items[0]);
                _logger.LogInformation($"Deleted system secret ({name}) successfully.");
            }
            return items.Length > 0;
        }
        else
        {
            _logger.LogError($"Unable to delete system secret. The OS is unsupported.");
            return false;
        }
    }

    /// <summary>
    /// Gets a secret asynchronously.
    /// </summary>
    /// <param name="name">The name of the secret to find</param>
    /// <returns>The secret if found, else null</returns>
    public async Task<Secret?> GetAsync(string name)
    {
        _logger.LogInformation($"Getting system secret ({name}).");
        if (string.IsNullOrEmpty(name))
        {
            _logger.LogError("Unable to get system secret as the name is null or empty.");
            return null;
        }
        if (OperatingSystem.IsWindows())
        {
            var value = await Task.Run(() => ReadCredentialWindows(name));
            if (value is null)
            {
                _logger.LogInformation($"System secret ({name}) not found.");
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
                _logger.LogInformation($"System secret ({name}) not found.");
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
            var collPath = await svc.GetDefaultCollectionPathAsync();
            if (string.IsNullOrEmpty(collPath) || collPath == "/")
            {
                collPath = await svc.CreateCollectionAsync("Default keyring", "default");
            }
            if (string.IsNullOrEmpty(collPath))
            {
                _logger.LogError($"Failed to get system secret ({name}) as the keyring collection could not be accessed.");
                return null;
            }
            await svc.UnlockAsync(collPath);
            var items = await svc.SearchItemsAsync(collPath,
                new Dictionary<string, string> { { "application", name } });
            if (items.Length == 0)
            {
                _logger.LogInformation($"System secret ({name}) not found.");
                return null;
            }
            var value = await svc.GetSecretAsync(items[0]);
            return value is null ? null : new Secret(name, value);
        }
        else
        {
            _logger.LogError($"Unable to get system secret. The OS is unsupported.");
            return null;
        }
    }

    /// <summary>
    /// Updates a secret asynchronously.
    /// </summary>
    /// <param name="secret">The secret to update</param>
    /// <returns>True if the secret was updated successfully, else false</returns>
    public async Task<bool> UpdateAsync(Secret secret)
    {
        _logger.LogInformation($"Updating system secret ({secret.Name}).");
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
            var (res, errorCode) = await Task.Run(() => WriteCredentialWindows(secret.Name, secret.Value));
            if (res)
            {
                _logger.LogInformation($"Updated system secret ({secret.Name}) successfully.");
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
                _logger.LogInformation($"Updated system secret ({secret.Name}) successfully.");
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
            var collPath = await svc.GetDefaultCollectionPathAsync();
            if (string.IsNullOrEmpty(collPath) || collPath == "/")
            {
                collPath = await svc.CreateCollectionAsync("Default keyring", "default");
            }
            if (string.IsNullOrEmpty(collPath))
            {
                _logger.LogError($"Failed to update system secret ({secret.Name}) as the keyring collection could not be accessed.");
                return false;
            }
            await svc.UnlockAsync(collPath);
            var items = await svc.SearchItemsAsync(collPath,
                new Dictionary<string, string> { { "application", secret.Name } });
            if (items.Length == 0)
            {
                _logger.LogError($"Failed to update system secret ({secret.Name}).");
            }
            else
            {
                await svc.SetSecretAsync(items[0], secret.Value);
                _logger.LogInformation($"Updated system secret ({secret.Name}) successfully.");
            }
            return items.Length > 0;
        }
        else
        {
            _logger.LogError($"Unable to update system secret. The OS is unsupported.");
            return false;
        }
    }

    /// <summary>
    /// Reads a generic credential from the Windows Credential Manager.
    /// </summary>
    /// <param name="name">The credential target name</param>
    /// <returns>The credential value if found, else null</returns>
    [SupportedOSPlatform("windows")]
    private static unsafe string? ReadCredentialWindows(string name)
    {
        if (!PInvoke.CredRead(name, CRED_TYPE.CRED_TYPE_GENERIC, out var credential))
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
            PInvoke.CredFree(credential);
        }
    }

    /// <summary>
    /// Writes (creates or updates) a generic credential in the Windows Credential Manager.
    /// </summary>
    /// <param name="name">The credential target name</param>
    /// <param name="value">The credential value to store</param>
    /// <returns>A tuple of (success, Win32 error code). Error code is 0 on success.</returns>
    [SupportedOSPlatform("windows")]
    private static unsafe (bool Success, int ErrorCode) WriteCredentialWindows(string name, string value)
    {
        var blob = Encoding.Unicode.GetBytes(value);
        fixed (char* targetName = name)
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
            var r = PInvoke.CredWrite(in cred, 0);
            return (r, r ? 0 : Marshal.GetLastWin32Error());
        }
    }

    /// <summary>
    /// Deletes a generic credential from the Windows Credential Manager.
    /// </summary>
    /// <param name="name">The credential target name</param>
    /// <returns>A tuple of (success, Win32 error code). Error code is 0 on success.</returns>
    [SupportedOSPlatform("windows")]
    private static (bool Success, int ErrorCode) DeleteCredentialWindows(string name)
    {
        var r = PInvoke.CredDelete(name, CRED_TYPE.CRED_TYPE_GENERIC);
        return (r, r ? 0 : Marshal.GetLastWin32Error());
    }
}
