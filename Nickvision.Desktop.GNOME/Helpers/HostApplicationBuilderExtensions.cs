using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Nickvision.Desktop.Application;
using Nickvision.Desktop.GNOME.Controls;
using Nickvision.Desktop.GNOME.Hosting;
using Nickvision.Desktop.Hosting;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace Nickvision.Desktop.GNOME.Helpers;

public static class HostApplicationBuilderExtensions
{
    extension(IHostApplicationBuilder builder)
    {
        [RequiresDynamicCode("Calls AddSingleton<T> which may use dynamic code generation.")]
        public IHostApplicationBuilder ConfigureAdw<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>(bool handlesOpen = false, bool includeResources = true) where T : Adw.ApplicationWindow
        {
            if (!builder.Properties.TryGetValue("UserInterfaceHostingContext", out var obj) || obj is not AdwUserInterfaceContext context)
            {
                context = new AdwUserInterfaceContext(handlesOpen);
                builder.Properties["UserInterfaceHostingContext"] = context;
            }
            if (!builder.Properties.TryGetValue("AppInfo", out obj) || obj is not AppInfo appInfo)
            {
                throw new InvalidOperationException("AppInfo must be configured before calling ConfigureAdw.");
            }
            if (OperatingSystem.IsMacOS())
            {
                context.ResourceBasePath = $"/{appInfo.Id.Replace('.', '/')}";
            }
            builder.Services.AddSingleton(context);
            builder.Services.AddSingleton<IUserInterfaceContext<Adw.Application>>(context);
            builder.Services.AddSingleton<IUserInterfaceThread, AdwUserInterfaceThread>();
            builder.Services.AddHostedService<UserInterfaceHostedService<Adw.Application>>();
            builder.Services.AddSingleton(Adw.Application.New(appInfo.Id, context.HandlesOpen ? Gio.ApplicationFlags.HandlesOpen : Gio.ApplicationFlags.DefaultFlags));
            builder.Services.AddSingleton<IGtkBuilderFactory, GtkBuilderFactory>();
            builder.Services.AddSingleton<T>();
            if (typeof(T) != typeof(Adw.ApplicationWindow))
            {
                builder.Services.AddSingleton<Adw.ApplicationWindow>(x => x.GetRequiredService<T>());
            }
            builder.Services.AddSingleton<ShortcutsDialog>();
            if (includeResources)
            {
                var resources = Gio.Resource.Load(Path.Combine(System.Environment.ExecutingDirectory, $"{appInfo.Id}.gresource"));
                resources.Register();
                builder.Properties["GioResources"] = resources;
            }
            return builder;
        }
    }
}
