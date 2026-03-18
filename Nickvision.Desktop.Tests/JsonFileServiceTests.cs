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

[JsonSerializable(typeof(Config))]
[JsonSerializable(typeof(WindowGeometry))]
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
        var configSourceGenPath = Path.Combine(UserDirectories.Config, "Nickvision.Desktop Tests", "config-sourcegen.json");
        var configSourceGenAsyncPath = Path.Combine(UserDirectories.Config, "Nickvision.Desktop Tests", "config-sourcegen-async.json");
        Directory.CreateDirectory(Path.Combine(UserDirectories.Config, "Nickvision.Desktop Tests"));
        foreach (var path in new[] { configPath, configAsyncPath, configSourceGenPath, configSourceGenAsyncPath })
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
        var config = _jsonFileService.Load<Config>();
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
        var configAsync = await _jsonFileService.LoadAsync<Config>("config-async");
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
        var config = _jsonFileService.Load<Config>();
        config.DarkModeEnabled = true;
        Assert.IsTrue(config.DarkModeEnabled);
        Assert.IsTrue(_jsonFileService.Save(config));
        Assert.IsTrue(File.Exists(Path.Combine(UserDirectories.Config, "Nickvision.Desktop Tests", "config.json")));
    }

    [TestMethod]
    public async Task Case005_ChangeAsync()
    {
        Assert.IsNotNull(_jsonFileService);
        var configAsync = await _jsonFileService.LoadAsync<Config>("config-async");
        configAsync.DarkModeEnabled = true;
        Assert.IsTrue(configAsync.DarkModeEnabled);
        Assert.IsTrue(await _jsonFileService.SaveAsync(configAsync, "config-async"));
        Assert.IsTrue(File.Exists(Path.Combine(UserDirectories.Config, "Nickvision.Desktop Tests", "config-async.json")));
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
    }

    [TestMethod]
    public void Case009_LoadWithSourceGen()
    {
        Assert.IsNotNull(_jsonFileService);
        var config = _jsonFileService.Load(TestJsonContext.Default.Config, "config-sourcegen");
        Assert.IsNotNull(config);
        Assert.IsFalse(config.DarkModeEnabled);
        Assert.AreEqual(900, config.WindowGeometry.Width);
        Assert.AreEqual(700, config.WindowGeometry.Height);
        Assert.IsFalse(File.Exists(Path.Combine(UserDirectories.Config, "Nickvision.Desktop Tests", "config-sourcegen.json")));
    }

    [TestMethod]
    public async Task Case010_LoadAsyncWithSourceGen()
    {
        Assert.IsNotNull(_jsonFileService);
        var config = await _jsonFileService.LoadAsync(TestJsonContext.Default.Config, "config-sourcegen-async");
        Assert.IsNotNull(config);
        Assert.IsFalse(config.DarkModeEnabled);
        Assert.AreEqual(900, config.WindowGeometry.Width);
        Assert.AreEqual(700, config.WindowGeometry.Height);
        Assert.IsFalse(File.Exists(Path.Combine(UserDirectories.Config, "Nickvision.Desktop Tests", "config-sourcegen-async.json")));
    }

    [TestMethod]
    public void Case011_SaveWithSourceGen()
    {
        Assert.IsNotNull(_jsonFileService);
        var config = _jsonFileService.Load(TestJsonContext.Default.Config, "config-sourcegen");
        config.DarkModeEnabled = true;
        Assert.IsTrue(config.DarkModeEnabled);
        Assert.IsTrue(_jsonFileService.Save(config, TestJsonContext.Default.Config, "config-sourcegen"));
        Assert.IsTrue(File.Exists(Path.Combine(UserDirectories.Config, "Nickvision.Desktop Tests", "config-sourcegen.json")));
    }

    [TestMethod]
    public async Task Case012_SaveAsyncWithSourceGen()
    {
        Assert.IsNotNull(_jsonFileService);
        var config = await _jsonFileService.LoadAsync(TestJsonContext.Default.Config, "config-sourcegen-async");
        config.DarkModeEnabled = true;
        Assert.IsTrue(config.DarkModeEnabled);
        Assert.IsTrue(await _jsonFileService.SaveAsync(config, TestJsonContext.Default.Config, "config-sourcegen-async"));
        Assert.IsTrue(File.Exists(Path.Combine(UserDirectories.Config, "Nickvision.Desktop Tests", "config-sourcegen-async.json")));
    }

    [TestMethod]
    public void Case013_VerifyWithSourceGen()
    {
        Assert.IsNotNull(_jsonFileService);
        var config = _jsonFileService.Load(TestJsonContext.Default.Config, "config-sourcegen");
        Assert.IsNotNull(config);
        Assert.IsTrue(config.DarkModeEnabled);
    }

    [TestMethod]
    public async Task Case014_VerifyAsyncWithSourceGen()
    {
        Assert.IsNotNull(_jsonFileService);
        var config = await _jsonFileService.LoadAsync(TestJsonContext.Default.Config, "config-sourcegen-async");
        Assert.IsNotNull(config);
        Assert.IsTrue(config.DarkModeEnabled);
    }

    [TestMethod]
    public void Case015_CleanupSourceGen()
    {
        var configSourceGenPath = Path.Combine(UserDirectories.Config, "Nickvision.Desktop Tests", "config-sourcegen.json");
        var configSourceGenAsyncPath = Path.Combine(UserDirectories.Config, "Nickvision.Desktop Tests", "config-sourcegen-async.json");
        if (File.Exists(configSourceGenPath))
        {
            File.Delete(configSourceGenPath);
        }
        if (File.Exists(configSourceGenAsyncPath))
        {
            File.Delete(configSourceGenAsyncPath);
        }
        Assert.IsFalse(File.Exists(configSourceGenPath));
        Assert.IsFalse(File.Exists(configSourceGenAsyncPath));
        Directory.Delete(Path.Combine(UserDirectories.Config, "Nickvision.Desktop Tests"));
    }
}
