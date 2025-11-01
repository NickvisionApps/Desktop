using System;

namespace Nickvision.Desktop.Notifications;

/// <summary>
/// An interface for a service for managing notifications.
/// </summary>
public interface INotificationService : IService
{
    /// <summary>
    /// The event for when app notifications are sent.
    /// </summary>
    event EventHandler<AppNotificationSentEventArgs>? AppNotificationSent;

    /// <summary>
    /// Sends an app notification.
    /// </summary>
    /// <param name="notification">The AppNotification to send</param>
    void Send(AppNotification notification);

    /// <summary>
    /// Sends a shell notification.
    /// </summary>
    /// <param name="notification">The ShellNotification to send</param>
    /// <returns>True if the shell notification was sent successfully, else false</returns>
    bool Send(ShellNotification notification);
}
