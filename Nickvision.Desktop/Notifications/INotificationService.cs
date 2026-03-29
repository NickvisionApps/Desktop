using System;

namespace Nickvision.Desktop.Notifications;

public interface INotificationService
{
    event EventHandler<AppNotificationSentEventArgs>? AppNotificationSent;

    void Send(AppNotification notification);
}
