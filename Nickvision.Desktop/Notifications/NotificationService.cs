using System;

namespace Nickvision.Desktop.Notifications;

public class NotificationService : INotificationService
{
    public event EventHandler<AppNotificationSentEventArgs>? AppNotificationSent;

    public void Send(AppNotification notification) => AppNotificationSent?.Invoke(this, new AppNotificationSentEventArgs(notification));
}