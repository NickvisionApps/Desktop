using Nickvision.Desktop.Application;
using Nickvision.Desktop.Filesystem;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Nickvision.Desktop.System;

/// <summary>
/// Helpers for working with the system environment.
/// </summary>
public static class Environment
{
    private static readonly Dictionary<(string Dependency, DependencySearchOption Search), string?> Dependencies;

    /// <summary>
    /// Constructs a static Environment.
    /// </summary>
    static Environment()
    {
        Dependencies = [];
    }

    /// <summary>
    /// The deployment mode of the application.
    /// </summary>
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

    /// <summary>
    /// The application executable's directory.
    /// </summary>
    public static string ExecutingDirectory => Path.GetDirectoryName(ExecutingPath) ?? global::System.Environment.CurrentDirectory;

    /// <summary>
    /// The application executable's path.
    /// </summary>
    public static string ExecutingPath => Path.GetFullPath(global::System.Environment.ProcessPath!);

    /// <summary>
    /// The list of directories in the PATH variable.
    /// </summary>
    public static IEnumerable<string> PathVariable
    {
        get
        {
            var path = global::System.Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
            return (OperatingSystem.IsWindows() ? path.Split(';') : path.Split(':')).Select(global::System.Environment.ExpandEnvironmentVariables);
        }
    }

    /// <summary>
    /// Finds a dependency on the system.
    /// </summary>
    /// <param name="dependency">The dependency to find</param>
    /// <param name="search">The search options</param>
    /// <returns>The path of the dependency if found, else null</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the search option is invalid</exception>
    public static string? FindDependency(string dependency, DependencySearchOption search = DependencySearchOption.Global)
    {
        if (OperatingSystem.IsWindows())
        {
            if (string.IsNullOrEmpty(Path.GetExtension(dependency) ?? string.Empty))
            {
                dependency += ".exe";
            }
        }
        if (Dependencies.TryGetValue((dependency, search), out var cachedPath))
        {
            if (cachedPath is not null && File.Exists(cachedPath))
            {
                return cachedPath;
            }
        }
        Dependencies[(dependency, search)] = null;
        string path;
        switch (search)
        {
            case DependencySearchOption.Global:
                path = Path.Combine(ExecutingDirectory, dependency);
                if (File.Exists(path))
                {
                    Dependencies[(dependency, search)] = path;
                }
                else
                {
                    foreach (var dir in PathVariable)
                    {
                        path = Path.Combine(dir, dependency);
                        if (!File.Exists(path) || dir.Contains(@"AppData\Local\Microsoft\WindowsApps"))
                        {
                            continue;
                        }
                        Dependencies[(dependency, search)] = path;
                        break;
                    }
                }
                break;
            case DependencySearchOption.App:
                path = Path.Combine(ExecutingDirectory, dependency);
                if (File.Exists(path))
                {
                    Dependencies[(dependency, search)] = path;
                }
                break;
            case DependencySearchOption.System:
                foreach (var dir in PathVariable)
                {
                    path = Path.Combine(dir, dependency);
                    if (!File.Exists(path) || dir.Contains(@"AppData\Local\Microsoft\WindowsApps"))
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

    /// <summary>
    /// Gets the debug information for the application.
    /// </summary>
    /// <param name="info">The AppInfo object for the app</param>
    /// <param name="extra">Any extra information to include in the debug information string</param>
    /// <returns>The debug information string</returns>
    public static string GetDebugInformation(AppInfo info, string extra = "") => $"""
         App: {info.Name}
         Version: {info.Version}

         Operating System: {RuntimeInformation.OSDescription}
         Deployment Mode: {DeploymentMode}
         Locale: {CultureInfo.CurrentCulture.Name}
         Running From: {ExecutingDirectory}

         {extra}
         """;
}
