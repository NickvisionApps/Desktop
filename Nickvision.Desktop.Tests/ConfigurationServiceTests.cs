using Nickvision.Desktop.Application;
using Nickvision.Desktop.Filesystem;
using Nickvision.Desktop.System;
using Nickvision.Desktop.Tests.Mocks;
using System.IO;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Nickvision.Desktop.Tests;

public class TestObj
{
    public string Test { get; set; }

    public TestObj(string test)
    {
        Test = test;
    }
}

[JsonSourceGenerationOptions(DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull, WriteIndented = true)]
[JsonSerializable(typeof(TestObj))]
internal partial class TestJsonContext : JsonSerializerContext { }

[TestClass]
public class ConfigurationServiceTests
{
    private static DatabaseService? _databaseService;
    private static ConfigurationService? _configurationService;

    [TestMethod]
    public void Case001_Init()
    {
        _databaseService = new DatabaseService(new MockLogger<DatabaseService>(), new AppInfo("org.nickvision.desktop.test.config", "Nickvision.Desktop.Test.Config", "Config Test"), new SecretService(new MockLogger<SecretService>()));
        _configurationService = new ConfigurationService(new MockLogger<ConfigurationService>(), _databaseService);
        Assert.IsNotNull(_databaseService);
        Assert.IsNotNull(_configurationService);
    }

    [TestMethod]
    public async Task Case002_Get()
    {
        Assert.IsNotNull(_configurationService);
        var val1 = _configurationService.GetBool("nonExistentBool", false);
        Assert.AreEqual(false, val1);
        var val2 = await _configurationService.GetBoolAsync("nonExistentBoolAsync", true);
        Assert.AreEqual(true, val2);
        var val3 = _configurationService.GetDouble("nonExistentDouble", 1.5);
        Assert.AreEqual(1.5, val3);
        var val4 = await _configurationService.GetDoubleAsync("nonExistentDoubleAsync", 2.5);
        Assert.AreEqual(2.5, val4);
        var val5 = _configurationService.GetInt("nonExistentInt", 42);
        Assert.AreEqual(42, val5);
        var val6 = await _configurationService.GetIntAsync("nonExistentIntAsync", 84);
        Assert.AreEqual(84, val6);
        var val7 = _configurationService.GetString("nonExistentString", "default");
        Assert.AreEqual("default", val7);
        var val8 = await _configurationService.GetStringAsync("nonExistentStringAsync", "asyncDefault");
        Assert.AreEqual("asyncDefault", val8);
        var val9 = _configurationService.GetObject("nonExistentObject", new TestObj("value"), TestJsonContext.Default.TestObj);
        Assert.AreEqual("value", val9.Test);
        var val10 = await _configurationService.GetObjectAsync("nonExistentObjectAsync", new TestObj("asyncValue"), TestJsonContext.Default.TestObj);
        Assert.AreEqual("asyncValue", val10.Test);
    }

    [TestMethod]
    public void Case003_Save()
    {
        Assert.IsNotNull(_configurationService);
        _configurationService.Save();
    }

    [TestMethod]
    public async Task Case004_Set()
    {
        Assert.IsNotNull(_configurationService);
        _configurationService.Set("nonExistentBool", true);
        await _configurationService.SetAsync("nonExistentBoolAsync", false);
        _configurationService.Set("nonExistentDouble", 2.5);
        await _configurationService.SetAsync("nonExistentDoubleAsync", 1.5);
        _configurationService.Set("nonExistentInt", 84);
        await _configurationService.SetAsync("nonExistentIntAsync", 42);
        _configurationService.Set("nonExistentString", "default2");
        await _configurationService.SetAsync("nonExistentStringAsync", "asyncDefault2");
        _configurationService.Set("nonExistentObject", new TestObj("value2"), TestJsonContext.Default.TestObj);
        await _configurationService.SetAsync("nonExistentObjectAsync", new TestObj("asyncValue2"), TestJsonContext.Default.TestObj);
    }

    [TestMethod]
    public async Task Case005_Save()
    {
        Assert.IsNotNull(_configurationService);
        await _configurationService.SaveAsync();
    }

    [TestMethod]
    public async Task Case006_Get()
    {
        Assert.IsNotNull(_configurationService);
        var val1 = _configurationService.GetBool("nonExistentBool", false);
        Assert.AreEqual(true, val1);
        var val2 = await _configurationService.GetBoolAsync("nonExistentBoolAsync", true);
        Assert.AreEqual(false, val2);
        var val3 = _configurationService.GetDouble("nonExistentDouble", 1.5);
        Assert.AreEqual(2.5, val3);
        var val4 = await _configurationService.GetDoubleAsync("nonExistentDoubleAsync", 2.5);
        Assert.AreEqual(1.5, val4);
        var val5 = _configurationService.GetInt("nonExistentInt", 42);
        Assert.AreEqual(84, val5);
        var val6 = await _configurationService.GetIntAsync("nonExistentIntAsync", 84);
        Assert.AreEqual(42, val6);
        var val7 = _configurationService.GetString("nonExistentString", "default");
        Assert.AreEqual("default2", val7);
        var val8 = await _configurationService.GetStringAsync("nonExistentStringAsync", "asyncDefault");
        Assert.AreEqual("asyncDefault2", val8);
        var val9 = _configurationService.GetObject("nonExistentObject", new TestObj("value"), TestJsonContext.Default.TestObj);
        Assert.AreEqual("value2", val9.Test);
        var val10 = await _configurationService.GetObjectAsync("nonExistentObjectAsync", new TestObj("asyncValue"), TestJsonContext.Default.TestObj);
        Assert.AreEqual("asyncValue2", val10.Test);
    }

    [TestMethod]
    public async Task Case007_Dispose()
    {
        Assert.IsNotNull(_databaseService);
        Assert.IsNotNull(_configurationService);
        await _configurationService.DisposeAsync();
        await _databaseService.DisposeAsync();
        _databaseService = null;
        _configurationService = null;
    }

    [TestMethod]
    public void Case008_Init()
    {
        _databaseService = new DatabaseService(new MockLogger<DatabaseService>(), new AppInfo("org.nickvision.desktop.test.config", "Nickvision.Desktop.Test.Config", "Config Test"), new SecretService(new MockLogger<SecretService>()));
        _configurationService = new ConfigurationService(new MockLogger<ConfigurationService>(), _databaseService);
        Assert.IsNotNull(_databaseService);
        Assert.IsNotNull(_configurationService);
    }

    [TestMethod]
    public async Task Case009_Get()
    {
        Assert.IsNotNull(_configurationService);
        var val1 = _configurationService.GetBool("nonExistentBool", false);
        Assert.AreEqual(true, val1);
        var val2 = await _configurationService.GetBoolAsync("nonExistentBoolAsync", true);
        Assert.AreEqual(false, val2);
        var val3 = _configurationService.GetDouble("nonExistentDouble", 1.5);
        Assert.AreEqual(2.5, val3);
        var val4 = await _configurationService.GetDoubleAsync("nonExistentDoubleAsync", 2.5);
        Assert.AreEqual(1.5, val4);
        var val5 = _configurationService.GetInt("nonExistentInt", 42);
        Assert.AreEqual(84, val5);
        var val6 = await _configurationService.GetIntAsync("nonExistentIntAsync", 84);
        Assert.AreEqual(42, val6);
        var val7 = _configurationService.GetString("nonExistentString", "default");
        Assert.AreEqual("default2", val7);
        var val8 = await _configurationService.GetStringAsync("nonExistentStringAsync", "asyncDefault");
        Assert.AreEqual("asyncDefault2", val8);
        var val9 = _configurationService.GetObject("nonExistentObject", new TestObj("value"), TestJsonContext.Default.TestObj);
        Assert.AreEqual("value2", val9.Test);
        var val10 = await _configurationService.GetObjectAsync("nonExistentObjectAsync", new TestObj("asyncValue"), TestJsonContext.Default.TestObj);
        Assert.AreEqual("asyncValue2", val10.Test);
    }

    [TestMethod]
    public async Task Case010_GetAll()
    {
        Assert.IsNotNull(_configurationService);
        Assert.AreEqual(10, (await _configurationService.GetAllRawAsync()).Count);
    }

    [TestMethod]
    public async Task Case011_Cleanup()
    {
        var path = Path.Combine(UserDirectories.Config, "Nickvision.Desktop.Test.Config", "app.db");
        Assert.IsNotNull(_databaseService);
        Assert.IsNotNull(_configurationService);
        await _configurationService.DisposeAsync();
        await _databaseService.DisposeAsync();
        File.Delete(path);
        Directory.Delete(Path.GetDirectoryName(path)!);
        Assert.IsFalse(File.Exists(path));
        _databaseService = null;
        _configurationService = null;
    }
}
