using Microsoft.Toolkit.Uwp.Notifications;
using Nickvision.Desktop.Application;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Nickvision.Desktop.Notifications;

/// <summary>
///     A service for managing notifications.
/// </summary>
public class NotificationService : IDisposable, INotificationService
{
    private bool _disposed;
    private readonly AppInfo _appInfo;
    private readonly string _openTranslatedText;
    private LinuxNotify.GDestroyNotify? _destroyCallback;
    private LinuxNotify.NotifyActionCallback? _openActionCallback;

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
        if (OperatingSystem.IsLinux())
        {
            _destroyCallback = Marshal.FreeHGlobal;
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
            LinuxNotify.notify_init(_appInfo.Id);
        }
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
    public void Send(AppNotification notification) => AppNotificationSent?.Invoke(this, new AppNotificationSentEventArgs(notification));

    /// <summary>
    ///     Sends a shell notification.
    /// </summary>
    /// <param name="notification">The ShellNotification to send</param>
    /// <returns>True if the shell notification was sent successfully, else false</returns>
    public bool Send(ShellNotification notification)
    {
        if (OperatingSystem.IsWindows())
        {
            var builder = new ToastContentBuilder();
            builder.AddText(notification.Title);
            builder.AddText(notification.Message);
            if (notification.Action == "open" && !string.IsNullOrEmpty(notification.ActionParam))
            {
                builder.AddButton(_openTranslatedText, ToastActivationType.Protocol, $"file://{notification.ActionParam}");
            }
#if NET_WINDOWS
            builder.Show();
#endif
            return true;
        }
        else if (OperatingSystem.IsMacOS())
        {
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
        }
        else if (OperatingSystem.IsLinux())
        {
            var notify = LinuxNotify.notify_notification_new(notification.Title, notification.Message, _appInfo.Id);
            if (notification.Action == "open" && !string.IsNullOrEmpty(notification.ActionParam) && _openActionCallback is not null && _destroyCallback is not null)
            {
                LinuxNotify.notify_notification_add_action(notify, "open", _openTranslatedText, _openActionCallback, Marshal.StringToHGlobalAnsi(notification.ActionParam), _destroyCallback);
            }
            LinuxNotify.notify_notification_set_urgency(notify,
                notification.Severity switch
                {
                    NotificationSeverity.Information => 1,
                    NotificationSeverity.Success => 1,
                    NotificationSeverity.Warning => 2,
                    NotificationSeverity.Error => 2,
                    _ => 0
                });
            var res = LinuxNotify.notify_notification_show(notify, nint.Zero);
            LinuxNotify.g_object_unref(notify);
            return res;
        }
        else
        {
            return true;
        }
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
        if (OperatingSystem.IsLinux())
        {
            LinuxNotify.notify_uninit();
        }
        _disposed = true;
    }
}