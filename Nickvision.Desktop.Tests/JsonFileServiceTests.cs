using Nickvision.Desktop.Application;
using Nickvision.Desktop.Filesystem;
using Nickvision.Desktop.Tests.Mocks;
using System.IO;
using System.Text.Json.Serialization;
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

[JsonSourceGenerationOptions(DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull, WriteIndented = true)]
[JsonSerializable(typeof(Config))]
internal partial class TestJsonContext : JsonSerializerContext { }

[TestClass]
public sealed class JsonFileServiceTests
{
    private static JsonFileService? _jsonFileService;

    [ClassInitialize]
    public static void ClassInitialize(TestContext context)
    {
        var configPath = Path.Combine(UserDirectories.Config, "Nickvision.Desktop Tests", "config.json");
        var configAsyncPath = Path.Combine(UserDirectories.Config, "Nickvision.Desktop Tests", "config-async.json");
        Directory.CreateDirectory(Path.Combine(UserDirectories.Config, "Nickvision.Desktop Tests"));
        foreach (var path in new[] { configPath, configAsyncPath })
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }

    [TestMethod]
    public void Case001_Initialize()
    {
        var appInfo = new AppInfo("org.nickvision.desktop.tests", "Nickvision.Desktop Tests", "Tests");
        _jsonFileService = new JsonFileService(new MockLogger<JsonFileService>(), appInfo);
        Assert.IsNotNull(_jsonFileService);
    }

    [TestMethod]
    public void Case002_Load()
    {
        Assert.IsNotNull(_jsonFileService);
        var config = _jsonFileService.Load(TestJsonContext.Default.Config);
        Assert.IsNotNull(config);
        Assert.IsFalse(config.DarkModeEnabled);
        Assert.AreEqual(900, config.WindowGeometry.Width);
        Assert.AreEqual(700, config.WindowGeometry.Height);
        Assert.IsFalse(File.Exists(Path.Combine(UserDirectories.Config, "Nickvision.Desktop Tests", "config.json")));
    }

    [TestMethod]
    public async Task Case003_LoadAsync()
    {
        Assert.IsNotNull(_jsonFileService);
        var configAsync = await _jsonFileService.LoadAsync(TestJsonContext.Default.Config, "config-async");
        Assert.IsNotNull(configAsync);
        Assert.IsFalse(configAsync.DarkModeEnabled);
        Assert.AreEqual(900, configAsync.WindowGeometry.Width);
        Assert.AreEqual(700, configAsync.WindowGeometry.Height);
        Assert.IsFalse(File.Exists(Path.Combine(UserDirectories.Config, "Nickvision.Desktop Tests", "config-async.json")));
    }

    [TestMethod]
    public void Case004_Change()
    {
        Assert.IsNotNull(_jsonFileService);
        var config = _jsonFileService.Load(TestJsonContext.Default.Config);
        config.DarkModeEnabled = true;
        Assert.IsTrue(config.DarkModeEnabled);
        Assert.IsTrue(_jsonFileService.Save(config, TestJsonContext.Default.Config));
        Assert.IsTrue(File.Exists(Path.Combine(UserDirectories.Config, "Nickvision.Desktop Tests", "config.json")));
    }

    [TestMethod]
    public async Task Case005_ChangeAsync()
    {
        Assert.IsNotNull(_jsonFileService);
        var configAsync = await _jsonFileService.LoadAsync(TestJsonContext.Default.Config, "config-async");
        configAsync.DarkModeEnabled = true;
        Assert.IsTrue(configAsync.DarkModeEnabled);
        Assert.IsTrue(await _jsonFileService.SaveAsync(configAsync, TestJsonContext.Default.Config, "config-async"));
        Assert.IsTrue(File.Exists(Path.Combine(UserDirectories.Config, "Nickvision.Desktop Tests", "config-async.json")));
    }

    [TestMethod]
    public void Case006_Verify()
    {
        Assert.IsNotNull(_jsonFileService);
        var config = _jsonFileService.Load(TestJsonContext.Default.Config);
        Assert.IsNotNull(config);
        Assert.IsTrue(config.DarkModeEnabled);
    }

    [TestMethod]
    public async Task Case007_VerifyAsync()
    {
        Assert.IsNotNull(_jsonFileService);
        var configAsync = await _jsonFileService.LoadAsync(TestJsonContext.Default.Config, "config-async");
        Assert.IsNotNull(configAsync);
        Assert.IsTrue(configAsync.DarkModeEnabled);
    }

    [TestMethod]
    public void Case008_Cleanup()
    {
        var configPath = Path.Combine(UserDirectories.Config, "Nickvision.Desktop Tests", "config.json");
        var configAsyncPath = Path.Combine(UserDirectories.Config, "Nickvision.Desktop Tests", "config-async.json");
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
        Directory.Delete(Path.Combine(UserDirectories.Config, "Nickvision.Desktop Tests"));
    }
}
