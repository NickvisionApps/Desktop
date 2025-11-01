using Nickvision.Desktop.Application;
using System;
#if OS_WINDOWS
using Microsoft.Toolkit.Uwp.Notifications;

#elif OS_MAC
using System.Diagnostics;

#elif OS_LINUX
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
#endif

namespace Nickvision.Desktop.Notifications;

/// <summary>
///     A service for managing notifications.
/// </summary>
#if OS_LINUX
public partial class NotificationService : IDisposable, INotificationService
#else
public class NotificationService : IDisposable, INotificationService
#endif
{
#if OS_LINUX
    private delegate void GDestroyNotify(nint data);

    private delegate void NotifyActionCallback(nint notification, [MarshalAs(UnmanagedType.LPStr)] string action, nint user_data);

    [LibraryImport("libnotify.so.4")]
    private static partial void g_object_unref(nint data);

    [LibraryImport("libnotify.so.4")]
    [return: MarshalAs(UnmanagedType.I1)]
    private static partial bool notify_init([MarshalAs(UnmanagedType.LPStr)] string appName);

    [LibraryImport("libnotify.so.4")]
    private static partial void notify_uninit();

    [LibraryImport("libnotify.so.4")]
    public static partial nint notify_notification_new([MarshalAs(UnmanagedType.LPStr)] string summary, [MarshalAs(UnmanagedType.LPStr)] string body, [MarshalAs(UnmanagedType.LPStr)] string icon);

    [LibraryImport("libnotify.so.4")]
    private static unsafe partial void notify_notification_add_action(nint notification,
        [MarshalAs(UnmanagedType.LPStr)] string action,
        [MarshalAs(UnmanagedType.LPStr)] string label,
        NotifyActionCallback callback,
        nint user_data,
        GDestroyNotify free_func);

    [LibraryImport("libnotify.so.4")]
    private static partial void notify_notification_set_urgency(nint notification, uint urgency);

    [LibraryImport("libnotify.so.4")]
    [return: MarshalAs(UnmanagedType.I1)]
    private static partial bool notify_notification_show(nint notification, nint error);
#endif

    private bool _disposed;
    private readonly AppInfo _appInfo;
    private readonly string _openTranslatedText;
#if OS_LINUX
    private readonly GDestroyNotify _destroyCallback;
    private readonly NotifyActionCallback _openActionCallback;
#endif

    /// <summary>
    ///     The event for when app notifications are sent.
    /// </summary>
    public event EventHandler<AppNotificationSentEventArgs>? AppNotificationSent;

    /// <summary>
    ///     Constructs a NotificationService.
    /// </summary>
    /// <param name="appInfo">The AppInfo object for the app</param>
    /// <param name="openTranslatedText">The text "Open" translated</param>
    public NotificationService(AppInfo appInfo, string openTranslatedText)
    {
        _disposed = false;
        _appInfo = appInfo;
        _openTranslatedText = openTranslatedText;
#if OS_LINUX
        _destroyCallback = (nint data) => Marshal.FreeHGlobal(data);
        _openActionCallback = (nint notification, string action, nint user_data) =>
        {
            var parameter = Marshal.PtrToStringAnsi(user_data);
            using var process = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = "xdg-open",
                    Arguments = parameter,
                    UseShellExecute = true
                }
            };
            process.Start();
        };
        notify_init(_appInfo.Id);
#endif
    }

    /// <summary>
    ///     Finalizes a NotificationService.
    /// </summary>
    ~NotificationService()
    {
        Dispose(false);
    }

    /// <summary>
    ///     Disposes a NotificationService.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///     Sends an app notification.
    /// </summary>
    /// <param name="notification">The AppNotification to send</param>
    public void Send(AppNotification notification)
    {
        AppNotificationSent?.Invoke(this, new AppNotificationSentEventArgs(notification));
    }

    /// <summary>
    ///     Sends a shell notification.
    /// </summary>
    /// <param name="notification">The ShellNotification to send</param>
    /// <returns>True if the shell notification was sent successfully, else false</returns>
    public bool Send(ShellNotification notification)
    {
#if OS_WINDOWS
        var builder = new ToastContentBuilder();
        builder.AddText(notification.Title);
        builder.AddText(notification.Message);
        if (notification.Action == "open" &&
            !string.IsNullOrEmpty(notification.ActionParam))
        {
            builder.AddButton(_openTranslatedText, ToastActivationType.Protocol, $"file://{notification.ActionParam}");
        }
        builder.Show();
        return true;
#elif OS_MAC
        using var process = new Process()
        {
            StartInfo = new ProcessStartInfo()
            {
                FileName = "osascript",
                Arguments = $"-e 'display notification \"{notification.Message}\" with title \"{notification.Title}\" subtitle \"{_appInfo.EnglishShortName}\"'",
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };
        process.Start();
        return true;
#elif OS_LINUX
        var notify = notify_notification_new(notification.Title, notification.Message, _appInfo.Id);
        if (notification.Action == "open" &&
            !string.IsNullOrEmpty(notification.ActionParam))
        {
            notify_notification_add_action(notify, "open", _openTranslatedText, _openActionCallback, Marshal.StringToHGlobalAnsi(notification.ActionParam), _destroyCallback);
        }
        notify_notification_set_urgency(notify,
            notification.Severity switch
            {
                NotificationSeverity.Information => 1,
                NotificationSeverity.Success => 1,
                NotificationSeverity.Warning => 2,
                NotificationSeverity.Error => 2,
                _ => 0
            });
        var res = notify_notification_show(notify, nint.Zero);
        g_object_unref(notify);
        return res;
#else
        AppNotificationSent?.Invoke(this, new AppNotificationSentEventArgs(args));
        return true;
#endif
    }

    /// <summary>
    ///     Disposes a NotificationService.
    /// </summary>
    /// <param name="disposing">Whether to dispose managed resources</param>
    private void Dispose(bool disposing)
    {
        if (_disposed || !disposing)
        {
            return;
        }
#if OS_LINUX
        notify_uninit();
#endif
        _disposed = true;
    }
}
