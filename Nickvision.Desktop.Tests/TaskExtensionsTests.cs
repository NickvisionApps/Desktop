using Nickvision.Desktop.Helpers;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Nickvision.Desktop.Tests;

[TestClass]
public sealed class TaskExtensionsTests
{
    [TestMethod]
    public async Task Case001_FireAndForget_DoesNotThrowOnException()
    {
        var faulted = Task.FromException(new InvalidOperationException("should be swallowed"));
        faulted.FireAndForget();
        await Task.Delay(150);
    }

    [TestMethod]
    public async Task Case002_FireAndForget_SuccessfulTaskRuns()
    {
        var flag = 0;
        var task = Task.Run(() => Interlocked.Exchange(ref flag, 1));
        task.FireAndForget();
        await Task.Delay(200);
        Assert.AreEqual(1, flag);
    }
}
