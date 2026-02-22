using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Nickvision.Desktop.WinUI.Hosting;

namespace Nickvision.Desktop.WinUI.Helpers;

public static class HostApplicationBuilderExtensions
{
    public static readonly string HostingContextKey;

    static HostApplicationBuilderExtensions()
    {
        HostingContextKey = "UserInterfaceHostingContext";
    }

    extension(IHostApplicationBuilder builder)
    {
        public IHostApplicationBuilder ConfigureWinUI<T>() where T : Microsoft.UI.Xaml.Application
        {
            if (!builder.Properties.TryGetValue(HostingContextKey, out var obj) || obj is not HostingContext context)
            {
                context = new HostingContext(true);
                builder.Properties[HostingContextKey] = context;
            }
            builder.Services.AddSingleton(context);
            builder.Services.AddSingleton<IUserInterfaceThread, UserInterfaceThread>();
            builder.Services.AddHostedService<UserInterfaceHostedService>();
            builder.Services.AddSingleton<T>();
            if (typeof(T) != typeof(Microsoft.UI.Xaml.Application))
            {
                builder.Services.AddSingleton<Microsoft.UI.Xaml.Application>(x => x.GetRequiredService<T>());
            }
            return builder;
        }
    }
}
