using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Nickvision.Desktop.Application;
using Nickvision.Desktop.Filesystem;
using Nickvision.Desktop.Globalization;
using Nickvision.Desktop.Keyring;
using Nickvision.Desktop.Notifications;
using Nickvision.Desktop.System;
using NReco.Logging.File;

namespace Nickvision.Desktop.Helpers;

public static class HostApplicationBuilderExtensions
{
    extension(IHostApplicationBuilder builder)
    {
        public IHostApplicationBuilder ConfigureNickvision(string[] args, string loggingPath)
        {
            builder.Services.AddSingleton<IArgumentsService>(new ArgumentsService(args));
            builder.Services.AddSingleton<IJsonFileService, JsonFileService>();
            builder.Services.AddSingleton<IKeyringService, KeyringService>();
            builder.Services.AddSingleton<INotificationService, NotificationService>();
            builder.Services.AddSingleton<IPowerService, PowerService>();
            builder.Services.AddSingleton<ISecretService, SecretService>();
            builder.Services.AddSingleton<ITranslationService, TranslationService>();
            builder.Services.AddSingleton<IUpdaterService, UpdaterService>();
            builder.Services.AddHttpClient<IUpdaterService, UpdaterService>();
            builder.Logging.ClearProviders();
            builder.Logging.SetMinimumLevel(LogLevel.Information);
            builder.Logging.AddConsole();
            if(!string.IsNullOrEmpty(loggingPath))
            {
                builder.Logging.AddFile(loggingPath);
            }
            return builder;
        }
    }
}
