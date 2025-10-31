using Nickvision.Desktop.System;

namespace Nickvision.Desktop.Tests;

[TestClass]
public class PowerServiceTests
{
    private static PowerService? _powerService;

    [TestMethod]
    public void Case001_Initalize()
    {
        _powerService = new PowerService();
        Assert.IsNotNull(_powerService);
    }

    [TestMethod]
    public void Case002_PreventSuspend()
    {
        Assert.IsNotNull(_powerService);
        Assert.IsTrue(_powerService.PreventSuspend());
    }

    [TestMethod]
    public void Case003_AllowSuspend()
    {
        Assert.IsNotNull(_powerService);
        Assert.IsTrue(_powerService.AllowSuspend());
    }

    [TestMethod]
    public void Case004_Dispose()
    {
        Assert.IsNotNull(_powerService);
        _powerService.Dispose();
        _powerService = null;
    }
}
