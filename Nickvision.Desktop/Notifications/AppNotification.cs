namespace Nickvision.Desktop.Notifications;

public class AppNotification
{
    public string? Action { get; set; }
    public string? ActionParam { get; set; }
    public string Message { get; init; }
    public NotificationSeverity Severity { get; init; }

    public AppNotification(string message, NotificationSeverity severity)
    {
        Message = message;
        Severity = severity;
        Action = null;
        ActionParam = null;
    }
}
