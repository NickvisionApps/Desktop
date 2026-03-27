using Nickvision.Desktop.Application;
using Nickvision.Desktop.System;
using Nickvision.Desktop.Tests.Mocks;

namespace Nickvision.Desktop.Tests;

[TestClass]
public class ConfigurationServiceTests
{
    private static DatabaseService? _databaseService;
    private static ConfigurationService? _configurationService;

    [TestMethod]
    public void Case001_Init()
    {
        _databaseService = new DatabaseService(new MockLogger<DatabaseService>(), new AppInfo("org.nickvision.desktop.test", "Nickvision.Desktop.Test", "Test"), new SecretService(new MockLogger<SecretService>()));
        _configurationService = new ConfigurationService(new MockLogger<ConfigurationService>(), _databaseService);
        Assert.IsNotNull(_databaseService);
        Assert.IsNotNull(_configurationService);
    }
}
