using System;

namespace Nickvision.Desktop.Notifications;

public interface INotificationService : IService
{
    event EventHandler<AppNotificationSentEventArgs>? AppNotificationSent;

    void Send(AppNotification notification);

    bool Send(ShellNotification notification);
}
