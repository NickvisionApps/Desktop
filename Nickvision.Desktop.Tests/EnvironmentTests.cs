using Nickvision.Desktop.System;
using System.IO;
using System.Linq;

namespace Nickvision.Desktop.Tests;

[TestClass]
public sealed class EnvironmentTests
{
    [TestMethod]
    public void Case001_Deployment()
    {
        Assert.AreEqual(DeploymentMode.Local, Environment.DeploymentMode);
    }

    [TestMethod]
    public void Case002_PathVariable()
    {
        Assert.IsTrue(Environment.PathVariable.Any());
    }

    [TestMethod]
    public void Case003_GlobalDependency()
    {
#if OS_WINDOWS
        const string dependency = "cmd.exe";
#else
        const string dependency = "ls";
#endif
        var path = Environment.FindDependency(dependency);
        Assert.IsFalse(string.IsNullOrEmpty(path));
        Assert.IsTrue(File.Exists(path));
    }

    [TestMethod]
    public void Case004_AppDependency()
    {
#if OS_WINDOWS
        const string dependency = "cmd.exe";
#else
        const string dependency = "ls";
#endif
        var path = Environment.FindDependency(dependency, DependencySearchOption.App);
        Assert.IsTrue(string.IsNullOrEmpty(path));
        Assert.IsFalse(File.Exists(path));
    }

    [TestMethod]
    public void Case005_SystemDependency()
    {
#if OS_WINDOWS
        const string dependency = "cmd.exe";
#else
        const string dependency = "ls";
#endif
        var path = Environment.FindDependency(dependency, DependencySearchOption.System);
        Assert.IsFalse(string.IsNullOrEmpty(path));
        Assert.IsTrue(File.Exists(path));
    }

    [TestMethod]
    public void Case006_LocalDependency()
    {
#if OS_WINDOWS
        const string dependency = "cmd.exe";
#else
        const string dependency = "ls";
#endif
        var path = Environment.FindDependency(dependency, DependencySearchOption.Local);
        Assert.IsTrue(string.IsNullOrEmpty(path));
        Assert.IsFalse(File.Exists(path));
    }
}
