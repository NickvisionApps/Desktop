using Nickvision.Desktop.Helpers;
using Nickvision.Desktop.Keyring;
using System;

namespace Nickvision.Desktop.Tests;

[TestClass]
public sealed class CredentialTests
{
    [TestMethod]
    public void Case001_Constructor_WithUrl_StoresAllFields()
    {
        var url = new Uri("https://example.com");
        var c = new Credential("MyService", "user", "pass", url);
        Assert.AreEqual("MyService", c.Name);
        Assert.AreEqual("user", c.Username);
        Assert.AreEqual("pass", c.Password);
        Assert.AreEqual(url, c.Url);
    }

    [TestMethod]
    public void Case002_Constructor_WithoutUrl_DefaultsToUriEmpty()
    {
        var c = new Credential("MyService", "user", "pass");
        Assert.AreEqual(Uri.Empty, c.Url);
        Assert.IsTrue(c.Url.IsEmpty);
    }

    [TestMethod]
    public void Case003_Constructor_WithNullUrl_DefaultsToUriEmpty()
    {
        var c = new Credential("MyService", "user", "pass", null);
        Assert.AreEqual(Uri.Empty, c.Url);
        Assert.IsTrue(c.Url.IsEmpty);
    }

    [TestMethod]
    public void Case004_Properties_AreMutable()
    {
        var c = new Credential("name", "user", "pass");
        c.Name = "new-name";
        c.Username = "new-user";
        c.Password = "new-pass";
        Assert.AreEqual("new-name", c.Name);
        Assert.AreEqual("new-user", c.Username);
        Assert.AreEqual("new-pass", c.Password);
    }
}
