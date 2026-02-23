using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Nickvision.Desktop.Application;
using Nickvision.Desktop.Filesystem;
using Nickvision.Desktop.Globalization;
using Nickvision.Desktop.Helpers;
using Nickvision.Desktop.Keyring;
using Nickvision.Desktop.Notifications;
using Nickvision.Desktop.System;
using System;

namespace Nickvision.Desktop.Tests;

[TestClass]
public sealed class HostingTests
{
    private static HostApplicationBuilder? _applicationBuilder;

    [ClassInitialize]
    public static void ClassInitialize(TestContext context)
    {
        _applicationBuilder = Host.CreateApplicationBuilder();
    }

    [TestMethod]
    public void Case001_Create() => Assert.IsNotNull(_applicationBuilder);

    [TestMethod]
    public void Case002_Configure()
    {
        Assert.IsNotNull(_applicationBuilder);
        _applicationBuilder.ConfigureNickvision([]);
        _applicationBuilder.Services.AddSingleton(new AppInfo("org.nickvision.tubeconverter", "Nickvision Parabolic", "Parabolic")
        {
            SourceRepository = new Uri("https://github.com/NickvisionApps/Parabolic")
        });
    }

    [TestMethod]
    public void Case003_Build()
    {
        Assert.IsNotNull(_applicationBuilder);
        using var host = _applicationBuilder.Build();
        Assert.IsNotNull(host);
        Assert.IsNotNull(host.Services.GetRequiredService<AppInfo>());
        Assert.IsNotNull(host.Services.GetRequiredService<IArgumentsService>());
        Assert.IsNotNull(host.Services.GetRequiredService<IJsonFileService>());
        Assert.IsNotNull(host.Services.GetRequiredService<IKeyringService>());
        Assert.IsNotNull(host.Services.GetRequiredService<INotificationService>());
        Assert.IsNotNull(host.Services.GetRequiredService<IPowerService>());
        Assert.IsNotNull(host.Services.GetRequiredService<ISecretService>());
        Assert.IsNotNull(host.Services.GetRequiredService<ITranslationService>());
        Assert.IsNotNull(host.Services.GetRequiredService<IUpdaterService>());
    }
}
