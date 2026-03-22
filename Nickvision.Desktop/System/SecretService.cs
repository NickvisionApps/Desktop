using DBus.Services.Secrets;
using Microsoft.Extensions.Logging;
using Nickvision.Desktop.Keyring;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Nickvision.Desktop.System;

/// <summary>
/// A service for managing secrets using the system's secret storage.
/// </summary>
public partial class SecretService : ISecretService
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
            var blob = Encoding.Unicode.GetBytes(secret.Value);
            var targetNamePtr = IntPtr.Zero;
            var userNamePtr = IntPtr.Zero;
            var blobPtr = IntPtr.Zero;
            var credPtr = IntPtr.Zero;
            try
            {
                targetNamePtr = Marshal.StringToHGlobalUni(secret.Name);
                userNamePtr = Marshal.StringToHGlobalUni("default");
                blobPtr = Marshal.AllocHGlobal(blob.Length);
                Marshal.Copy(blob, 0, blobPtr, blob.Length);
                unsafe
                {
                    credPtr = Marshal.AllocHGlobal(sizeof(CREDENTIAL_WIN32));
                    var cred = (CREDENTIAL_WIN32*)credPtr;
                    cred->Flags = 0;
                    cred->Type = _credTypeGeneric;
                    cred->TargetName = (char*)targetNamePtr;
                    cred->Comment = null;
                    cred->LastWritten = 0;
                    cred->CredentialBlobSize = (uint)blob.Length;
                    cred->CredentialBlob = (byte*)blobPtr;
                    cred->Persist = _credPersistLocalMachine;
                    cred->AttributeCount = 0;
                    cred->Attributes = null;
                    cred->TargetAlias = null;
                    cred->UserName = (char*)userNamePtr;
                }
                var (res, errorCode) = await Task.Run(() =>
                {
                    var r = CredWriteNative(credPtr, 0);
                    return (r, r ? 0 : Marshal.GetLastWin32Error());
                });
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
            finally
            {
                if (credPtr != IntPtr.Zero) Marshal.FreeHGlobal(credPtr);
                if (blobPtr != IntPtr.Zero) Marshal.FreeHGlobal(blobPtr);
                if (userNamePtr != IntPtr.Zero) Marshal.FreeHGlobal(userNamePtr);
                if (targetNamePtr != IntPtr.Zero) Marshal.FreeHGlobal(targetNamePtr);
            }
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
            var dbus = await DBus.Services.Secrets.SecretService.ConnectAsync(EncryptionType.Dh);
            var collection = await dbus.GetDefaultCollectionAsync() ?? await dbus.CreateCollectionAsync("Default keyring", "default");
            if (collection is null)
            {
                _logger.LogError($"Failed to add system secret ({secret.Name}) as the keyring collection could not be accessed.");
                return false;
            }
            await collection.UnlockAsync();
            var res = (await collection.CreateItemAsync(secret.Name, new Dictionary<string, string>()
            {
                { "application", secret.Name }
            }, Encoding.UTF8.GetBytes(secret.Value), "text/plain; charset=utf8", false)) is not null;
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
            var (res, errorCode) = await Task.Run(() =>
            {
                var r = CredDeleteNative(name, _credTypeGeneric, 0);
                return (r, r ? 0 : Marshal.GetLastWin32Error());
            });
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
            var dbus = await DBus.Services.Secrets.SecretService.ConnectAsync(EncryptionType.Dh);
            var collection = await dbus.GetDefaultCollectionAsync() ?? await dbus.CreateCollectionAsync("Default keyring", "default");
            if (collection is null)
            {
                _logger.LogError($"Failed to delete system secret ({name}) as the keyring collection could not be accessed.");
                return false;
            }
            await collection.UnlockAsync();
            var items = await collection.SearchItemsAsync(new Dictionary<string, string>()
            {
                { "application", name }
            });
            if (items.Length == 0)
            {
                _logger.LogWarning($"System secret ({name}) not found.");
            }
            else
            {
                await items[0].DeleteAsync();
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
            var credentialPtr = await Task.Run(() =>
            {
                if (CredReadNative(name, _credTypeGeneric, 0, out var ptr))
                {
                    return ptr;
                }
                return IntPtr.Zero;
            });
            if (credentialPtr == IntPtr.Zero)
            {
                _logger.LogInformation($"System secret ({name}) not found.");
                return null;
            }
            try
            {
                unsafe
                {
                    var cred = (CREDENTIAL_WIN32*)credentialPtr;
                    if (cred->CredentialBlob == null || cred->CredentialBlobSize == 0)
                    {
                        _logger.LogInformation($"System secret ({name}) not found.");
                        return null;
                    }
                    var blob = new ReadOnlySpan<byte>(cred->CredentialBlob, (int)cred->CredentialBlobSize);
                    return new Secret(name, Encoding.Unicode.GetString(blob));
                }
            }
            finally
            {
                CredFreeNative(credentialPtr);
            }
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
            var dbus = await DBus.Services.Secrets.SecretService.ConnectAsync(EncryptionType.Dh);
            var collection = await dbus.GetDefaultCollectionAsync() ?? await dbus.CreateCollectionAsync("Default keyring", "default");
            if (collection is null)
            {
                _logger.LogError($"Failed to get system secret ({name}) as the keyring collection could not be accessed.");
                return null;
            }
            await collection.UnlockAsync();
            var items = await collection.SearchItemsAsync(new Dictionary<string, string>()
            {
                { "application", name }
            });
            if (items.Length == 0)
            {
                _logger.LogInformation($"System secret ({name}) not found.");
                return null;
            }
            return new Secret(name, Encoding.UTF8.GetString(await items[0].GetSecretAsync()));
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
        if (OperatingSystem.IsWindows())
        {
            if (await GetAsync(secret.Name) is null)
            {
                _logger.LogError($"Unable to update system secret ({secret.Name}) as it does not exist.");
                return false;
            }
            var blob = Encoding.Unicode.GetBytes(secret.Value);
            var targetNamePtr = IntPtr.Zero;
            var userNamePtr = IntPtr.Zero;
            var blobPtr = IntPtr.Zero;
            var credPtr = IntPtr.Zero;
            try
            {
                targetNamePtr = Marshal.StringToHGlobalUni(secret.Name);
                userNamePtr = Marshal.StringToHGlobalUni("default");
                blobPtr = Marshal.AllocHGlobal(blob.Length);
                Marshal.Copy(blob, 0, blobPtr, blob.Length);
                unsafe
                {
                    credPtr = Marshal.AllocHGlobal(sizeof(CREDENTIAL_WIN32));
                    var cred = (CREDENTIAL_WIN32*)credPtr;
                    cred->Flags = 0;
                    cred->Type = _credTypeGeneric;
                    cred->TargetName = (char*)targetNamePtr;
                    cred->Comment = null;
                    cred->LastWritten = 0;
                    cred->CredentialBlobSize = (uint)blob.Length;
                    cred->CredentialBlob = (byte*)blobPtr;
                    cred->Persist = _credPersistLocalMachine;
                    cred->AttributeCount = 0;
                    cred->Attributes = null;
                    cred->TargetAlias = null;
                    cred->UserName = (char*)userNamePtr;
                }
                var (res, errorCode) = await Task.Run(() =>
                {
                    var r = CredWriteNative(credPtr, 0);
                    return (r, r ? 0 : Marshal.GetLastWin32Error());
                });
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
            finally
            {
                if (credPtr != IntPtr.Zero) Marshal.FreeHGlobal(credPtr);
                if (blobPtr != IntPtr.Zero) Marshal.FreeHGlobal(blobPtr);
                if (userNamePtr != IntPtr.Zero) Marshal.FreeHGlobal(userNamePtr);
                if (targetNamePtr != IntPtr.Zero) Marshal.FreeHGlobal(targetNamePtr);
            }
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
            var dbus = await DBus.Services.Secrets.SecretService.ConnectAsync(EncryptionType.Dh);
            var collection = await dbus.GetDefaultCollectionAsync() ?? await dbus.CreateCollectionAsync("Default keyring", "default");
            if (collection is null)
            {
                _logger.LogError($"Failed to update system secret ({secret.Name}) as the keyring collection could not be accessed.");
                return false;
            }
            await collection.UnlockAsync();
            var items = await collection.SearchItemsAsync(new Dictionary<string, string>()
            {
                { "application", secret.Name }
            });
            if (items.Length == 0)
            {
                _logger.LogError($"Failed to update system secret ({secret.Name}).");
            }
            else
            {
                await items[0].SetSecret(Encoding.UTF8.GetBytes(secret.Value), "text/plain; charset=utf8");
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
    /// A NativeAOT-compatible representation of the Windows CREDENTIAL structure.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    private unsafe struct CREDENTIAL_WIN32
    {
        public uint Flags;
        public uint Type;
        public char* TargetName;
        public char* Comment;
        public ulong LastWritten; // FILETIME (two DWORDs, treated as opaque 64-bit value)
        public uint CredentialBlobSize;
        public byte* CredentialBlob;
        public uint Persist;
        public uint AttributeCount;
        public void* Attributes;
        public char* TargetAlias;
        public char* UserName;
    }

    private const uint _credTypeGeneric = 1;        // CRED_TYPE_GENERIC
    private const uint _credPersistLocalMachine = 2; // CRED_PERSIST_LOCAL_MACHINE

    [LibraryImport("advapi32.dll", EntryPoint = "CredReadW", StringMarshalling = StringMarshalling.Utf16, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool CredReadNative(string target, uint type, uint flags, out IntPtr credential);

    [LibraryImport("advapi32.dll", EntryPoint = "CredWriteW", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool CredWriteNative(IntPtr credential, uint flags);

    [LibraryImport("advapi32.dll", EntryPoint = "CredDeleteW", StringMarshalling = StringMarshalling.Utf16, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool CredDeleteNative(string target, uint type, uint flags);

    [LibraryImport("advapi32.dll", EntryPoint = "CredFree")]
    private static partial void CredFreeNative(IntPtr buffer);
}
