using Nickvision.Desktop.Application;
using System;

namespace Nickvision.Desktop.Tests;

internal interface ITestService : IDisposable, IService
{
    bool Disposed { get; }

    string GetData();
}

internal class TestService : ITestService
{
    public TestService()
    {
        Disposed = false;
    }

    public bool Disposed { get; private set; }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public string GetData() => "Test Data";

    ~TestService()
    {
        Dispose(false);
    }

    private void Dispose(bool disposing)
    {
        if (Disposed || !disposing)
        {
            return;
        }
        Disposed = true;
    }
}

[TestClass]
public class ServiceCollectionTests
{
    private static ServiceCollection? _collection;

    [TestMethod]
    public void Case001_Initialize()
    {
        _collection = new ServiceCollection();
        Assert.IsNotNull(_collection);
    }

    [TestMethod]
    public void Case002_Has()
    {
        Assert.IsNotNull(_collection);
        Assert.IsFalse(_collection.Has<ITestService>());
    }

    [TestMethod]
    public void Case003_Add()
    {
        Assert.IsNotNull(_collection);
        var service = new TestService();
        Assert.IsNotNull(_collection.Add<ITestService>(service));
        Assert.IsNull(_collection.Add<ITestService>(service));
        Assert.IsTrue(_collection.Has<ITestService>());
        Assert.IsFalse(service.Disposed);
    }

    [TestMethod]
    public void Case004_Get()
    {
        Assert.IsNotNull(_collection);
        var service = _collection.Get<ITestService>();
        Assert.IsNotNull(service);
        Assert.AreEqual("Test Data", service.GetData());
    }

    [TestMethod]
    public void Case005_Remove()
    {
        Assert.IsNotNull(_collection);
        Assert.IsTrue(_collection.Remove<ITestService>());
        Assert.IsNull(_collection.Get<ITestService>());
        Assert.IsFalse(_collection.Has<ITestService>());
    }

    [TestMethod]
    public void Case006_Dispose()
    {
        Assert.IsNotNull(_collection);
        var service = new TestService();
        Assert.IsNotNull(_collection.Add<ITestService>(service));
        Assert.IsFalse(service.Disposed);
        _collection.Dispose();
        _collection = null;
        Assert.IsTrue(service.Disposed);
    }
}
