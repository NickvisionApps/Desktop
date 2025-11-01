using Markdig;
using System;
using System.Collections.Generic;

namespace Nickvision.Desktop.Application;

/// <summary>
/// A class containing information about an application.
/// </summary>
public class AppInfo
{
    /// <summary>
    /// Constructs an AppInfo.
    /// </summary>
    /// <param name="id">The id of the app</param>
    /// <param name="name">The name of the app</param>
    /// <param name="englishShortName">The short name of the app in English (untranslated)</param>
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

    /// <summary>
    /// The id of the app.
    /// </summary>
    public string Id { get; init; }
    /// <summary>
    /// The name of the app.
    /// </summary>
    public string Name { get; init; }
    /// <summary>
    /// The short name of the app in English (untranslated).
    /// </summary>
    public string EnglishShortName { get; init; }
    /// <summary>
    /// The short name of the app (translated).
    /// </summary>
    public string? ShortName { get; set; }
    /// <summary>
    /// The description of the app.
    /// </summary>
    public string? Description { get; set; }
    /// <summary>
    /// The current running version of the app.
    /// </summary>
    public Version? Version { get; set; }
    /// <summary>
    /// The changelog of the app in Markdown format.
    /// </summary>
    public string? Changelog { get; set; }
    /// <summary>
    /// The url to the source repository of the app.
    /// </summary>
    public Uri? SourceRepository { get; set; }
    /// <summary>
    /// The url to the issue tracker of the app.
    /// </summary>
    public Uri? IssueTracker { get; set; }
    /// <summary>
    /// The url to the discussions forum of the app.
    /// </summary>
    public Uri? DiscussionsForum { get; set; }
    /// <summary>
    /// The url to the documentation store of the app.
    /// </summary>
    public Uri? DocumentationStore { get; set; }
    /// <summary>
    /// A map of extra links' names and their urls related to the app.
    /// </summary>
    public Dictionary<string, Uri> ExtraLinks { get; }
    /// <summary>
    /// A map of developers' names and their emails related to the app.
    /// </summary>
    public Dictionary<string, string> Developers { get; }
    /// <summary>
    /// A map of designers' names and their emails related to the app.
    /// </summary>
    public Dictionary<string, string> Designers { get; }
    /// <summary>
    /// A map of artists' names and their emails related to the app.
    /// </summary>
    public Dictionary<string, string> Artists { get; }
    /// <summary>
    /// The translation credits for the app.
    /// </summary>
    public string? TranslationCredits { get; set; }

    /// <summary>
    /// The changelog of the app in HTML format.
    /// </summary>
    public string? HtmlChangelog => string.IsNullOrEmpty(Changelog) ? null : Markdown.ToHtml(Changelog.Trim());
}
