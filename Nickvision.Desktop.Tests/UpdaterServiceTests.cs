using Nickvision.Desktop.Application;
using Nickvision.Desktop.Filesystem;
using Nickvision.Desktop.Tests.Mocks;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Nickvision.Desktop.Tests;

[TestClass]
public class UpdaterServiceTests
{
    private static HttpClient? _client;
    private static UpdaterService? _updaterService;

    [ClassInitialize]
    public static void ClassInitialize(TestContext context)
    {
        if (Environment.GetEnvironmentVariable("CI") == "true")
        {
            return;
        }
        _client = new HttpClient();
        var path = Path.Combine(UserDirectories.Cache, "NickvisionApps_Parabolic_Setup.exe");
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }

    [ClassCleanup]
    public static void ClassCleanup() => _client?.Dispose();

    [TestMethod]
    public void Case001_Initialize()
    {
        if (Environment.GetEnvironmentVariable("CI") == "true")
        {
            Assert.Inconclusive("Update service is not supported in CI environments");
        }
        Assert.IsNotNull(_client);
        _updaterService = new UpdaterService(new MockLogger<UpdaterService>(), new AppInfo("org.nickvision.tubeconverter", "Nickvision Parabolic", "Parabolic")
        {
            SourceRepository = new Uri("https://github.com/NickvisionApps/Parabolic")
        }, new MockHttpClientFacotry());
        Assert.IsNotNull(_updaterService);
    }

    [TestMethod]
    public async Task Case002_CheckForStableUpdates()
    {
        if (Environment.GetEnvironmentVariable("CI") == "true")
        {
            Assert.Inconclusive("Update service is not supported in CI environments");
        }
        Assert.IsNotNull(_updaterService);
        var version = await _updaterService.GetLatestStableVersionAsync();
        Assert.IsNotNull(version);
        Assert.IsTrue(version >= new Version("2026.2.4"));
    }

    [TestMethod]
    public async Task Case003_CheckForPreviewUpdates()
    {
        if (Environment.GetEnvironmentVariable("CI") == "true")
        {
            Assert.Inconclusive("Update service is not supported in CI environments");
        }
        Assert.IsNotNull(_updaterService);
        var version = await _updaterService.GetLatestPreviewVersionAsync();
        Assert.IsNotNull(version);
        Assert.IsTrue(version >= new Version("2026.2.4"));
    }

    [TestMethod]
    public async Task Case004_CompareVersions()
    {
        if (Environment.GetEnvironmentVariable("CI") == "true")
        {
            Assert.Inconclusive("Update service is not supported in CI environments");
        }
        Assert.IsNotNull(_updaterService);
        var preview = await _updaterService.GetLatestPreviewVersionAsync();
        var stable = await _updaterService.GetLatestStableVersionAsync();
        Assert.IsNotNull(preview);
        Assert.IsNotNull(stable);
    }

    [TestMethod]
    public async Task Case005_Ytdlp()
    {
        if (Environment.GetEnvironmentVariable("CI") == "true")
        {
            Assert.Inconclusive("Update service is not supported in CI environments");
        }
        Assert.IsNotNull(_client);
        var updateService = new UpdaterService(new MockLogger<UpdaterService>(), "yt-dlp", "yt-dlp", _client);
        var stable = await updateService.GetLatestStableVersionAsync();
        Assert.IsNotNull(stable);
        Assert.IsTrue(stable >= new AppVersion("2022.03.03"));
    }

    [TestMethod]
    public async Task Check006_WindowsUpdate()
    {
        if (!OperatingSystem.IsWindows())
        {
            Assert.Inconclusive("This test only runs on Windows");
        }
        if (Environment.GetEnvironmentVariable("CI") == "true")
        {
            Assert.Inconclusive("Update service is not supported in CI environments");
        }
        Assert.IsNotNull(_updaterService);
        var version = await _updaterService.GetLatestStableVersionAsync();
        Assert.IsNotNull(version);
        Assert.IsTrue(version >= new Version("2026.2.4"));
        Assert.IsTrue(await _updaterService.WindowsApplicationUpdateAsync(version));
    }
}
