namespace Nickvision.Desktop.Notifications;

/// <summary>
///     A class containing information about an application notification.
/// </summary>
public class AppNotification
{
    /// <summary>
    ///     Constructs an AppNotification.
    /// </summary>
    /// <param name="message">The message of the notification</param>
    /// <param name="severity">The severity of the notification</param>
    public AppNotification(string message, NotificationSeverity severity)
    {
        Message = message;
        Severity = severity;
        Action = null;
        ActionParam = null;
    }

    /// <summary>
    ///     The message of the notification.
    /// </summary>
    public string Message { get; init; }

    /// <summary>
    ///     The severity of the notification.
    /// </summary>
    public NotificationSeverity Severity { get; init; }

    /// <summary>
    ///     The action name of the notification.
    /// </summary>
    public string? Action { get; set; }

    /// <summary>
    ///     The action parameter of the notification.
    /// </summary>
    public string? ActionParam { get; set; }
}
