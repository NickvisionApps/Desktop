using System.Runtime.InteropServices;

namespace Nickvision.Desktop.Helpers;

/// <summary>
/// NativeAOT-compatible P/Invoke helpers for Windows Credential Manager (advapi32).
/// </summary>
internal static partial class WindowsSecretHelpers
{
    internal const uint CredTypeGeneric = 1;         // CRED_TYPE_GENERIC
    internal const uint CredPersistLocalMachine = 2; // CRED_PERSIST_LOCAL_MACHINE

    /// <summary>
    /// A NativeAOT-compatible representation of the Windows CREDENTIAL structure.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct CREDENTIAL_WIN32
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
    internal static partial bool CredReadNative(string target, uint type, uint flags, out nint credential);

    [LibraryImport("advapi32.dll", EntryPoint = "CredWriteW", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool CredWriteNative(nint credential, uint flags);

    [LibraryImport("advapi32.dll", EntryPoint = "CredDeleteW", StringMarshalling = StringMarshalling.Utf16, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool CredDeleteNative(string target, uint type, uint flags);

    [LibraryImport("advapi32.dll", EntryPoint = "CredFree")]
    internal static partial void CredFreeNative(nint buffer);
}
