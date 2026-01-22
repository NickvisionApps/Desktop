using System.Runtime.InteropServices;

namespace Nickvision.Desktop.Notifications;

/// <summary>
///     P/Invoke wrapper for libnotify on Linux.
/// </summary>
internal static partial class LinuxNotify
{
    internal delegate void GDestroyNotify(nint data);

    internal delegate void NotifyActionCallback(nint notification, [MarshalAs(UnmanagedType.LPStr)] string action, nint user_data);

    [LibraryImport("libnotify.so.4")]
    internal static partial void g_object_unref(nint data);

    [LibraryImport("libnotify.so.4")]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static partial bool notify_init([MarshalAs(UnmanagedType.LPStr)] string appName);

    [LibraryImport("libnotify.so.4")]
    internal static partial void notify_uninit();

    [LibraryImport("libnotify.so.4")]
    internal static partial nint notify_notification_new([MarshalAs(UnmanagedType.LPStr)] string summary, [MarshalAs(UnmanagedType.LPStr)] string body, [MarshalAs(UnmanagedType.LPStr)] string icon);

    [LibraryImport("libnotify.so.4")]
    internal static unsafe partial void notify_notification_add_action(nint notification, [MarshalAs(UnmanagedType.LPStr)] string action, [MarshalAs(UnmanagedType.LPStr)] string label, NotifyActionCallback callback, nint user_data, GDestroyNotify free_func);

    [LibraryImport("libnotify.so.4")]
    internal static partial void notify_notification_set_urgency(nint notification, uint urgency);

    [LibraryImport("libnotify.so.4")]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static partial bool notify_notification_show(nint notification, nint error);
}
