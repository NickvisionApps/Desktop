using System;

namespace Nickvision.Desktop.Notifications;

public class AppNotificationSentEventArgs : EventArgs
{
    public AppNotification Notification { get; init; }

    public DateTime Timestamp { get; init; }

    public AppNotificationSentEventArgs(AppNotification notification)
    {
        Notification = notification;
        Timestamp = DateTime.Now;
    }
}
