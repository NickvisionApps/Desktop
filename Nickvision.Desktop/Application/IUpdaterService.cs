using Nickvision.Desktop.Network;
using System;
using System.Threading.Tasks;

namespace Nickvision.Desktop.Application;

/// <summary>
///     An interface of a service for updating an application.
/// </summary>
public interface IUpdaterService
{
    /// <summary>
    ///     Downloads an asset from a released version.
    /// </summary>
    /// <param name="version">The released version</param>
    /// <param name="path">The path of where to download the asset to</param>
    /// <param name="assertName">The name of the asset to download</param>
    /// <param name="exactMatch">Whether the asset name should match exactly to the asset to download</param>
    /// <param name="progress">An optional progress reporter</param>
    /// <returns></returns>
    Task<bool> DownloadReleaseAssetAsync(Version version,
        string path,
        string assertName,
        bool exactMatch = true,
        IProgress<DownloadProgress>? progress = null);

    /// <summary>
    ///     Gets the latest preview version available.
    /// </summary>
    /// <returns>The latest preview version or null if unavailable</returns>
    Task<PreviewVersion?> GetLatestPreviewVersionAsync();

    /// <summary>
    ///     Gets the latest stable version available.
    /// </summary>
    /// <returns>The latest stable version or null if unavailable</returns>
    Task<Version?> GetLatestStableVersionAsync();

#if OS_WINDOWS
    /// <summary>
    ///     Downloads and runs the updated Windows installer of the given released version.
    /// </summary>
    /// <param name="version">The released version</param>
    /// <param name="progress">An optional progress reporter</param>
    /// <returns>True if the update was downloaded and ran successfully, else false</returns>
    Task<bool> WindowsUpdate(Version version, IProgress<DownloadProgress>? progress = null);
#endif
}
