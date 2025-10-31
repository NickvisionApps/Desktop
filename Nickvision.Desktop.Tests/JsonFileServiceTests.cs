using Nickvision.Desktop.Application;
using Nickvision.Desktop.Filesystem;
using System.IO;
using System.Threading.Tasks;

namespace Nickvision.Desktop.Tests;

public class Config
{
    public Config()
    {
        DarkModeEnabled = false;
        WindowGeometry = new WindowGeometry();
    }

    public bool DarkModeEnabled { get; set; }
    public WindowGeometry WindowGeometry { get; set; }
}

[TestClass]
public sealed class JsonFileServiceTests
{
    private static JsonFileService? _jsonFileService;

    [TestMethod]
    public void Case001_Initalize()
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
        Assert.AreEqual(800, config.WindowGeometry.Width);
        Assert.AreEqual(600, config.WindowGeometry.Height);
        Assert.IsFalse(File.Exists("config.json"));
    }

    [TestMethod]
    public async Task Case003_LoadAsync()
    {
        Assert.IsNotNull(_jsonFileService);
        var configAsync = await _jsonFileService.LoadAsync<Config>("config-async");
        Assert.IsNotNull(configAsync);
        Assert.IsFalse(configAsync.DarkModeEnabled);
        Assert.AreEqual(800, configAsync.WindowGeometry.Width);
        Assert.AreEqual(600, configAsync.WindowGeometry.Height);
        Assert.IsFalse(File.Exists("config-async.json"));
    }

    [TestMethod]
    public void Case004_Change()
    {
        Assert.IsNotNull(_jsonFileService);
        var config = _jsonFileService.Load<Config>();
        config.DarkModeEnabled = true;
        Assert.IsTrue(config.DarkModeEnabled);
        Assert.IsTrue(_jsonFileService.Save(config));
        Assert.IsTrue(File.Exists("config.json"));
    }

    [TestMethod]
    public async Task Case005_ChangeAsync()
    {
        Assert.IsNotNull(_jsonFileService);
        var configAsync = await _jsonFileService.LoadAsync<Config>("config-async");
        configAsync.DarkModeEnabled = true;
        Assert.IsTrue(configAsync.DarkModeEnabled);
        Assert.IsTrue(await _jsonFileService.SaveAsync(configAsync, "config-async"));
        Assert.IsTrue(File.Exists("config-async.json"));
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
        if (File.Exists("config.json"))
        {
            File.Delete("config.json");
        }
        if (File.Exists("config-async.json"))
        {
            File.Delete("config-async.json");
        }
        Assert.IsFalse(File.Exists("config.json"));
        Assert.IsFalse(File.Exists("config-async.json"));
    }
}
