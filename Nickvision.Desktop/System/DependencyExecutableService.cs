using Microsoft.Extensions.Logging;
using Nickvision.Desktop.Application;
using Nickvision.Desktop.Filesystem;
using Nickvision.Desktop.Network;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;

namespace Nickvision.Desktop.System;

public abstract class DependencyExecutableService : IDependencyExecutableService
{
    protected readonly ILogger _logger;
    protected readonly string _executableName;
    protected readonly string _assetName;
    protected readonly IConfigurationService _configurationService;
    protected readonly IUpdaterService _updaterService;
    protected AppVersion? _latestStableVersion;
    protected AppVersion? _latestPreviewVersion;

    public AppVersion BundledVersion { get; }

    public virtual AppVersion InstalledVersion => _configurationService.Get($"installed_{_executableName}_appversion", BundledVersion, AppVersionJsonContext.Default.AppVersion);

    public DependencyExecutableService(ILogger logger, string executableName, AppVersion bundledVersion, string assetName, IConfigurationService configurationService, IUpdaterService updaterService)
    {
        _logger = logger;
        _executableName = executableName;
        _assetName = assetName;
        _configurationService = configurationService;
        _updaterService = updaterService;
        _latestStableVersion = null;
        BundledVersion = bundledVersion;
    }

    public virtual string ExecutablePath
    {
        get
        {
            if (!string.IsNullOrEmpty(field))
            {
                return field;
            }
            _logger.LogInformation($"Searching for {_executableName} executable...");
            var configKey = $"installed_{_executableName}_appversion";
            if (_configurationService.Get(configKey, BundledVersion, AppVersionJsonContext.Default.AppVersion) > BundledVersion)
            {
                var local = Environment.FindDependency(_executableName, DependencySearchOption.Local);
                if (!string.IsNullOrEmpty(local) && File.Exists(local))
                {
                    _logger.LogInformation($"Found updated {_executableName} executable: {local}");
                    field = local;
                    return field;
                }
                else
                {
                    _configurationService.Set(configKey, new AppVersion());
                    _configurationService.Save();
                }
            }
            field = Environment.FindDependency(_executableName, DependencySearchOption.Global);
            _logger.LogInformation($"Found bundled {_executableName} executable: {field}");
            return field ?? _executableName;
        }
    }

    public virtual async Task<bool> DownloadUpdateAsync(AppVersion version, IProgress<DownloadProgress>? progress = null)
    {
        var isZip = Path.GetExtension(_assetName).Equals(".zip", StringComparison.OrdinalIgnoreCase);
        var downloadPath = Path.Combine(UserDirectories.LocalData, $"{_executableName}.{(isZip ? "zip" : (OperatingSystem.IsWindows() ? "exe" : string.Empty))}");
        var res = await _updaterService.DownloadReleaseAssetAsync(version, downloadPath, _assetName, true, progress);
        if (res)
        {
            var configKey = $"installed_{_executableName}_appversion";
            var executablePath = Path.Combine(UserDirectories.LocalData, $"{_executableName}.{(OperatingSystem.IsWindows() ? "exe" : string.Empty)}");
            await _configurationService.SetAsync(configKey, version, AppVersionJsonContext.Default.AppVersion);
            if (isZip)
            {
                await ZipFile.ExtractToDirectoryAsync(downloadPath, UserDirectories.LocalData, true);
                File.Delete(downloadPath);
            }
            if (!OperatingSystem.IsWindows())
            {
                using var process = new Process()
                {
                    StartInfo = new ProcessStartInfo("chmod", ["0755", executablePath])
                    {
                        UseShellExecute = false,
                        CreateNoWindow = true,
                    }
                };
                process.Start();
                await process.WaitForExitAsync();
                return process.ExitCode == 0;
            }
        }
        return res;
    }

    public virtual async Task<ProcessResult> ExecuteAsync(IReadOnlyList<string> arguments, CancellationToken cancellationToken = default)
    {
        using var proc = new Process()
        {
            EnableRaisingEvents = true,
            StartInfo = new ProcessStartInfo(ExecutablePath, arguments)
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            }
        };
        proc.Start();
        var outputTask = proc.StandardOutput.ReadToEndAsync(cancellationToken);
        var errorTask = proc.StandardError.ReadToEndAsync(cancellationToken);
        await proc.WaitForExitAsync(cancellationToken);
        var output = await outputTask;
        var error = await errorTask;
        return new ProcessResult(proc.ExitCode, output, error);
    }

    public virtual async Task<AppVersion?> GetExecutableVersionAsync(string versionArgument = "--version")
    {
        using var process = new Process()
        {
            StartInfo = new ProcessStartInfo(ExecutablePath, versionArgument)
            {
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            }
        };
        process.Start();
        var outputTask = process.StandardOutput.ReadToEndAsync();
        await process.WaitForExitAsync();
        var output = await outputTask;
        if (process.ExitCode == 0 && AppVersion.TryParse(output.Trim(), out var version))
        {
            return version;
        }
        return null;
    }

    public virtual async Task<AppVersion?> GetLatestStableVersionAsync()
    {
        if (_latestStableVersion is null)
        {
            var _ = ExecutablePath;
            _latestStableVersion = await _updaterService.GetLatestStableVersionAsync();
        }
        return _latestStableVersion;
    }

    public virtual async Task<AppVersion?> GetLatestPreviewVersionAsync()
    {
        if (_latestPreviewVersion is null)
        {
            var _ = ExecutablePath;
            _latestPreviewVersion = await _updaterService.GetLatestPreviewVersionAsync();
        }
        return _latestPreviewVersion;
    }
}
