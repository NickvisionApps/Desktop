using Nickvision.Desktop.Application;
using Nickvision.Desktop.Filesystem;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace Nickvision.Desktop.System;

public static class Environment
{
    private static readonly Dictionary<(string Dependency, DependencySearchOption Search), string?> Dependencies;

    static Environment()
    {
        Dependencies = [];
    }

    public static DeploymentMode DeploymentMode
    {
        get
        {
            if (global::System.Environment.GetEnvironmentVariable("container") is not null)
            {
                return DeploymentMode.Flatpak;
            }
            else if (global::System.Environment.GetEnvironmentVariable("SNAP") is not null)
            {
                return DeploymentMode.Snap;
            }
            return DeploymentMode.Local;
        }
    }

    public static IEnumerable<string> PathVariable
    {
        get
        {
            var path = global::System.Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
#if OS_WINDOWS
            return path.Split(';').Select(p => global::System.Environment.ExpandEnvironmentVariables(p));
#else
            return path.Split(':').Select(p => global::System.Environment.ExpandEnvironmentVariables(p));
#endif
        }
    }

    public static string? FindDependency(
        string dependency,
        DependencySearchOption search = DependencySearchOption.Global)
    {
#if OS_WINDOWS
        if (string.IsNullOrEmpty(Path.GetExtension(dependency) ?? string.Empty))
        {
            dependency += ".exe";
        }
#endif
        if (Dependencies.TryGetValue((dependency, search), out var cachedPath))
        {
            if (cachedPath is not null &&
                File.Exists(cachedPath))
            {
                return cachedPath;
            }
        }
        Dependencies[(dependency, search)] = null;
        string path;
        switch (search)
        {
            case DependencySearchOption.Global:
                path = Path.Combine(Directory.GetCurrentDirectory(), dependency);
                if (File.Exists(path))
                {
                    Dependencies[(dependency, search)] = path;
                }
                else
                {
                    foreach (var dir in PathVariable)
                    {
                        path = Path.Combine(dir, dependency);
                        if (!File.Exists(path) ||
                            dir.Contains(@"AppData\Local\Microsoft\WindowsApps"))
                        {
                            continue;
                        }
                        Dependencies[(dependency, search)] = path;
                        break;
                    }
                }
                break;
            case DependencySearchOption.App:
                path = Path.Combine(Directory.GetCurrentDirectory(), dependency);
                if (File.Exists(path))
                {
                    Dependencies[(dependency, search)] = path;
                }
                break;
            case DependencySearchOption.System:
                foreach (var dir in PathVariable)
                {
                    path = Path.Combine(dir, dependency);
                    if (!File.Exists(path) ||
                        dir.Contains(@"AppData\Local\Microsoft\WindowsApps"))
                    {
                        continue;
                    }
                    Dependencies[(dependency, search)] = path;
                    break;
                }
                break;
            case DependencySearchOption.Local:
                path = Path.Combine(UserDirectories.LocalData, dependency);
                if (File.Exists(path))
                {
                    Dependencies[(dependency, search)] = path;
                }
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(search), search, null);
        }
        return Dependencies[(dependency, search)];
    }

    public static string GetDebugInformation(AppInfo info, string extra = "") =>
        $"""
         App: {info.Name}
         Version: {info.Version}

         Operating System: {RuntimeInformation.OSDescription}
         Deployment Mode: {DeploymentMode}
         Locale: {CultureInfo.CurrentCulture.Name}
         Running From: {Directory.GetCurrentDirectory()}

         {extra}
         """;
}
