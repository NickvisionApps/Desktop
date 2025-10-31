using Markdig;
using System;
using System.Collections.Generic;

namespace Nickvision.Desktop.Application;

public class AppInfo
{
    public AppInfo(string id, string name, string englishShortName)
    {
        Id = id;
        Name = name;
        EnglishShortName = englishShortName;
        ExtraLinks = [];
        Developers = [];
        Designers = [];
        Artists = [];
    }

    public string Id { get; init; }
    public string Name { get; init; }
    public string EnglishShortName { get; init; }
    public string? ShortName { get; set; }
    public string? Description { get; set; }
    public Version? Version { get; set; }
    public string? Changelog { get; set; }
    public Uri? SourceRepository { get; set; }
    public Uri? IssueTracker { get; set; }
    public Uri? DiscussionsForm { get; set; }
    public Uri? DocumentationStore { get; set; }
    public Dictionary<string, Uri> ExtraLinks { get; }
    public Dictionary<string, string> Developers { get; }
    public Dictionary<string, string> Designers { get; }
    public Dictionary<string, string> Artists { get; }
    public string? TranslationCredits { get; set; }

    public string? HtmlChangelog => string.IsNullOrEmpty(Changelog) ? null : Markdown.ToHtml(Changelog.Trim());
}
