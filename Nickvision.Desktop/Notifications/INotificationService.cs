using System;

namespace Nickvision.Desktop.Notifications;

/// <summary>
/// An interface for a service for managing notifications.
/// </summary>
public interface INotificationService
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
}
