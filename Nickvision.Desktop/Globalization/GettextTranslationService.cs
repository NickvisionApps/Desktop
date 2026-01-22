using GetText;
using Nickvision.Desktop.Application;
using Nickvision.Desktop.System;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Nickvision.Desktop.Globalization;

/// <summary>
///     A service for translations using Gettext.
/// </summary>
public class GettextTranslationService : ITranslationService
{
    private readonly AppInfo _appInfo;
    private Catalog? _catalog;
    private string _language;

    /// <summary>
    ///     Constructs a TranslationService.
    /// </summary>
    /// <param name="appInfo">The AppInfo object for the app</param>
    /// <param name="language">The language code to use for translations</param>
    /// <remarks>
    ///     An empty string language code will use the system's language code for translations. The language code "C" will
    ///     cause strings to remain untranslated
    /// </remarks>
    public GettextTranslationService(AppInfo appInfo, string language = "")
    {
        _appInfo = appInfo;
        _language = language;
        if (string.IsNullOrEmpty(_language))
        {
            _catalog = new Catalog(_appInfo.EnglishShortName);
        }
        else if (_language == "C")
        {
            _catalog = null;
        }
        else
        {
            _catalog = !AvailableLanguages.Contains(_language) ? new Catalog(_appInfo.EnglishShortName) : _catalog = new Catalog(_appInfo.EnglishShortName, new CultureInfo(language));
        }
    }

    /// <summary>
    ///     The language code for translations.
    /// </summary>
    /// <remarks>
    ///     An empty string will use the system's language code for translations. The language code "C" will cause strings
    ///     to remain untranslated
    /// </remarks>
    public string Language
    {
        get => _language;

        set
        {
            _language = value;
            if (string.IsNullOrEmpty(_language))
            {
                _catalog = new Catalog(_appInfo.EnglishShortName);
            }
            else if (_language == "C")
            {
                _catalog = null;
            }
            else
            {
                _catalog = !AvailableLanguages.Contains(_language) ? new Catalog(_appInfo.EnglishShortName) : _catalog = new Catalog(_appInfo.EnglishShortName, new CultureInfo(_language));
            }
        }
    }

    /// <summary>
    ///     The list of available language codes for translations.
    /// </summary>
    public IEnumerable<string> AvailableLanguages
    {
        get
        {
            var languages = new List<string>();
            foreach (var directory in Directory.EnumerateDirectories(System.Environment.ExecutingDirectory))
            {
                if (File.Exists(Path.Combine(directory, $"{_appInfo.EnglishShortName}.mo")))
                {
                    languages.Add(new DirectoryInfo(directory).Name);
                }
            }
            return languages;
        }
    }

    /// <summary>
    ///     Translates a string.
    /// </summary>
    /// <param name="text">The string to translate.</param>
    /// <returns>The translated string</returns>
    public string _(string text) => _catalog?.GetString(text) ?? text;

    /// <summary>
    ///     Translates a format string.
    /// </summary>
    /// <param name="text">The format string to translate.</param>
    /// <param name="args">The arguments for the format string</param>
    /// <returns>The translated format string</returns>
    public string _(string text, params object[] args) => _catalog?.GetString(text, args) ?? text;

    /// <summary>
    ///     Translates a possible plural string.
    /// </summary>
    /// <param name="text">The non-plural string to translate</param>
    /// <param name="pluralText">The plural string to translate</param>
    /// <param name="n">The number of objects</param>
    /// <returns>The translated plural string if n != 1, else the translated non-plural string</returns>
    public string _n(string text, string pluralText, long n) => _catalog?.GetPluralString(text, pluralText, n) ?? (n == 1 ? text : pluralText);

    /// <summary>
    ///     Translates a possible plural format string.
    /// </summary>
    /// <param name="text">The non-plural format string to translate</param>
    /// <param name="pluralText">The plural format string to translate</param>
    /// <param name="n">The number of objects</param>
    /// <param name="args">The arguments for the format string</param>
    /// <returns>The translated format plural string if n != 1, else the translated non-plural format string</returns>
    public string _n(string text, string pluralText, long n, params object[] args) => _catalog?.GetPluralString(text, pluralText, n, args) ?? (n == 1 ? text : pluralText);

    /// <summary>
    ///     Translates a string for a particular context.
    /// </summary>
    /// <param name="context">The context of the string</param>
    /// <param name="text">The string to translate</param>
    /// <returns>The translated string for the context</returns>
    public string _p(string context, string text) => _catalog?.GetParticularString(context, text) ?? text;

    /// <summary>
    ///     Translates a format string for a particular context.
    /// </summary>
    /// <param name="context">The context of the string</param>
    /// <param name="text">The format string to translate</param>
    /// <param name="args">The arguments for the format string</param>
    /// <returns>The translated format string for the context</returns>
    public string _p(string context, string text, params object[] args) => _catalog?.GetParticularString(context, text, args) ?? text;

    /// <summary>
    ///     Translates a possible plural string for a particular context.
    /// </summary>
    /// <param name="context">The context of the string</param>
    /// <param name="text">The non-plural string to translate</param>
    /// <param name="pluralText">The plural string to translate</param>
    /// <param name="n">The number of objects</param>
    /// <returns>The translated plural string if n != 1, else the translated non-plural string for the context</returns>
    public string _pn(string context, string text, string pluralText, long n) => _catalog?.GetParticularPluralString(context, text, pluralText, n) ?? (n == 1 ? text : pluralText);

    /// <summary>
    ///     Translates a possible plural format string for a particular context.
    /// </summary>
    /// <param name="context">The context of the string</param>
    /// <param name="text">The non-plural format string to translate</param>
    /// <param name="pluralText">The plural format string to translate</param>
    /// <param name="n">The number of objects</param>
    /// <param name="args">The arguments for the format string</param>
    /// <returns>The translated plural format string if n != 1, else the translated non-plural format string for the context</returns>
    public string _pn(string context, string text, string pluralText, long n, params object[] args) => _catalog?.GetParticularPluralString(context, text, pluralText, n, args) ?? (n == 1 ? text : pluralText);

    /// <summary>
    ///     Gets the localized help page url for a given page name.
    /// </summary>
    /// <param name="pageName">The name of the help page</param>
    /// <returns>The help page url for the current system locale</returns>
    public Uri GetHelpUrl(string pageName)
    {
        if (OperatingSystem.IsLinux())
        {
            if (System.Environment.DeploymentMode == DeploymentMode.Flatpak)
            {
                return new Uri($"help:{_appInfo.EnglishShortName.ToLower()}/{pageName}");
            }
        }
        var lang = "C";
        var sysLocale = CultureInfo.CurrentCulture.Name.Replace('-', '_');
        if (!string.IsNullOrEmpty(sysLocale) && sysLocale != "C" && sysLocale != "en_US" && sysLocale != "*")
        {
            var twoLetter = sysLocale.Split('_')[0];
            foreach (var language in AvailableLanguages)
            {
                if (language != sysLocale && language != twoLetter)
                {
                    continue;
                }
                lang = language;
                break;
            }
        }
        return new Uri($"https://htmlpreview.github.io/?{_appInfo.DocumentationStore}/{lang}/{pageName}.html");
    }
}
