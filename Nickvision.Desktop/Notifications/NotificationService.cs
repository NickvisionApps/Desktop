using Microsoft.Toolkit.Uwp.Notifications;
using Nickvision.Desktop.Application;
using Nickvision.Desktop.FreeDesktop;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Tmds.DBus;

namespace Nickvision.Desktop.Notifications;

/// <summary>
/// A service for managing notifications.
/// </summary>
public class NotificationService : IDisposable, INotificationService
{
    private bool _disposed;
    private readonly AppInfo _appInfo;
    private readonly string _openTranslatedText;
    private Connection? _dbus;
    private INotifications? _freeDesktopNotifications;
    private Dictionary<uint, IDisposable> _watchers;

    /// <summary>
    /// The event for when app notifications are sent.
    /// </summary>
    public event EventHandler<AppNotificationSentEventArgs>? AppNotificationSent;

    /// <summary>
    /// Constructs a NotificationService.
    /// </summary>
    /// <param name="appInfo">The AppInfo object for the app</param>
    /// <param name="openTranslatedText">The text "Open" translated</param>
    public NotificationService(AppInfo appInfo, string openTranslatedText)
    {
        _disposed = false;
        _appInfo = appInfo;
        _openTranslatedText = openTranslatedText;
        _watchers = new Dictionary<uint, IDisposable>();
    }

    /// <summary>
    /// Finalizes a NotificationService.
    /// </summary>
    ~NotificationService()
    {
        Dispose(false);
    }

    /// <summary>
    /// Disposes a NotificationService.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Sends an app notification.
    /// </summary>
    /// <param name="notification">The AppNotification to send</param>
    public void Send(AppNotification notification) => AppNotificationSent?.Invoke(this, new AppNotificationSentEventArgs(notification));

    /// <summary>
    /// Sends a shell notification.
    /// </summary>
    /// <param name="notification">The ShellNotification to send</param>
    /// <returns>True if the shell notification was sent successfully, else false</returns>
    public async Task<bool> SendAsync(ShellNotification notification)
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
            if (_dbus is null)
            {
                _dbus = new Connection(Address.Session);
                await _dbus.ConnectAsync();
            }
            if (_freeDesktopNotifications is null)
            {
                try
                {
                    _freeDesktopNotifications = _dbus.CreateProxy<INotifications>("org.freedesktop.Notifications", "/org/freedesktop/Notifications");
                    await _freeDesktopNotifications.WatchNotificationClosedAsync(((uint id, uint reason) e) =>
                    {
                        if (_watchers.TryGetValue(e.id, out var watcher))
                        {
                            watcher.Dispose();
                            _watchers.Remove(e.id);
                        }
                    });
                }
                catch
                {
                    return false;
                }
            }
            try
            {
                var actionWatcher = await _freeDesktopNotifications.WatchActionInvokedAsync(((uint id, string actionKey) e) =>
                {
                    if (e.actionKey == "open")
                    {
                        Process.Start(new ProcessStartInfo()
                        {
                            FileName = "xdg-open",
                            Arguments = notification.ActionParam,
                            UseShellExecute = true
                        });
                    }
                });
                var id = await _freeDesktopNotifications.NotifyAsync(_appInfo.Id, 0, _appInfo.Id, notification.Title, notification.Message, notification.Action == "open" && !string.IsNullOrEmpty(notification.ActionParam) ? ["open", _openTranslatedText] : [], new Dictionary<string, object>(), -1);
                _watchers[id] = actionWatcher;
                return id > 0;
            }
            catch
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Disposes a NotificationService.
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
            _dbus?.Dispose();
            _dbus = null;
            _freeDesktopNotifications = null;
        }
        _disposed = true;
    }
}