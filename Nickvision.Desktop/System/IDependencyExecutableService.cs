using Nickvision.Desktop.Application;
using Nickvision.Desktop.Network;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Nickvision.Desktop.System;

public interface IDependencyExecutableService
{
    AppVersion BundledVersion { get; }
    string ExecutablePath { get; }
    AppVersion InstalledVersion { get; }

    Task<bool> DownloadUpdateAsync(AppVersion version, IProgress<DownloadProgress>? progress = null);
    Task<ProcessResult> ExecuteAsync(IReadOnlyList<string> arguments, CancellationToken cancellationToken = default);
    Task<AppVersion?> GetExecutableVersionAsync(string versionArgument = "--version");
    Task<AppVersion?> GetLatestStableVersionAsync();
    Task<AppVersion?> GetLatestPreviewVersionAsync();
}
