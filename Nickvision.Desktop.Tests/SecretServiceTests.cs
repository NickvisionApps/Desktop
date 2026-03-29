using Nickvision.Desktop.System;
using Nickvision.Desktop.Tests.Mocks;
using System.Threading.Tasks;

namespace Nickvision.Desktop.Tests;

[TestClass]
public sealed class SecretServiceTests
{
    private static SecretService? _secretService;

    [TestMethod]
    public void Case001_Initialize()
    {
        _secretService = new SecretService(new MockLogger<SecretService>());
        Assert.IsNotNull(_secretService);
    }

    [TestMethod]
    public async Task Case002_Add()
    {
        Assert.IsNotNull(_secretService);
        var secret = new Secret("Nickvision.Desktop.Test", "abc");
        Assert.IsTrue(await _secretService.AddAsync(secret));
        Assert.IsFalse(await _secretService.AddAsync(secret));
    }

    [TestMethod]
    public async Task Case003_Create()
    {
        Assert.IsNotNull(_secretService);
        var service = await _secretService.CreateAsync("Nickvision.Desktop.Test2");
        Assert.IsNotNull(service);
        Assert.IsFalse(service.Empty);
    }

    [TestMethod]
    public async Task Case004_Get()
    {
        Assert.IsNotNull(_secretService);
        Assert.IsTrue(await _secretService.AddAsync(new Secret("Nickvision.Desktop.Test3", "abc")));
        var secret = await _secretService.GetAsync("Nickvision.Desktop.Test3");
        Assert.IsNotNull(secret);
        Assert.IsFalse(secret.Empty);
        Assert.AreEqual("abc", secret.Value);
    }

    [TestMethod]
    public async Task Case005_Get()
    {
        Assert.IsNotNull(_secretService);
        var secret = await _secretService.CreateAsync("Nickvision.Desktop.Test4");
        Assert.IsNotNull(secret);
        Assert.IsFalse(secret.Empty);
        var secretFromGet = await _secretService.GetAsync("Nickvision.Desktop.Test4");
        Assert.IsNotNull(secretFromGet);
        Assert.IsFalse(secretFromGet.Empty);
        Assert.AreEqual(secret.Value, secretFromGet.Value);
    }

    [TestMethod]
    public async Task Case006_Update()
    {
        Assert.IsNotNull(_secretService);
        Assert.IsTrue(await _secretService.AddAsync(new Secret("Nickvision.Desktop.Test5", "abc123")));
        var secret = await _secretService.GetAsync("Nickvision.Desktop.Test5");
        Assert.IsNotNull(secret);
        Assert.IsFalse(secret.Empty);
        Assert.AreEqual("abc123", secret.Value);
        Assert.IsTrue(await _secretService.UpdateAsync(new Secret("Nickvision.Desktop.Test5", "abc!")));
        secret = await _secretService.GetAsync("Nickvision.Desktop.Test5");
        Assert.IsNotNull(secret);
        Assert.IsFalse(secret.Empty);
        Assert.AreEqual("abc!", secret.Value);
    }

    [TestMethod]
    public async Task Case007_Delete()
    {
        Assert.IsNotNull(_secretService);
        foreach (var cred in new[]
        {
            "Nickvision.Desktop.Test",
            "Nickvision.Desktop.Test2",
            "Nickvision.Desktop.Test3",
            "Nickvision.Desktop.Test4",
            "Nickvision.Desktop.Test5",
        })
        {
            Assert.IsTrue(await _secretService.DeleteAsync(cred));
        }
    }
}
