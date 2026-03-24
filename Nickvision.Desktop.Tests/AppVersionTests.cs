using Nickvision.Desktop.Application;
using System;

namespace Nickvision.Desktop.Tests;

[TestClass]
public sealed class AppVersionTests
{
    [TestMethod]
    public void Case001_StableVersion_ToString()
    {
        var v = new AppVersion("1.2.3");
        Assert.AreEqual("1.2.3", v.ToString());
    }

    [TestMethod]
    public void Case002_PreviewVersion_ToString()
    {
        var v = new AppVersion("2.0.0-alpha.1");
        Assert.AreEqual("2.0.0-alpha.1", v.ToString());
    }

    [TestMethod]
    public void Case003_IsPreview_False_ForStableVersion()
    {
        var v = new AppVersion("1.0.0");
        Assert.IsFalse(v.IsPreview);
    }

    [TestMethod]
    public void Case004_IsPreview_True_ForPreviewVersion()
    {
        var v = new AppVersion("1.0.0-beta");
        Assert.IsTrue(v.IsPreview);
    }

    [TestMethod]
    public void Case005_TryParse_ValidVersion_ReturnsTrue()
    {
        var result = AppVersion.TryParse("3.1.4", out var v);
        Assert.IsTrue(result);
        Assert.IsNotNull(v);
        Assert.AreEqual("3.1.4", v.ToString());
    }

    [TestMethod]
    public void Case006_TryParse_InvalidVersion_ReturnsFalse()
    {
        var result = AppVersion.TryParse("not-a-version", out var v);
        Assert.IsFalse(result);
        Assert.IsNull(v);
    }

    [TestMethod]
    public void Case007_Comparison_LessThan()
    {
        var v1 = new AppVersion("1.0.0");
        var v2 = new AppVersion("2.0.0");
        Assert.IsTrue(v1 < v2);
        Assert.IsFalse(v2 < v1);
    }

    [TestMethod]
    public void Case008_Comparison_GreaterThan()
    {
        var v1 = new AppVersion("3.0.0");
        var v2 = new AppVersion("2.9.9");
        Assert.IsTrue(v1 > v2);
        Assert.IsFalse(v2 > v1);
    }

    [TestMethod]
    public void Case009_Comparison_Equal()
    {
        var v1 = new AppVersion("1.5.0");
        var v2 = new AppVersion("1.5.0");
        Assert.IsTrue(v1 == v2);
        Assert.IsFalse(v1 != v2);
    }

    [TestMethod]
    public void Case010_SameBase_StableLessThanPreview()
    {
        // NOTE: This implementation uses ordinal string comparison on PreviewLabel,
        // so an empty label ("") sorts before any non-empty label ("beta").
        // This differs from SemVer, where stable (1.0.0) > prerelease (1.0.0-beta).
        var stable = new AppVersion("1.0.0");    // PreviewLabel = ""
        var preview = new AppVersion("1.0.0-beta");  // PreviewLabel = "beta"
        Assert.IsTrue(stable < preview);
        Assert.IsTrue(preview > stable);
    }

    [TestMethod]
    public void Case011_VPrefix_IsStripped()
    {
        var v = new AppVersion("v2.1.0");
        Assert.AreEqual("2.1.0", v.ToString());
    }

    [TestMethod]
    public void Case012_Constructor_FromVersion()
    {
        var baseVer = new Version(4, 0, 1);
        var v = new AppVersion(baseVer);
        Assert.AreEqual(baseVer, v.BaseVersion);
        Assert.IsFalse(v.IsPreview);
    }
}
