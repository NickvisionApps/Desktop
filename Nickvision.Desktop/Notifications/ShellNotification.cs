namespace Nickvision.Desktop.Notifications;

/// <summary>
/// A class containing information about a shell notification.
/// </summary>
public class ShellNotification : AppNotification
{
    /// <summary>
    /// The title of the notification.
    /// </summary>
    public string Title { get; init; }

    /// <summary>
    /// Constructs a ShellNotification.
    /// </summary>
    /// <param name="title">The title of the notification</param>
    /// <param name="message">The message of the notification</param>
    /// <param name="severity">The severity of the notification</param>
    public ShellNotification(string title, string message, NotificationSeverity severity) : base(message, severity)
    {
        Title = title;
    }
}
