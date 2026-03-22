using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Nickvision.Desktop.Helpers;

/// <summary>
/// NativeAOT-compatible P/Invoke helpers for Windows Credential Manager (advapi32).
/// </summary>
internal static partial class WindowsSecretHelpers
{
    private const uint CredTypeGeneric = 1;         // CRED_TYPE_GENERIC
    private const uint CredPersistLocalMachine = 2; // CRED_PERSIST_LOCAL_MACHINE

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

    [LibraryImport("advapi32.dll", EntryPoint = "CredReadW", StringMarshalling = StringMarshalling.Utf16, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool CredReadNative(string target, uint type, uint flags, out nint credential);

    [LibraryImport("advapi32.dll", EntryPoint = "CredWriteW", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool CredWriteNative(nint credential, uint flags);

    [LibraryImport("advapi32.dll", EntryPoint = "CredDeleteW", StringMarshalling = StringMarshalling.Utf16, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool CredDeleteNative(string target, uint type, uint flags);

    [LibraryImport("advapi32.dll", EntryPoint = "CredFree")]
    private static partial void CredFreeNative(nint buffer);

    /// <summary>
    /// Reads a generic credential from the Windows Credential Manager.
    /// </summary>
    /// <param name="name">The credential target name</param>
    /// <returns>The credential value if found, else null</returns>
    internal static string? ReadCredential(string name)
    {
        if (!CredReadNative(name, CredTypeGeneric, 0, out var credentialPtr))
        {
            return null;
        }
        try
        {
            unsafe
            {
                var cred = (CREDENTIAL_WIN32*)credentialPtr;
                if (cred->CredentialBlob == null || cred->CredentialBlobSize == 0)
                {
                    return null;
                }
                return Encoding.Unicode.GetString(new ReadOnlySpan<byte>(cred->CredentialBlob, (int)cred->CredentialBlobSize));
            }
        }
        finally
        {
            CredFreeNative(credentialPtr);
        }
    }

    /// <summary>
    /// Writes (creates or updates) a generic credential in the Windows Credential Manager.
    /// </summary>
    /// <param name="name">The credential target name</param>
    /// <param name="value">The credential value to store</param>
    /// <returns>A tuple of (success, Win32 error code). Error code is 0 on success.</returns>
    internal static (bool Success, int ErrorCode) WriteCredential(string name, string value)
    {
        var blob = Encoding.Unicode.GetBytes(value);
        var targetNamePtr = IntPtr.Zero;
        var userNamePtr = IntPtr.Zero;
        var blobPtr = IntPtr.Zero;
        var credPtr = IntPtr.Zero;
        try
        {
            targetNamePtr = Marshal.StringToHGlobalUni(name);
            userNamePtr = Marshal.StringToHGlobalUni("default");
            blobPtr = Marshal.AllocHGlobal(blob.Length);
            Marshal.Copy(blob, 0, blobPtr, blob.Length);
            unsafe
            {
                credPtr = Marshal.AllocHGlobal(sizeof(CREDENTIAL_WIN32));
                var cred = (CREDENTIAL_WIN32*)credPtr;
                cred->Flags = 0;
                cred->Type = CredTypeGeneric;
                cred->TargetName = (char*)targetNamePtr;
                cred->Comment = null;
                cred->LastWritten = 0;
                cred->CredentialBlobSize = (uint)blob.Length;
                cred->CredentialBlob = (byte*)blobPtr;
                cred->Persist = CredPersistLocalMachine;
                cred->AttributeCount = 0;
                cred->Attributes = null;
                cred->TargetAlias = null;
                cred->UserName = (char*)userNamePtr;
            }
            var r = CredWriteNative(credPtr, 0);
            return (r, r ? 0 : Marshal.GetLastWin32Error());
        }
        finally
        {
            if (credPtr != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(credPtr);
            }
            if (blobPtr != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(blobPtr);
            }
            if (userNamePtr != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(userNamePtr);
            }
            if (targetNamePtr != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(targetNamePtr);
            }
        }
    }

    /// <summary>
    /// Deletes a generic credential from the Windows Credential Manager.
    /// </summary>
    /// <param name="name">The credential target name</param>
    /// <returns>A tuple of (success, Win32 error code). Error code is 0 on success.</returns>
    internal static (bool Success, int ErrorCode) DeleteCredential(string name)
    {
        var r = CredDeleteNative(name, CredTypeGeneric, 0);
        return (r, r ? 0 : Marshal.GetLastWin32Error());
    }
}
