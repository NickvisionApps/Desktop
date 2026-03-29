using Nickvision.Desktop.Filesystem;
using Nickvision.Desktop.Notifications;
using System.Threading.Tasks;

namespace Nickvision.Desktop.Tests;

[TestClass]
public class NotificationServiceTests
{
    private static NotificationService? _notificationService;

    [TestMethod]
    public void Case001_Initialize()
    {
        _notificationService = new NotificationService();
        Assert.IsNotNull(_notificationService);
    }

    [TestMethod]
    public async Task Case002_SendShell()
    {
        Assert.IsNotNull(_notificationService);
        _notificationService.Send(new ShellNotification("Test Notification", "This is a test notification body.", NotificationSeverity.Information)
        {
            Action = "open",
            ActionParam = UserDirectories.Home
        });
    }
}
