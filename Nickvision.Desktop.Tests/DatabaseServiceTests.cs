using Nickvision.Desktop.Application;
using Nickvision.Desktop.System;
using Nickvision.Desktop.Tests.Mocks;

namespace Nickvision.Desktop.Tests;

[TestClass]
public class DatabaseServiceTests
{
    private static DatabaseService? _databaseService;

    [TestMethod]
    public void Case001_Init()
    {
        _databaseService = new DatabaseService(new MockLogger<DatabaseService>(), new AppInfo("org.nickvision.desktop.test", "Nickvision.Desktop.Test", "Test"), new SecretService(new MockLogger<SecretService>()));
        Assert.IsNotNull(_databaseService);
    }
}
