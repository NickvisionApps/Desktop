using Nickvision.Desktop.Application;
using Nickvision.Desktop.Filesystem;
using Nickvision.Desktop.Notifications;

namespace Nickvision.Desktop.Tests;

[TestClass]
public class NotificationServiceTests
{
    private static NotificationService? _notificationService;

    [TestMethod]
    public void Case001_Initalize()
    {
        _notificationService = new NotificationService(
            new AppInfo("org.nickvision.desktop.tests", "Nickvision Desktop Tests", "Desktop Tests"),
            "Open");
        Assert.IsNotNull(_notificationService);
    }

    [TestMethod]
    public void Case002_SendShell()
    {
        Assert.IsNotNull(_notificationService);
        Assert.IsTrue(
            _notificationService.Send(
                new ShellNotification(
                    "Test Notification", 
                    "This is a test notification body.",
                    NotificationSeverity.Information)
                {
                    Action = "open",
                    ActionParam = UserDirectories.Home
                }));
    }

    [TestMethod]
    public void Case003_Cleanup()
    {
        Assert.IsNotNull(_notificationService);
        _notificationService.Dispose();
        _notificationService = null;
    }
}
