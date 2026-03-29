namespace Nickvision.Desktop.Notifications;

public class ShellNotification : AppNotification
{
    public string Title { get; init; }

    public ShellNotification(string title, string message, NotificationSeverity severity) : base(message, severity)
    {
        Title = title;
    }
}
