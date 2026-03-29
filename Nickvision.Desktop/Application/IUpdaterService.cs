using Nickvision.Desktop.Network;
using System;
using System.Threading.Tasks;

namespace Nickvision.Desktop.Application;

public interface IUpdaterService
{
    Task<bool> DownloadReleaseAssetAsync(AppVersion version, string path, string assertName, bool exactMatch = true, IProgress<DownloadProgress>? progress = null);

    Task<AppVersion?> GetLatestPreviewVersionAsync();

    Task<AppVersion?> GetLatestStableVersionAsync();

    Task<bool> WindowsApplicationUpdateAsync(AppVersion version, IProgress<DownloadProgress>? progress = null);
}
