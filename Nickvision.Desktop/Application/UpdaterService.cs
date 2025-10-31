using Nickvision.Desktop.Filesystem;
using Nickvision.Desktop.Helpers;
using Nickvision.Desktop.Network;
using Octokit;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using FileMode = System.IO.FileMode;

namespace Nickvision.Desktop.Application;

public class UpdaterService : IUpdaterService
{
    private readonly GitHubClient _githubClient;
    private readonly HttpClient _httpClient;
    private readonly string _name;
    private readonly string _owner;

    public UpdaterService(AppInfo appInfo, HttpClient httpClient)
    {
        if (appInfo.SourceRepository is null ||
            appInfo.SourceRepository.IsEmpty())
        {
            throw new ArgumentException("AppInfo.SourceRepository cannot be null");
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

    public async Task<bool> DownloadReleaseAssetAsync(Version version,
        string path,
        string assertName,
        bool exactMatch = true,
        IProgress<DownloadProgress>? progress = null)
    {
        var releases = await _githubClient.Repository.Release.GetAll(_owner, _name);
        foreach (var release in releases)
        {
            if (!Version.TryParse(release.TagName.TrimStart('v'), out var releaseVersion))
            {
                continue;
            }
            if (version != releaseVersion)
            {
                continue;
            }
            foreach (var asset in release.Assets)
            {
                if ((!exactMatch || asset.Name.ToLower() != assertName.ToLower()) &&
                    (exactMatch || !asset.Name.ToLower().Contains(assertName.ToLower())))
                {
                    continue;
                }
                try
                {
                    using var response = await _httpClient.GetAsync(asset.BrowserDownloadUrl, HttpCompletionOption.ResponseHeadersRead);
                    response.EnsureSuccessStatusCode();
                    var totalBytesToRead = response.Content.Headers.ContentLength ?? -1L;
                    var totalBytesRead = 0L;
                    var buffer = new byte[81920];
                    await using var downloadStream = await response.Content.ReadAsStreamAsync();
                    await using var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);
                    while (true)
                    {
                        var bytesRead = await downloadStream.ReadAsync(buffer);
                        if (bytesRead == 0)
                        {
                            progress?.Report(new DownloadProgress(totalBytesToRead, totalBytesRead, true));
                            return true;
                        }
                        await fileStream.WriteAsync(buffer);
                        totalBytesRead += bytesRead;
                        progress?.Report(new DownloadProgress(totalBytesToRead, totalBytesRead, false));
                    }
                }
                catch
                {
                    return false;
                }
            }
        }
        return false;
    }

    public async Task<Version?> GetLatestPreviewVersionAsync()
    {
        var releases = await _githubClient.Repository.Release.GetAll(_owner, _name);
        foreach (var release in releases)
        {
            if (!release.Prerelease ||
                release.Draft)
            {
                continue;
            }
            if (!Version.TryParse(release.TagName.TrimStart('v'), out var version))
            {
                continue;
            }
            return version;
        }
        return null;
    }

    public async Task<Version?> GetLatestStableVersionAsync()
    {
        var releases = await _githubClient.Repository.Release.GetAll(_owner, _name);
        foreach (var release in releases)
        {
            if (release.Prerelease ||
                release.Draft)
            {
                continue;
            }
            if (!Version.TryParse(release.TagName.TrimStart('v'), out var version))
            {
                continue;
            }
            return version;
        }
        return null;
    }

    public async Task<bool> WindowsUpdate(Version version, IProgress<DownloadProgress>? progress = null)
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
