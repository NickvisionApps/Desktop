using GetText;
using Nickvision.Desktop.Application;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Nickvision.Desktop.Globalization;

public class TranslationService : ITranslationService
{
    private readonly string _domainName;
    private Catalog? _catalog;

    public TranslationService(AppInfo appInfo)
    {
        _domainName = appInfo.EnglishShortName.Replace(" ", "").ToLower();
        Language = "C";
    }

    public string Language
    {
        get => field;

        set
        {
            field = value;
            if (string.IsNullOrEmpty(field))
            {
                _catalog = new Catalog(_domainName, System.Environment.ExecutingDirectory);
            }
            else if (field == "C")
            {
                _catalog = null;
            }
            else
            {
                _catalog = !AvailableLanguages.Contains(field) ? new Catalog(_domainName, System.Environment.ExecutingDirectory) : _catalog = new Catalog(_domainName, System.Environment.ExecutingDirectory, new CultureInfo(field));
            }
        }
    }

    public IEnumerable<string> AvailableLanguages
    {
        get
        {
            var languages = new List<string>();
            foreach (var directory in Directory.EnumerateDirectories(System.Environment.ExecutingDirectory))
            {
                if (File.Exists(Path.Combine(directory, $"{_domainName}.mo")))
                {
                    languages.Add(new DirectoryInfo(directory).Name);
                }
            }
            return languages;
        }
    }

    public string _(string text) => _catalog?.GetString(text) ?? text;

    public string _(string text, params object[] args) => _catalog?.GetString(text, args) ?? text;

    public string _n(string text, string pluralText, long n) => _catalog?.GetPluralString(text, pluralText, n) ?? (n == 1 ? text : pluralText);

    public string _n(string text, string pluralText, long n, params object[] args) => _catalog?.GetPluralString(text, pluralText, n, args) ?? (n == 1 ? text : pluralText);

    public string _p(string context, string text) => _catalog?.GetParticularString(context, text) ?? text;

    public string _p(string context, string text, params object[] args) => _catalog?.GetParticularString(context, text, args) ?? text;

    public string _pn(string context, string text, string pluralText, long n) => _catalog?.GetParticularPluralString(context, text, pluralText, n) ?? (n == 1 ? text : pluralText);

    public string _pn(string context, string text, string pluralText, long n, params object[] args) => _catalog?.GetParticularPluralString(context, text, pluralText, n, args) ?? (n == 1 ? text : pluralText);
}
