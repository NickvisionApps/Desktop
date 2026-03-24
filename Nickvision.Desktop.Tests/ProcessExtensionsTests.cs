using Nickvision.Desktop.Helpers;
using System.Diagnostics;

namespace Nickvision.Desktop.Tests;

[TestClass]
public sealed class ProcessExtensionsTests
{
    private static Process? _process;

    [TestMethod]
    public void Case001_Start()
    {
        var startInfo = new ProcessStartInfo
        {
            CreateNoWindow = true,
            UseShellExecute = false
        };
        if (global::System.OperatingSystem.IsWindows())
        {
            startInfo.FileName = "ping";
            startInfo.Arguments = "-n 100 127.0.0.1";
        }
        else
        {
            startInfo.FileName = "sleep";
            startInfo.Arguments = "100";
        }
        _process = Process.Start(startInfo);
        Assert.IsNotNull(_process);
    }

    [TestMethod]
    public void Case002_SetAsParentProcess()
    {
        Assert.IsNotNull(_process);
        _process.SetAsParentProcess();
    }

    [TestMethod]
    public void Case003_Suspend()
    {
        Assert.IsNotNull(_process);
        _process.Suspend();
    }

    [TestMethod]
    public void Case004_Resume()
    {
        Assert.IsNotNull(_process);
        _process.Resume();
    }

    [TestMethod]
    public void Case005_Kill()
    {
        Assert.IsNotNull(_process);
        _process.Kill(entireProcessTree: true);
        _process.Dispose();
        _process = null;
    }
}
