using System;

namespace Nickvision.Desktop.Notifications;

/// <summary>
/// A class of event arguments for when an app notification is sent.
/// </summary>
public class AppNotificationSentEventArgs : EventArgs
{
    /// <summary>
    /// The AppNotification sent.
    /// </summary>
    public AppNotification Notification { get; init; }

    /// <summary>
    /// The timestamp of when the notification was sent.
    /// </summary>
    public DateTime Timestamp { get; init; }

    /// <summary>
    /// Constructs an AppNotificationSentEventArgs.
    /// </summary>
    /// <param name="notification">The AppNotification sent</param>
    public AppNotificationSentEventArgs(AppNotification notification)
    {
        Notification = notification;
        Timestamp = DateTime.Now;
    }
}
