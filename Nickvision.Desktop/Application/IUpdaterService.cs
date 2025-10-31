using Nickvision.Desktop.Network;
using System;
using System.Threading.Tasks;

namespace Nickvision.Desktop.Application;

public interface IUpdaterService
{
    Task<bool> DownloadReleaseAssetAsync(
        Version version,
        string path,
        string assertName,
        bool exactMatch = true,
        IProgress<DownloadProgress>? progress = null);

    Task<Version?> GetLatestPreviewVersionAsync();

    Task<Version?> GetLatestStableVersionAsync();

#if OS_WINDOWS
    Task<bool> WindowsUpdate(Version version, IProgress<DownloadProgress>? progress = null);
#endif
}
