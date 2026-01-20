using Nickvision.Desktop.System;
using System.Threading.Tasks;

namespace Nickvision.Desktop.Tests;

[TestClass]
public class PowerServiceTests
{
    private static PowerService? _powerService;

    [TestMethod]
    public void Case001_Initialize()
    {
        _powerService = new PowerService();
        Assert.IsNotNull(_powerService);
    }

    [TestMethod]
    public async Task Case002_PreventSuspend()
    {
        Assert.IsNotNull(_powerService);
        Assert.IsTrue(await _powerService.PreventSuspendAsync());
    }

    [TestMethod]
    public async Task Case003_AllowSuspend()
    {
        Assert.IsNotNull(_powerService);
        Assert.IsTrue(await _powerService.AllowSuspendAsync());
    }

    [TestMethod]
    public void Case004_Dispose()
    {
        Assert.IsNotNull(_powerService);
        _powerService.Dispose();
        _powerService = null;
    }
}
