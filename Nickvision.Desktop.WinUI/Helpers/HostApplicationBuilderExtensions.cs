using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Nickvision.Desktop.Application;
using Nickvision.Desktop.Hosting;
using Nickvision.Desktop.WinUI.Hosting;
using System;

namespace Nickvision.Desktop.WinUI.Helpers;

public static class HostApplicationBuilderExtensions
{
    extension(IHostApplicationBuilder builder)
    {
        public IHostApplicationBuilder ConfigureWinUI<T>() where T : Microsoft.UI.Xaml.Application
        {
            if (!builder.Properties.TryGetValue("UserInterfaceHostingContext", out var obj) || obj is not WinUIUserInterfaceContext context)
            {
                context = new WinUIUserInterfaceContext();
                builder.Properties["UserInterfaceHostingContext"] = context;
            }
            if (!builder.Properties.TryGetValue("AppInfo", out obj) || obj is not AppInfo)
            {
                throw new InvalidOperationException("AppInfo must be configured before calling ConfigureWinUI.");
            }
            builder.Services.AddSingleton(context);
            builder.Services.AddSingleton<IUserInterfaceThread, WinUIUserInterfaceThread>();
            builder.Services.AddHostedService<UserInterfaceHostedService<Microsoft.UI.Xaml.Application>>();
            builder.Services.AddSingleton<T>();
            if (typeof(T) != typeof(Microsoft.UI.Xaml.Application))
            {
                builder.Services.AddSingleton<Microsoft.UI.Xaml.Application>(x => x.GetRequiredService<T>());
            }
            return builder;
        }
    }
}
