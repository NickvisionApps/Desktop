using Nickvision.Desktop.System;
using System.Runtime.Versioning;
using System.Threading.Tasks;

namespace Nickvision.Desktop.Tests;

[TestClass]
[SupportedOSPlatform("windows")]
[SupportedOSPlatform("linux")]
[SupportedOSPlatform("macos")]
public sealed class SystemSecretServiceTests
{
    private static SystemSecretService? _secretService;

    [TestMethod]
    public void Case001_Initialize()
    {
#if OS_LINUX
        if (Environment.GetEnvironmentVariable("CI") == "true")
        {
            Assert.Inconclusive("Dialogs are not supported in CI environments");
        }
#endif
        _secretService = new SystemSecretService();
        Assert.IsNotNull(_secretService);
    }

    [TestMethod]
    public async Task Case002_Add()
    {
#if OS_LINUX
        if (Environment.GetEnvironmentVariable("CI") == "true")
        {
            Assert.Inconclusive("Dialogs are not supported in CI environments");
        }
#endif
        Assert.IsNotNull(_secretService);
        var secret = new Secret("Nickvision.Desktop.Test", "abc");
        Assert.IsTrue(await _secretService.AddAsync(secret));
        Assert.IsFalse(_secretService.Add(secret));
    }

    [TestMethod]
    public async Task Case003_Create()
    {
#if OS_LINUX
        if (Environment.GetEnvironmentVariable("CI") == "true")
        {
            Assert.Inconclusive("Dialogs are not supported in CI environments");
        }
#endif
        Assert.IsNotNull(_secretService);
        var service = await _secretService.CreateAsync("Nickvision.Desktop.Test2");
        Assert.IsFalse(service is null);
        Assert.IsFalse(service.Empty);
    }

    [TestMethod]
    public void Case004_Create()
    {
#if OS_LINUX
        if (Environment.GetEnvironmentVariable("CI") == "true")
        {
            Assert.Inconclusive("Dialogs are not supported in CI environments");
        }
#endif
        Assert.IsNotNull(_secretService);
        var service = _secretService.Create("Nickvision.Desktop.Test3");
        Assert.IsFalse(service is null);
        Assert.IsFalse(service.Empty);
    }

    [TestMethod]
    public async Task Case005_Get()
    {
#if OS_LINUX
        if (Environment.GetEnvironmentVariable("CI") == "true")
        {
            Assert.Inconclusive("Dialogs are not supported in CI environments");
        }
#endif
        Assert.IsNotNull(_secretService);
        Assert.IsTrue(await _secretService.AddAsync(new Secret("Nickvision.Desktop.Test4", "abc")));
        var secret = _secretService.Get("Nickvision.Desktop.Test4");
        Assert.IsFalse(secret is null);
        Assert.IsFalse(secret.Empty);
        Assert.AreEqual("abc", secret.Value);
    }

    [TestMethod]
    public async Task Case006_Get()
    {
#if OS_LINUX
        if (Environment.GetEnvironmentVariable("CI") == "true")
        {
            Assert.Inconclusive("Dialogs are not supported in CI environments");
        }
#endif
        Assert.IsNotNull(_secretService);
        var secret = await _secretService.CreateAsync("Nickvision.Desktop.Test5");
        Assert.IsFalse(secret is null);
        Assert.IsFalse(secret.Empty);
        var secretFromGet = await _secretService.GetAsync("Nickvision.Desktop.Test5");
        Assert.IsFalse(secretFromGet is null);
        Assert.IsFalse(secretFromGet.Empty);
        Assert.AreEqual(secret.Value, secretFromGet.Value);
    }

    [TestMethod]
    public async Task Case007_Update()
    {
#if OS_LINUX
        if (Environment.GetEnvironmentVariable("CI") == "true")
        {
            Assert.Inconclusive("Dialogs are not supported in CI environments");
        }
#endif
        Assert.IsNotNull(_secretService);
        Assert.IsTrue(_secretService.Add(new Secret("Nickvision.Desktop.Test6", "abc123")));
        var secret = await _secretService.GetAsync("Nickvision.Desktop.Test6");
        Assert.IsFalse(secret is null);
        Assert.IsFalse(secret.Empty);
        Assert.AreEqual("abc123", secret.Value);
        Assert.IsTrue(await _secretService.UpdateAsync(new Secret("Nickvision.Desktop.Test6", "abc!")));
        secret = _secretService.Get("Nickvision.Desktop.Test6");
        Assert.IsFalse(secret is null);
        Assert.IsFalse(secret.Empty);
        Assert.AreEqual("abc!", secret.Value);
    }

    [TestMethod]
    public async Task Case008_Update()
    {
#if OS_LINUX
        if (Environment.GetEnvironmentVariable("CI") == "true")
        {
            Assert.Inconclusive("Dialogs are not supported in CI environments");
        }
#endif
        Assert.IsNotNull(_secretService);
        Assert.IsTrue(await _secretService.AddAsync(new Secret("Nickvision.Desktop.Test7", "abc123")));
        var secret = _secretService.Get("Nickvision.Desktop.Test7");
        Assert.IsFalse(secret is null);
        Assert.IsFalse(secret.Empty);
        Assert.AreEqual("abc123", secret.Value);
        Assert.IsTrue(_secretService.Update(new Secret("Nickvision.Desktop.Test7", "abc!")));
        secret = await _secretService.GetAsync("Nickvision.Desktop.Test7");
        Assert.IsFalse(secret is null);
        Assert.IsFalse(secret.Empty);
        Assert.AreEqual("abc!", secret.Value);
    }

    [TestMethod]
    public async Task Case009_Delete()
    {
#if OS_LINUX
        if (Environment.GetEnvironmentVariable("CI") == "true")
        {
            Assert.Inconclusive("Dialogs are not supported in CI environments");
        }
#endif
        Assert.IsNotNull(_secretService);
        foreach (var cred in new[]
        {
            "Nickvision.Desktop.Test",
            "Nickvision.Desktop.Test2",
            "Nickvision.Desktop.Test3",
            "Nickvision.Desktop.Test4",
            "Nickvision.Desktop.Test5",
            "Nickvision.Desktop.Test6",
            "Nickvision.Desktop.Test7"
        })
        {
            Assert.IsTrue(await _secretService.DeleteAsync(cred));
        }
    }
}
