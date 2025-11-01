using Nickvision.Desktop.Application;
using Nickvision.Desktop.Filesystem;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Nickvision.Desktop.Tests;

[TestClass]
public class GitHubUpdaterServiceTests
{
    private static HttpClient? _client;
    private static GitHubUpdaterService? _updaterService;

    [ClassInitialize]
    public static void ClassInitialize(TestContext context)
    {
        _client = new HttpClient();
        var path = Path.Combine(UserDirectories.Cache, "NickvisionApps_Parabolic_Setup.exe");
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }

    [ClassCleanup]
    public static void ClassCleanup()
    {
        _client?.Dispose();
    }

    [TestMethod]
    public void Case001_Initialize()
    {
        Assert.IsNotNull(_client);
        _updaterService = new GitHubUpdaterService(new AppInfo("org.nickvision.tubeconverter", "Nickvision Parabolic", "Parabolic")
            {
                SourceRepository = new Uri("https://github.com/NickvisionApps/Parabolic")
            },
            _client);
        Assert.IsNotNull(_updaterService);
    }

    [TestMethod]
    public async Task Case002_CheckForStableUpdates()
    {
        Assert.IsNotNull(_updaterService);
        var version = await _updaterService.GetLatestStableVersionAsync();
        Assert.IsNotNull(version);
        Assert.IsTrue(version > new Version("2025.10.0"));
    }

    [TestMethod]
    public async Task Case003_CheckForPreviewUpdates()
    {
        Assert.IsNotNull(_updaterService);
        var version = await _updaterService.GetLatestPreviewVersionAsync();
        Assert.IsNotNull(version);
        Assert.IsTrue(version > new Version("2025.7.0"));
    }

    [TestMethod]
    public async Task Case004_CompareVersions()
    {
        Assert.IsNotNull(_updaterService);
        var preview = await _updaterService.GetLatestPreviewVersionAsync();
        var stable = await _updaterService.GetLatestStableVersionAsync();
        Assert.IsNotNull(preview);
        Assert.IsNotNull(stable);
        Assert.IsTrue(preview < stable);
    }

#if OS_WINDOWS
    [TestMethod]
    public async Task Check005_WindowsUpdate()
    {
        Assert.IsNotNull(_updaterService);
        var version = await _updaterService.GetLatestStableVersionAsync();
        Assert.IsNotNull(version);
        Assert.IsTrue(version > new Version("2025.10.0"));
        Assert.IsTrue(await _updaterService.WindowsUpdate(version));
    }
#endif
}
