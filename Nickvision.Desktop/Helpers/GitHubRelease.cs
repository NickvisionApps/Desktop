using System.Collections.Generic;

namespace Nickvision.Desktop.Helpers;

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
