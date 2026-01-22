using Nickvision.Desktop.Application;
using Nickvision.Desktop.Filesystem;
using Nickvision.Desktop.Keyring;
using Nickvision.Desktop.System;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Nickvision.Desktop.Tests;

[TestClass]
public sealed class DatabaseKeyringServiceTests
{
    private static DatabaseKeyringService? _keyringService;

    [TestMethod]
    public void Case001_Init()
    {
#if OS_LINUX
        if (global::System.Environment.GetEnvironmentVariable("CI") == "true")
        {
            Assert.Inconclusive("Dialogs are not supported in CI environments");
        }
#endif
        _keyringService = new DatabaseKeyringService(new AppInfo("org.nickvision.desktop.test", "Nickvision.Desktop.Test", "Test"), new SystemSecretService());
        Assert.IsNotNull(_keyringService);
        Assert.IsTrue(_keyringService.IsSavingToDisk);
    }

    [TestMethod]
    public void Case002_Check()
    {
#if OS_LINUX
        if (global::System.Environment.GetEnvironmentVariable("CI") == "true")
        {
            Assert.Inconclusive("Dialogs are not supported in CI environments");
        }
#endif
        Assert.IsNotNull(_keyringService);
        Assert.IsTrue(_keyringService.IsSavingToDisk);
    }

    [TestMethod]
    public async Task Case003_Add()
    {
#if OS_LINUX
        if (global::System.Environment.GetEnvironmentVariable("CI") == "true")
        {
            Assert.Inconclusive("Dialogs are not supported in CI environments");
        }
#endif
        Assert.IsNotNull(_keyringService);
        Assert.IsTrue(await _keyringService.AddCredentialAsync(new Credential("YouTube", "abc", "123", new Uri("https://www.youtube.com"))));
        Assert.IsNotNull(_keyringService.Credentials.FirstOrDefault(c => c.Name == "YouTube"));
        Assert.IsFalse(await _keyringService.AddCredentialAsync(new Credential("YouTube", "abc", "123", new Uri("https://www.youtube.com"))));
    }

    [TestMethod]
    public async Task Case004_Update()
    {
#if OS_LINUX
        if (global::System.Environment.GetEnvironmentVariable("CI") == "true")
        {
            Assert.Inconclusive("Dialogs are not supported in CI environments");
        }
#endif
        Assert.IsNotNull(_keyringService);
        Assert.IsTrue(await _keyringService.AddCredentialAsync(new Credential("Google", "x@gmail.com", "asdfgh123!", new Uri("https://www.google.com"))));
        var cred = _keyringService.Credentials.FirstOrDefault(c => c.Name == "Google");
        Assert.IsNotNull(cred);
        cred.Password = "newpassword456!";
        Assert.IsTrue(await _keyringService.UpdateCredentialAsync(cred));
        var updatedCred = _keyringService.Credentials.FirstOrDefault(c => c.Name == "Google");
        Assert.IsNotNull(updatedCred);
        Assert.AreEqual("newpassword456!", updatedCred.Password);
    }

    [TestMethod]
    public async Task Case005_Remove()
    {
#if OS_LINUX
        if (global::System.Environment.GetEnvironmentVariable("CI") == "true")
        {
            Assert.Inconclusive("Dialogs are not supported in CI environments");
        }
#endif
        Assert.IsNotNull(_keyringService);
        Assert.IsTrue(await _keyringService.AddCredentialAsync(new Credential("Example", "user1", "pass1", new Uri("https://www.example.com"))));
        var cred = _keyringService.Credentials.FirstOrDefault(c => c.Name == "Example");
        Assert.IsNotNull(cred);
        Assert.IsTrue(await _keyringService.RemoveCredentialAsync(cred));
        Assert.IsNull(_keyringService.Credentials.FirstOrDefault(c => c.Name == "Example"));
    }

    [TestMethod]
    public async Task Case006_Cleanup()
    {
#if OS_LINUX
        if (global::System.Environment.GetEnvironmentVariable("CI") == "true")
        {
            Assert.Inconclusive("Dialogs are not supported in CI environments");
        }
#endif
        Assert.IsNotNull(_keyringService);
        Assert.IsTrue(await _keyringService.DestroyAsync());
        Assert.IsFalse(_keyringService.Credentials.Any());
        Assert.IsFalse(_keyringService.IsSavingToDisk);
        Assert.IsFalse(File.Exists(Path.Combine(UserDirectories.Config, "Nickvision", "Keyring", "Nickvision.Desktop.Keyring.Test.ring2")));
        _keyringService = null;
    }
}
