using System.Runtime.InteropServices;

namespace Nickvision.Desktop.GNOME.Helpers;

public static partial class MacOSImports
{
    [LibraryImport("libgobject-2.0.0.dylib", StringMarshalling = StringMarshalling.Utf8)]
    public static partial ulong g_signal_connect_data(nint instance, string signal, WindowExtensions.OpenCallback callback, nint data, nint destroy_data, int flags);

    [LibraryImport("libgio-2.0.0.dylib", StringMarshalling = StringMarshalling.Utf8)]
    public static partial string g_file_get_uri(nint file);
}
