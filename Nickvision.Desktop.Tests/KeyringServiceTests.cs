using Nickvision.Desktop.Application;
using Nickvision.Desktop.Filesystem;
using Nickvision.Desktop.Keyring;
using Nickvision.Desktop.System;
using Nickvision.Desktop.Tests.Mocks;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Nickvision.Desktop.Tests;

[TestClass]
public sealed class KeyringServiceTests
{
    private static DatabaseService? _databaseService;
    private static KeyringService? _keyringService;

    [TestMethod]
    public void Case001_Init()
    {
        var appInfo = new AppInfo("org.nickvision.desktop.test.keyring", "Nickvision.Desktop.Test.Keyring", "Keyring Test");
        var secretService = new SecretService(new MockLogger<SecretService>());
        _databaseService = new DatabaseService(new MockLogger<DatabaseService>(), appInfo, secretService);
        _keyringService = new KeyringService(new MockLogger<KeyringService>(), appInfo, _databaseService, secretService);
        Assert.IsNotNull(_databaseService);
        Assert.IsNotNull(_keyringService);
    }

    [TestMethod]
    public async Task Case002_Get()
    {
        Assert.IsNotNull(_keyringService);
        Assert.AreEqual(0, (await _keyringService.GetAllCredentialAsync()).Count);
    }

    [TestMethod]
    public async Task Case003_Add()
    {
        Assert.IsNotNull(_keyringService);
        Assert.IsTrue(await _keyringService.AddCredentialAsync(new Credential("YouTube", "abc", "123", new Uri("https://www.youtube.com"))));
        Assert.IsNotNull((await _keyringService.GetAllCredentialAsync()).FirstOrDefault(c => c.Name == "YouTube"));
        Assert.IsFalse(await _keyringService.AddCredentialAsync(new Credential("YouTube", "abc", "123", new Uri("https://www.youtube.com"))));
        Assert.IsTrue(await _keyringService.AddCredentialAsync(new Credential("YouTube 2", "def", "456", new Uri("https://www.youtube.com"))));
        Assert.IsTrue(await _keyringService.AddCredentialAsync(new Credential("YouTube 3", "ghi", "789", new Uri("https://www.youtube.com"))));
    }

    [TestMethod]
    public async Task Case004_Get()
    {
        Assert.IsNotNull(_keyringService);
        Assert.AreEqual(3, (await _keyringService.GetAllCredentialAsync()).Count);
    }

    [TestMethod]
    public async Task Case005_Update()
    {
        Assert.IsNotNull(_keyringService);
        Assert.IsTrue(await _keyringService.AddCredentialAsync(new Credential("Google", "x@gmail.com", "asdfgh123!", new Uri("https://www.google.com"))));
        var cred = (await _keyringService.GetAllCredentialAsync()).FirstOrDefault(c => c.Name == "Google");
        Assert.IsNotNull(cred);
        cred.Password = "newpassword456!";
        Assert.IsTrue(await _keyringService.UpdateCredentialAsync(cred));
        var updatedCred = (await _keyringService.GetAllCredentialAsync()).FirstOrDefault(c => c.Name == "Google");
        Assert.IsNotNull(updatedCred);
        Assert.AreEqual("newpassword456!", updatedCred.Password);
    }

    [TestMethod]
    public async Task Case006_Remove()
    {
        Assert.IsNotNull(_keyringService);
        Assert.IsTrue(await _keyringService.AddCredentialAsync(new Credential("Example", "user1", "pass1", new Uri("https://www.example.com"))));
        var cred = (await _keyringService.GetAllCredentialAsync()).FirstOrDefault(c => c.Name == "Example");
        Assert.IsNotNull(cred);
        Assert.IsTrue(await _keyringService.DeleteCredentialAsync(cred));
        Assert.IsNull((await _keyringService.GetAllCredentialAsync()).FirstOrDefault(c => c.Name == "Example"));
    }

    [TestMethod]
    public async Task Case007_Cleanup()
    {
        var path = Path.Combine(UserDirectories.Config, "Nickvision.Desktop.Test.Keyring", "app.db");
        Assert.IsNotNull(_databaseService);
        Assert.IsNotNull(_keyringService);
        await _databaseService.DisposeAsync();
        File.Delete(path);
        Directory.Delete(Path.GetDirectoryName(path)!);
        Assert.IsFalse(File.Exists(path));
        _databaseService = null;
        _keyringService = null;
    }
}
