using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nickvision.Desktop.Filesystem;
using Nickvision.Desktop.Helpers;
using Nickvision.Desktop.Network;
using Octokit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using FileMode = System.IO.FileMode;

namespace Nickvision.Desktop.Application;

/// <summary>
/// A service for updating an application via GitHub releases.
/// </summary>
public class UpdaterService : IDisposable, IUpdaterService
{
    private readonly ILogger<UpdaterService> _logger;
    private readonly SHA256 _hasher;
    private readonly GitHubClient _githubClient;
    private readonly HttpClient _httpClient;
    private readonly string _owner;
    private readonly string _name;
    private readonly string _cacheReleasesPath;

    /// <summary>
    /// Constructs an UpdaterService.
    /// </summary>
    /// <param name="logger">Logger for the service</param>
    /// <param name="appInfo">The AppInfo object for the app</param>
    /// <param name="httpClient">The HttpClient for the app</param>
    /// <exception cref="ArgumentException">Thrown if the AppInfo.SourceRepository is missing or ill-formated</exception>
    [ActivatorUtilitiesConstructor]
    public UpdaterService(ILogger<UpdaterService> logger, AppInfo appInfo, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _hasher = SHA256.Create();
        if (appInfo.SourceRepository is null || appInfo.SourceRepository.IsEmpty)
        {
            _logger.LogCritical("AppInfo.SourceRepository is null or empty.");
            throw new ArgumentException("AppInfo.SourceRepository cannot be null or empty");
        }
        _httpClient = httpClientFactory.CreateClient();
        _httpClient.DefaultRequestHeaders.UserAgent.Add(new global::System.Net.Http.Headers.ProductInfoHeaderValue("NickvisionDesktop", "1.0"));
        _githubClient = new GitHubClient(new ProductHeaderValue("Nickvision.Desktop"));
        try
        {
            var fields = appInfo.SourceRepository.ToString().Split("/");
            _owner = fields[3];
            _name = fields[4];
        }
        catch (Exception e)
        {
            _logger.LogCritical(e, $"AppInfo.SourceRepository ({appInfo.SourceRepository}) is ill-formated.");
            throw new ArgumentException("AppInfo.SourceRepository is ill-formated", e);
        }
        _cacheReleasesPath = Path.Combine(UserDirectories.Cache, "Nickvision", $"{_owner}-{_name}-releases.json");
        Directory.CreateDirectory(Path.GetDirectoryName(_cacheReleasesPath)!);
    }

    /// <summary>
    /// Constructs an UpdaterService.
    /// </summary>
    /// <param name="logger">Logger for the service</param>
    /// <param name="owner">The repository owner</param>
    /// <param name="name">The repository name</param>
    /// <param name="httpClient">The HttpClient for the app</param>
    /// <exception cref="ArgumentException">Thrown if the Owner and/or Name are empty</exception>
    public UpdaterService(ILogger<UpdaterService> logger, string owner, string name, HttpClient httpClient)
    {
        if (string.IsNullOrEmpty(owner) || string.IsNullOrEmpty(name))
        {
            throw new ArgumentException("Owner and Name cannot be null or empty");
        }
        _logger = logger;
        _hasher = SHA256.Create();
        _owner = owner;
        _name = name;
        _httpClient = httpClient;
        _httpClient.DefaultRequestHeaders.UserAgent.Add(new global::System.Net.Http.Headers.ProductInfoHeaderValue("NickvisionDesktop", "1.0"));
        _githubClient = new GitHubClient(new ProductHeaderValue("Nickvision.Desktop"));
        _cacheReleasesPath = Path.Combine(UserDirectories.Cache, $"{_owner}-{_name}-releases.json");
    }

    /// <summary>
    /// Destructs an UpdaterService. 
    /// </summary>
    ~UpdaterService()
    {
        Dispose(false);
    }

    /// <summary>
    /// Frees resources used by the UpdaterService.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Downloads an asset from a released version.
    /// </summary>
    /// <param name="version">The released version</param>
    /// <param name="path">The path of where to download the asset to</param>
    /// <param name="assertName">The name of the asset to download</param>
    /// <param name="exactMatch">Whether the asset name should match exactly to the asset to download</param>
    /// <param name="progress">An optional progress reporter</param>
    /// <returns></returns>
    public async Task<bool> DownloadReleaseAssetAsync(AppVersion version, string path, string assertName, bool exactMatch = true, IProgress<DownloadProgress>? progress = null)
    {
        _logger.LogInformation($"Starting download of asset ({assertName}{(exactMatch ? string.Empty : "*")}) for {_owner}/{_name} version {version}...");
        foreach (var release in await GetReleasesAsync())
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
                    _logger.LogInformation($"Skipping asset {asset.Name} as it does not match the requested name ({assertName}{(exactMatch ? string.Empty : "*")}).");
                    continue;
                }
                try
                {
                    _logger.LogInformation($"Downloading asset {asset.Name} from {asset.BrowserDownloadUrl} to {path}...");
                    using var response = await _httpClient.GetAsync(asset.BrowserDownloadUrl, HttpCompletionOption.ResponseHeadersRead);
                    response.EnsureSuccessStatusCode();
                    var totalBytesToRead = response.Content.Headers.ContentLength ?? 0L;
                    var totalBytesRead = 0L;
                    var bytesSinceLastReport = 0L;
                    var buffer = new byte[81920];
                    await using var downloadStream = await response.Content.ReadAsStreamAsync();
                    await using var fileStream = new FileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.None, 8192, true);
                    while (true)
                    {
                        var bytesRead = await downloadStream.ReadAsync(buffer, 0, buffer.Length);
                        if (bytesRead == 0)
                        {
                            fileStream.Seek(0, SeekOrigin.Begin);
                            progress?.Report(new DownloadProgress(totalBytesToRead, totalBytesRead, true));
                            var assetWithDigest = await _httpClient.GetFromJsonAsync(asset.Url, UpdaterServiceJsonContext.Default.GitHubReleaseAsset);
                            if(assetWithDigest is null)
                            {
                                _logger.LogError($"Failed to get asset information for {asset.Name} from GitHub API.");
                                return false;
                            }
                            var expectedHash = assetWithDigest.Digest.Replace("sha256:", string.Empty);
                            var actualHash = string.Join(null, (await _hasher.ComputeHashAsync(fileStream)).Select(x => x.ToString("x2")));
                            if (expectedHash == actualHash)
                            {
                                _logger.LogInformation($"Downloaded asset {asset.Name} to {path} successfully.");
                                return true;
                            }
                            else
                            {
                                _logger.LogError($"Error in downloaded asset ({asset.Name}). File size does not match expected size (expected: {expectedHash}, actual: {actualHash}).");
                                return false;
                            }
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
                catch (Exception e)
                {
                    _logger.LogError($"Failed to download asset {asset.Name} from {asset.BrowserDownloadUrl}: {e}");
                    return false;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// Gets the latest preview version available.
    /// </summary>
    /// <returns>The latest preview version or null if unavailable</returns>
    public async Task<AppVersion?> GetLatestPreviewVersionAsync()
    {
        var releases = await GetReleasesAsync();
        foreach (var release in releases.Where(r => !string.IsNullOrEmpty(r.TagName) && r.Prerelease && !r.Draft))
        {
            if (!AppVersion.TryParse(release.TagName, out var version))
            {
                continue;
            }
            return version;
        }
        return null;
    }

    /// <summary>
    /// Gets the latest stable version available.
    /// </summary>
    /// <returns>The latest stable version or null if unavailable</returns>
    public async Task<AppVersion?> GetLatestStableVersionAsync()
    {
        var releases = await GetReleasesAsync();
        foreach (var release in releases.Where(r => !string.IsNullOrEmpty(r.TagName) && !r.Prerelease && !r.Draft))
        {
            if (!AppVersion.TryParse(release.TagName, out var version))
            {
                continue;
            }
            return version;
        }
        return null;
    }

    /// <summary>
    /// Downloads and runs the updated Windows installer of the given released version.
    /// </summary>
    /// <param name="version">The released version</param>
    /// <param name="progress">An optional progress reporter</param>
    /// <returns>True if the update was downloaded and ran successfully, else false</returns>
    public async Task<bool> WindowsApplicationUpdateAsync(AppVersion version, IProgress<DownloadProgress>? progress = null)
    {
        _logger.LogInformation($"Starting Windows application update for {_owner}/{_name} version {version}...");
        if (!OperatingSystem.IsWindows())
        {
            _logger.LogError($"Unable to perform update as system is not Windows.");
            return false;
        }
        var setupPath = Path.Combine(UserDirectories.Cache, $"{_owner}_{_name}_Setup.exe");
        if (RuntimeInformation.ProcessArchitecture == Architecture.Arm64)
        {
            _logger.LogInformation($"Downloading ARM64 installer...");
            if (!await DownloadReleaseAssetAsync(version, setupPath, "setup-arm64.exe", false, progress))
            {
                if (!await DownloadReleaseAssetAsync(version, setupPath, "setup.exe", false, progress))
                {
                    _logger.LogError($"Failed to download ARM64 installer.");
                    return false;
                }
            }
        }
        else
        {
            _logger.LogInformation($"Downloading x64 installer...");
            if (!await DownloadReleaseAssetAsync(version, setupPath, "setup-x64.exe", false, progress))
            {
                if (!await DownloadReleaseAssetAsync(version, setupPath, "setup.exe", false, progress))
                {
                    _logger.LogError($"Failed to download x64 installer.");
                    return false;
                }
            }
        }
        _logger.LogInformation($"Starting downloaded installer ({setupPath})...");
        var res = Process.Start(new ProcessStartInfo
        {
            FileName = setupPath,
            UseShellExecute = true,
            Verb = "open"
        });
        if (res is null)
        {
            _logger.LogError($"Failed to start installer process.");
            return false;
        }
        _logger.LogInformation($"Started downloaded installer. Update complete.");
        return true;
    }

    private void Dispose(bool disposing)
    {
        if(!disposing)
        {
            return;
        }
        _hasher.Dispose();
    }

    private async Task<IReadOnlyList<GitHubRelease>> GetReleasesAsync()
    {
        _logger.LogInformation($"Fetching all releases for {_owner}/{_name}...");
        try
        {
            _logger.LogInformation($"Checking for releases in cache ({_cacheReleasesPath})...");
            if (File.Exists(_cacheReleasesPath) && DateTime.UtcNow - File.GetLastAccessTimeUtc(_cacheReleasesPath) > TimeSpan.FromHours(6))
            {
                File.Delete(_cacheReleasesPath);
                _logger.LogWarning($"Deleted cache file as it is older than 6 hours.");
            }
            IReadOnlyList<GitHubRelease> releases = [];
            if (File.Exists(_cacheReleasesPath))
            {
                _logger.LogInformation($"Cache file found, loading releases from cache...");
                releases = JsonSerializer.Deserialize(await File.ReadAllTextAsync(_cacheReleasesPath), UpdaterServiceJsonContext.Default.ListGitHubRelease) ?? [];
                _logger.LogInformation($"Loaded {releases.Count} releases from cache.");
            }
            if (releases.Count == 0)
            {
                _logger.LogInformation($"No releases found in cache, fetching from GitHub API...");
                var octokitReleases = await _githubClient.Repository.Release.GetAll(_owner, _name);
                var mappedReleases = octokitReleases.Select(r => new GitHubRelease
                {
                    TagName = r.TagName,
                    Prerelease = r.Prerelease,
                    Draft = r.Draft,
                    Assets = r.Assets?.Select(a => new GitHubReleaseAsset
                    {
                        Url = a.Url,
                        Name = a.Name,
                        Size = a.Size,
                        BrowserDownloadUrl = a.BrowserDownloadUrl
                    }).ToList() ?? new List<GitHubReleaseAsset>()
                }).ToList();
                var json = JsonSerializer.Serialize(mappedReleases, UpdaterServiceJsonContext.Default.ListGitHubRelease);
                await File.WriteAllTextAsync(_cacheReleasesPath, json);
                File.SetLastWriteTimeUtc(_cacheReleasesPath, DateTime.UtcNow);
                releases = mappedReleases;
                _logger.LogInformation($"Fetched {releases.Count} releases from GitHub API and saved to cache.");
            }
            return releases;
        }
        catch (Exception e)
        {
            _logger.LogError($"Failed to fetch releases for {_owner}/{_name}: {e}");
            return [];
        }
    }
}

internal class GitHubRelease
{
    public string TagName { get; set; }
    public bool Prerelease { get; set; }
    public bool Draft { get; set; }
    public List<GitHubReleaseAsset> Assets { get; set; }

    public GitHubRelease()
    {
        TagName = string.Empty;
        Prerelease = false;
        Draft = false;
        Assets = new List<GitHubReleaseAsset>();
    }
}

internal class GitHubReleaseAsset
{
    public string Url { get; set; }
    public string Name { get; set; }
    public long Size { get; set; }
    public string Digest { get; set; }
    public string BrowserDownloadUrl { get; set; }

    public GitHubReleaseAsset()
    {
        Url = string.Empty;
        Name = string.Empty;
        Size = 0;
        Digest = string.Empty;
        BrowserDownloadUrl = string.Empty;
    }
}

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower)]
[JsonSerializable(typeof(List<GitHubRelease>))]
[JsonSerializable(typeof(GitHubReleaseAsset))]
internal partial class UpdaterServiceJsonContext : JsonSerializerContext { }