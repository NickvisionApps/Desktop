using GetText;
using Nickvision.Desktop.Application;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
#if OS_LINUX
using Nickvision.Desktop.System;
#endif

namespace Nickvision.Desktop.Globalization;

public class TranslationService : ITranslationService
{
    private readonly AppInfo _appInfo;
    private Catalog? _catalog;
    private string _language;

    public TranslationService(AppInfo appInfo, string language = "")
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
            _catalog = !AvailableLanguages.Contains(_language)
                ? new Catalog(_appInfo.EnglishShortName)
                : _catalog = new Catalog(_appInfo.EnglishShortName, new CultureInfo(language));
        }
    }

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
                _catalog = !AvailableLanguages.Contains(_language)
                    ? new Catalog(_appInfo.EnglishShortName)
                    : _catalog = new Catalog(_appInfo.EnglishShortName, new CultureInfo(_language));
            }
        }
    }

    public IEnumerable<string> AvailableLanguages
    {
        get
        {
            var languages = new List<string>();
            foreach (var directory in Directory.EnumerateDirectories(Directory.GetCurrentDirectory()))
            {
                if (File.Exists(Path.Combine(directory, $"{_appInfo.EnglishShortName}.mo")))
                {
                    languages.Add(new DirectoryInfo(directory).Name);
                }
            }
            return languages;
        }
    }

    public string _(string text) => _catalog?.GetString(text) ?? text;

    public string _(string text, params object[] args) => _catalog?.GetString(text, args) ?? text;

    public string _n(string text, string pluralText, long n) =>
        _catalog?.GetPluralString(text, pluralText, n) ?? (n == 1 ? text : pluralText);

    public string _n(string text, string pluralText, long n, params object[] args) =>
        _catalog?.GetPluralString(text, pluralText, n, args) ?? (n == 1 ? text : pluralText);

    public string _p(string context, string text) => _catalog?.GetParticularString(context, text) ?? text;

    public string _p(string context, string text, params object[] args) =>
        _catalog?.GetParticularString(context, text, args) ?? text;

    public string _pn(string context, string text, string pluralText, long n) =>
        _catalog?.GetParticularPluralString(context, text, pluralText, n) ?? (n == 1 ? text : pluralText);

    public string _pn(string context, string text, string pluralText, long n, params object[] args) =>
        _catalog?.GetParticularPluralString(context, text, pluralText, n, args) ?? (n == 1 ? text : pluralText);

    public Uri GetHelpUrl(string pageName)
    {
#if OS_LINUX
        if (Nickvision.Desktop.System.Environment.DeploymentMode == DeploymentMode.Flatpak)
        {
            return new Uri($"help:{_appInfo.EnglishShortName.ToLower()}/{pageName}");
        }
#endif
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
