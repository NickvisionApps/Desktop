using System;

namespace Nickvision.Desktop.Notifications;

/// <summary>
/// A service for managing notifications.
/// </summary>
public class NotificationService : INotificationService
{
    /// <summary>
    /// The event for when app notifications are sent.
    /// </summary>
    public event EventHandler<AppNotificationSentEventArgs>? AppNotificationSent;

    /// <summary>
    /// Sends an app notification.
    /// </summary>
    /// <param name="notification">The AppNotification to send</param>
    public void Send(AppNotification notification) => AppNotificationSent?.Invoke(this, new AppNotificationSentEventArgs(notification));
}