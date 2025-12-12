using Nickvision.Desktop.Filesystem;
using Nickvision.Desktop.Helpers;
using Nickvision.Desktop.Network;
using Octokit;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using FileMode = System.IO.FileMode;

namespace Nickvision.Desktop.Application;

/// <summary>
///     A service for updating an application via GitHub releases.
/// </summary>
public class GitHubUpdaterService : IUpdaterService
{
    private readonly GitHubClient _githubClient;
    private readonly HttpClient _httpClient;
    private readonly string _name;
    private readonly string _owner;

    /// <summary>
    ///     Constructs an UpdaterService.
    /// </summary>
    /// <param name="appInfo">The AppInfo object for the app</param>
    /// <param name="httpClient">The HttpClient for the app</param>
    /// <exception cref="ArgumentException">Thrown if the AppInfo.SourceRepository is missing or ill-formated</exception>
    public GitHubUpdaterService(AppInfo appInfo, HttpClient httpClient)
    {
        if (appInfo.SourceRepository is null || appInfo.SourceRepository.IsEmpty)
        {
            throw new ArgumentException("AppInfo.SourceRepository cannot be null or empty");
        }
        _httpClient = httpClient;
        _githubClient = new GitHubClient(new ProductHeaderValue("Nickvision.Desktop"));
        try
        {
            var fields = appInfo.SourceRepository.ToString().Split("/");
            _owner = fields[3];
            _name = fields[4];
        }
        catch (Exception e)
        {
            throw new ArgumentException("AppInfo.SourceRepository is ill-formated", e);
        }
    }

    /// <summary>
    ///     Constructs an UpdaterService.
    /// </summary>
    /// <param name="owner">The repository owner</param>
    /// <param name="name">The repository name</param>
    /// <param name="httpClient">The HttpClient for the app</param>
    /// <exception cref="ArgumentException">Thrown if the Owner and/or Name are empty</exception>
    public GitHubUpdaterService(string owner, string name, HttpClient httpClient)
    {
        if (string.IsNullOrEmpty(owner) || string.IsNullOrEmpty(name))
        {
            throw new ArgumentException("Owner and Name cannot be null or empty");
        }
        _owner = owner;
        _name = name;
        _httpClient = httpClient;
        _githubClient = new GitHubClient(new ProductHeaderValue("Nickvision.Desktop"));
    }

    /// <summary>
    ///     Downloads an asset from a released version.
    /// </summary>
    /// <param name="version">The released version</param>
    /// <param name="path">The path of where to download the asset to</param>
    /// <param name="assertName">The name of the asset to download</param>
    /// <param name="exactMatch">Whether the asset name should match exactly to the asset to download</param>
    /// <param name="progress">An optional progress reporter</param>
    /// <returns></returns>
    public async Task<bool> DownloadReleaseAssetAsync(AppVersion version, string path, string assertName, bool exactMatch = true, IProgress<DownloadProgress>? progress = null)
    {
        try
        {
            var releases = await _githubClient.Repository.Release.GetAll(_owner, _name);
            foreach (var release in releases)
            {
                if (!AppVersion.TryParse(release.TagName.TrimStart('v'), out var releaseVersion))
                {
                    continue;
                }
                if (version != releaseVersion)
                {
                    continue;
                }
                foreach (var asset in release.Assets)
                {
                    if ((!exactMatch || asset.Name.ToLower() != assertName.ToLower()) && (exactMatch || !asset.Name.ToLower().Contains(assertName.ToLower())))
                    {
                        continue;
                    }
                    try
                    {
                        using var response = await _httpClient.GetAsync(asset.BrowserDownloadUrl, HttpCompletionOption.ResponseHeadersRead);
                        response.EnsureSuccessStatusCode();
                        var totalBytesToRead = response.Content.Headers.ContentLength ?? 0L;
                        var totalBytesRead = 0L;
                        var bytesSinceLastReport = 0L;
                        var buffer = new byte[81920];
                        await using var downloadStream = await response.Content.ReadAsStreamAsync();
                        await using var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);
                        while (true)
                        {
                            var bytesRead = await downloadStream.ReadAsync(buffer, 0, buffer.Length);
                            if (bytesRead == 0)
                            {
                                progress?.Report(new DownloadProgress(totalBytesToRead, totalBytesRead, true));
                                return new FileInfo(path).Length == asset.Size;
                            }
                            await fileStream.WriteAsync(buffer, 0, bytesRead);
                            totalBytesRead += bytesRead;
                            bytesSinceLastReport += bytesRead;
                            if (bytesSinceLastReport >= 524288)
                            {
                                progress?.Report(new DownloadProgress(totalBytesToRead, totalBytesRead, false));
                                bytesSinceLastReport = 0;
                            }
                        }
                    }
                    catch
                    {
                        return false;
                    }
                }
            }
        }
        catch (Exception)
        {
            return false;
        }
        return false;
    }

    /// <summary>
    ///     Gets the latest preview version available.
    /// </summary>
    /// <returns>The latest preview version or null if unavailable</returns>
    public async Task<AppVersion?> GetLatestPreviewVersionAsync()
    {
        try
        {
            var releases = await _githubClient.Repository.Release.GetAll(_owner, _name);
            foreach (var release in releases.Where(r => r.Prerelease && !r.Draft))
            {
                if (!AppVersion.TryParse(release.TagName, out var version))
                {
                    continue;
                }
                return version;
            }
            return null;
        }
        catch (Exception)
        {
            return null;
        }
    }

    /// <summary>
    ///     Gets the latest stable version available.
    /// </summary>
    /// <returns>The latest stable version or null if unavailable</returns>
    public async Task<AppVersion?> GetLatestStableVersionAsync()
    {
        try
        {
            var releases = await _githubClient.Repository.Release.GetAll(_owner, _name);
            foreach (var release in releases.Where(r => !r.Prerelease && !r.Draft))
            {
                if (!AppVersion.TryParse(release.TagName, out var version))
                {
                    continue;
                }
                return version;
            }
            return null;
        }
        catch (Exception)
        {
            return null;
        }
    }

    /// <summary>
    ///     Downloads and runs the updated Windows installer of the given released version.
    /// </summary>
    /// <param name="version">The released version</param>
    /// <param name="progress">An optional progress reporter</param>
    /// <returns>True if the update was downloaded and ran successfully, else false</returns>
    public async Task<bool> WindowsUpdate(AppVersion version, IProgress<DownloadProgress>? progress = null)
    {
        var setupPath = Path.Combine(UserDirectories.Cache, $"{_owner}_{_name}_Setup.exe");
        if (!await DownloadReleaseAssetAsync(version, setupPath, "setup.exe", false, progress))
        {
            return false;
        }
        Process.Start(new ProcessStartInfo
        {
            FileName = setupPath,
            UseShellExecute = true,
            Verb = "open"
        });
        return true;
    }
}
