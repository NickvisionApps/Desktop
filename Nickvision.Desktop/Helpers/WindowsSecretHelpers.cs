using System;
using System.Runtime.InteropServices;
using System.Text;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Security.Credentials;

namespace Nickvision.Desktop.Helpers;

/// <summary>
/// NativeAOT-compatible P/Invoke helpers for Windows Credential Manager (advapi32).
/// </summary>
internal static class WindowsSecretHelpers
{
    /// <summary>
    /// Reads a generic credential from the Windows Credential Manager.
    /// </summary>
    /// <param name="name">The credential target name</param>
    /// <returns>The credential value if found, else null</returns>
    internal static unsafe string? ReadCredential(string name)
    {
#pragma warning disable CA1416
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
#pragma warning restore CA1416
    }

    /// <summary>
    /// Writes (creates or updates) a generic credential in the Windows Credential Manager.
    /// </summary>
    /// <param name="name">The credential target name</param>
    /// <param name="value">The credential value to store</param>
    /// <returns>A tuple of (success, Win32 error code). Error code is 0 on success.</returns>
    internal static unsafe (bool Success, int ErrorCode) WriteCredential(string name, string value)
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
#pragma warning disable CA1416
            var r = PInvoke.CredWrite(in cred, 0);
#pragma warning restore CA1416
            return (r, r ? 0 : Marshal.GetLastWin32Error());
        }
    }

    /// <summary>
    /// Deletes a generic credential from the Windows Credential Manager.
    /// </summary>
    /// <param name="name">The credential target name</param>
    /// <returns>A tuple of (success, Win32 error code). Error code is 0 on success.</returns>
    internal static (bool Success, int ErrorCode) DeleteCredential(string name)
    {
#pragma warning disable CA1416
        var r = PInvoke.CredDelete(name, CRED_TYPE.CRED_TYPE_GENERIC);
#pragma warning restore CA1416
        return (r, r ? 0 : Marshal.GetLastWin32Error());
    }
}
