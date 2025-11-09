using Nickvision.Desktop.Application;
using Nickvision.Desktop.Filesystem;
using System.IO;
using System.Threading.Tasks;

namespace Nickvision.Desktop.Tests;

public class Config
{
    public bool DarkModeEnabled { get; set; }
    public WindowGeometry WindowGeometry { get; set; }

    public Config()
    {
        DarkModeEnabled = false;
        WindowGeometry = new WindowGeometry();
    }
}

[TestClass]
public sealed class JsonFileServiceTests
{
    private static JsonFileService? _jsonFileService;

    [ClassInitialize]
    public static void ClassInitialize(TestContext context)
    {
        var configPath = Path.Combine(Path.Combine(Directory.GetCurrentDirectory(), "config.json"));
        var configAsyncPath = Path.Combine(Path.Combine(Directory.GetCurrentDirectory(), "config-async.json"));
        if (File.Exists(configPath))
        {
            File.Delete(configPath);
        }
        if (File.Exists(configAsyncPath))
        {
            File.Delete(configAsyncPath);
        }
    }

    [TestMethod]
    public void Case001_Initialize()
    {
        _jsonFileService = new JsonFileService(Directory.GetCurrentDirectory());
        Assert.IsNotNull(_jsonFileService);
    }

    [TestMethod]
    public void Case002_Load()
    {
        Assert.IsNotNull(_jsonFileService);
        var config = _jsonFileService.Load<Config>();
        Assert.IsNotNull(config);
        Assert.IsFalse(config.DarkModeEnabled);
        Assert.AreEqual(900, config.WindowGeometry.Width);
        Assert.AreEqual(700, config.WindowGeometry.Height);
        Assert.IsFalse(File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "config.json")));
    }

    [TestMethod]
    public async Task Case003_LoadAsync()
    {
        Assert.IsNotNull(_jsonFileService);
        var configAsync = await _jsonFileService.LoadAsync<Config>("config-async");
        Assert.IsNotNull(configAsync);
        Assert.IsFalse(configAsync.DarkModeEnabled);
        Assert.AreEqual(900, configAsync.WindowGeometry.Width);
        Assert.AreEqual(700, configAsync.WindowGeometry.Height);
        Assert.IsFalse(File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "config-async.json")));
    }

    [TestMethod]
    public void Case004_Change()
    {
        Assert.IsNotNull(_jsonFileService);
        var config = _jsonFileService.Load<Config>();
        config.DarkModeEnabled = true;
        Assert.IsTrue(config.DarkModeEnabled);
        Assert.IsTrue(_jsonFileService.Save(config));
        Assert.IsTrue(File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "config.json")));
    }

    [TestMethod]
    public async Task Case005_ChangeAsync()
    {
        Assert.IsNotNull(_jsonFileService);
        var configAsync = await _jsonFileService.LoadAsync<Config>("config-async");
        configAsync.DarkModeEnabled = true;
        Assert.IsTrue(configAsync.DarkModeEnabled);
        Assert.IsTrue(await _jsonFileService.SaveAsync(configAsync, "config-async"));
        Assert.IsTrue(File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "config-async.json")));
    }

    [TestMethod]
    public void Case006_Verify()
    {
        Assert.IsNotNull(_jsonFileService);
        var config = _jsonFileService.Load<Config>();
        Assert.IsNotNull(config);
        Assert.IsTrue(config.DarkModeEnabled);
    }

    [TestMethod]
    public async Task Case007_VerifyAsync()
    {
        Assert.IsNotNull(_jsonFileService);
        var configAsync = await _jsonFileService.LoadAsync<Config>("config-async");
        Assert.IsNotNull(configAsync);
        Assert.IsTrue(configAsync.DarkModeEnabled);
    }

    [TestMethod]
    public void Case008_Cleanup()
    {
        var configPath = Path.Combine(Path.Combine(Directory.GetCurrentDirectory(), "config.json"));
        var configAsyncPath = Path.Combine(Path.Combine(Directory.GetCurrentDirectory(), "config-async.json"));
        if (File.Exists(configPath))
        {
            File.Delete(configPath);
        }
        if (File.Exists(configAsyncPath))
        {
            File.Delete(configAsyncPath);
        }
        Assert.IsFalse(File.Exists(configPath));
        Assert.IsFalse(File.Exists(configAsyncPath));
    }
}
