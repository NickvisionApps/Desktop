namespace Nickvision.Desktop.Helpers;

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
