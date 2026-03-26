using Markdig;
using System;
using System.Collections.Generic;

namespace Nickvision.Desktop.Application;

public class AppInfo
{
    public Dictionary<string, string> Artists { get; }

    public string? Changelog { get; set; }

    public string? Description { get; set; }

    public Dictionary<string, string> Designers { get; }

    public Dictionary<string, string> Developers { get; }

    public Uri? DiscussionsForum { get; set; }

    public Uri? DocumentationStore { get; set; }

    public string EnglishShortName { get; init; }

    public Dictionary<string, Uri> ExtraLinks { get; }

    public string Id { get; init; }

    public Uri? IssueTracker { get; set; }

    public string Name { get; init; }

    public string? ShortName { get; set; }

    public Uri? SourceRepository { get; set; }

    public string? TranslationCredits { get; set; }

    public AppVersion? Version { get; set; }
    public bool IsPortable { get; set; }

    public AppInfo(string id, string name, string englishShortName)
    {
        Id = id;
        Name = name;
        EnglishShortName = englishShortName;
        ExtraLinks = [];
        Developers = [];
        Designers = [];
        Artists = [];
        IsPortable = false;
    }

    public string? HtmlChangelog => string.IsNullOrEmpty(Changelog) ? null : Markdown.ToHtml(Changelog.Trim());
}
