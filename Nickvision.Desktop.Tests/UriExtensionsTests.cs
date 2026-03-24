using Nickvision.Desktop.Helpers;
using System;

namespace Nickvision.Desktop.Tests;

[TestClass]
public sealed class UriExtensionsTests
{
    [TestMethod]
    public void Case001_Empty_IsAboutBlank()
    {
        Assert.AreEqual("about:blank", Uri.Empty.ToString());
    }

    [TestMethod]
    public void Case002_Empty_IsSameInstanceOnMultipleCalls()
    {
        var a = Uri.Empty;
        var b = Uri.Empty;
        Assert.IsTrue(ReferenceEquals(a, b));
    }

    [TestMethod]
    public void Case003_IsEmpty_TrueForEmptyUri()
    {
        Assert.IsTrue(Uri.Empty.IsEmpty);
    }

    [TestMethod]
    public void Case004_IsEmpty_TrueForAboutBlankString()
    {
        var uri = new Uri("about:blank");
        Assert.IsTrue(uri.IsEmpty);
    }

    [TestMethod]
    public void Case005_IsEmpty_FalseForRealUrl()
    {
        var uri = new Uri("https://example.com");
        Assert.IsFalse(uri.IsEmpty);
    }

    [TestMethod]
    public void Case006_IsEmpty_FalseForLocalhostUrl()
    {
        var uri = new Uri("http://localhost:8080");
        Assert.IsFalse(uri.IsEmpty);
    }
}
