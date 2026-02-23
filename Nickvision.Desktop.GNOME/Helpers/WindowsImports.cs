using System.Runtime.InteropServices;

namespace Nickvision.Desktop.GNOME.Helpers;

public static partial class WindowsImports
{
    [LibraryImport("libgobject-2.0-0.dll", StringMarshalling = StringMarshalling.Utf8)]
    public static partial ulong g_signal_connect_data(nint instance, string signal, WindowExtensions.OpenCallback callback, nint data, nint destroy_data, int flags);

    [LibraryImport("libgobject-2.0-0.dll", StringMarshalling = StringMarshalling.Utf8)]
    public static partial string g_file_get_uri(nint file);
}
