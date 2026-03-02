using Nickvision.Desktop.Application;
using Nickvision.Desktop.Filesystem;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Nickvision.Desktop.Tests;

public class MockHttpClientFacotry : IHttpClientFactory
{
    private static readonly Dictionary<string, HttpClient> Clients;

    static MockHttpClientFacotry()
    {
        Clients = [];
    }

    public HttpClient CreateClient(string name)
    {
        if (Clients.TryGetValue(name, out var client))
        {
            return client;
        }
        Clients[name] = new HttpClient();
        return Clients[name];
    }
}

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
        var httpClientFactorty = 
        _updaterService = new UpdaterService(new AppInfo("org.nickvision.tubeconverter", "Nickvision Parabolic", "Parabolic")
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
        Assert.IsTrue(version > new Version("2025.10.0"));
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
        Assert.IsTrue(version > new Version("2025.7.0"));
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
        Assert.IsTrue(preview < stable);
    }

    [TestMethod]
    public async Task Case005_Ytdlp()
    {
        if (Environment.GetEnvironmentVariable("CI") == "true")
        {
            Assert.Inconclusive("Update service is not supported in CI environments");
        }
        Assert.IsNotNull(_client);
        var updateService = new UpdaterService("yt-dlp", "yt-dlp", _client);
        var stable = await updateService.GetLatestStableVersionAsync();
        Assert.IsNotNull(stable);
        Assert.IsTrue(stable >= new AppVersion("2025.12.08"));
    }

    [TestMethod]
    public async Task Check006_WindowsUpdate()
    {
        if (!global::System.OperatingSystem.IsWindows())
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
        Assert.IsTrue(version > new Version("2025.10.0"));
        Assert.IsTrue(await _updaterService.WindowsUpdate(version));
    }
}
