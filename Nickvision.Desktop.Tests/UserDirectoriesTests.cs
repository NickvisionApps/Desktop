using Nickvision.Desktop.Filesystem;
using System.IO;

namespace Nickvision.Desktop.Tests;

[TestClass]
public sealed class UserDirectoriesTests
{
    [TestMethod]
    public void Case001_HomeDirectoryExists()
    {
        Assert.IsTrue(Directory.Exists(UserDirectories.Home));
    }

    [TestMethod]
    public void Case002_ConfigDirectoryExists()
    {
        Assert.IsTrue(Directory.Exists(UserDirectories.Config));
    }

    [TestMethod]
    public void Case003_CacheDirectoryExists()
    {
        Assert.IsTrue(Directory.Exists(UserDirectories.Cache));
    }

    [TestMethod]
    public void Case005_LocalDataDirectoryExists()
    {
        Assert.IsTrue(Directory.Exists(UserDirectories.LocalData));
    }

    [TestMethod]
    public void Case006_DesktopDirectoryExists()
    {
        Assert.IsTrue(Directory.Exists(UserDirectories.Desktop));
    }

    [TestMethod]
    public void Case007_DocumentsDirectoryExists()
    {
        Assert.IsTrue(Directory.Exists(UserDirectories.Documents));
    }

    [TestMethod]
    public void Case008_DownloadsDirectoryExists()
    {
        Assert.IsTrue(Directory.Exists(UserDirectories.Downloads));
    }

    [TestMethod]
    public void Case009_MusicDirectoryExists()
    {
        Assert.IsTrue(Directory.Exists(UserDirectories.Music));
    }

    [TestMethod]
    public void Case010_PicturesDirectoryExists()
    {
        Assert.IsTrue(Directory.Exists(UserDirectories.Pictures));
    }

    [TestMethod]
    public void Case011_TemplatesDirectoryExists()
    {
        Assert.IsTrue(Directory.Exists(UserDirectories.Templates));
    }

    [TestMethod]
    public void Case012_VideosDirectoryExists()
    {
        Assert.IsTrue(Directory.Exists(UserDirectories.Videos));
    }
}
