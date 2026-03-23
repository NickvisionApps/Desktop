using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tmds.DBus.Protocol;

namespace Nickvision.Desktop.FreeDesktop;

/// <summary>
/// Internal proxy for the org.freedesktop.Notifications D-Bus interface.
/// </summary>
internal static class NotificationsProxy
{
    private const string Service = "org.freedesktop.Notifications";
    private const string Path = "/org/freedesktop/Notifications";
    private const string Interface = "org.freedesktop.Notifications";

    /// <summary>
    /// Sends a desktop notification.
    /// </summary>
    /// <param name="connection">The D-Bus connection</param>
    /// <param name="appName">The application name</param>
    /// <param name="replacesId">The notification ID to replace, or 0</param>
    /// <param name="appIcon">The application icon name</param>
    /// <param name="summary">The notification summary</param>
    /// <param name="body">The notification body</param>
    /// <param name="actions">The list of actions (alternating id and label pairs)</param>
    /// <param name="hints">Additional notification hints</param>
    /// <param name="expireTimeout">Expiry timeout in milliseconds, -1 for default</param>
    /// <returns>The notification ID assigned by the server</returns>
    internal static async Task<uint> NotifyAsync(DBusConnection connection, string appName, uint replacesId,
        string appIcon, string summary, string body, string[] actions,
        Dictionary<string, VariantValue> hints, int expireTimeout)
    {
        MessageBuffer buffer;
        {
            using var writer = connection.GetMessageWriter();
            writer.WriteMethodCallHeader(Service, Path, Interface, "Notify", "susssasa{sv}i", MessageFlags.None);
            writer.WriteString(appName);
            writer.WriteUInt32(replacesId);
            writer.WriteString(appIcon);
            writer.WriteString(summary);
            writer.WriteString(body);
            writer.WriteArray(actions);
            writer.WriteDictionary(hints);
            writer.WriteInt32(expireTimeout);
            buffer = writer.CreateMessage();
        }
        return await connection.CallMethodAsync(buffer, static (Message m, object? _) =>
        {
            var reader = m.GetBodyReader();
            return reader.ReadUInt32();
        }, null);
    }

    /// <summary>
    /// Watches for the NotificationClosed signal.
    /// </summary>
    /// <param name="connection">The D-Bus connection</param>
    /// <param name="handler">Handler invoked with (exception, (id, reason))</param>
    /// <returns>A disposable that unsubscribes the watcher</returns>
    internal static async Task<IDisposable> WatchNotificationClosedAsync(
        DBusConnection connection,
        Action<Exception?, (uint id, uint reason)> handler)
    {
        return await connection.WatchSignalAsync<(uint id, uint reason)>(
            Service, Interface, Path, "NotificationClosed",
            static (Message m, object? _) =>
            {
                var reader = m.GetBodyReader();
                return (reader.ReadUInt32(), reader.ReadUInt32());
            },
            handler, null, false, ObserverFlags.None);
    }

    /// <summary>
    /// Watches for the ActionInvoked signal.
    /// </summary>
    /// <param name="connection">The D-Bus connection</param>
    /// <param name="handler">Handler invoked with (exception, (id, actionKey))</param>
    /// <returns>A disposable that unsubscribes the watcher</returns>
    internal static async Task<IDisposable> WatchActionInvokedAsync(
        DBusConnection connection,
        Action<Exception?, (uint id, string actionKey)> handler)
    {
        return await connection.WatchSignalAsync<(uint id, string actionKey)>(
            Service, Interface, Path, "ActionInvoked",
            static (Message m, object? _) =>
            {
                var reader = m.GetBodyReader();
                return (reader.ReadUInt32(), reader.ReadString());
            },
            handler, null, false, ObserverFlags.None);
    }
}
