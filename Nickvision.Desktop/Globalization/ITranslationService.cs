using System;
using System.Collections.Generic;

namespace Nickvision.Desktop.Globalization;

public interface ITranslationService : IService
{
    string Language { get; set; }
    IEnumerable<string> AvailableLanguages { get; }

    string _(string text);

    string _(string text, params object[] args);

    string _n(string text, string pluralText, long n);

    string _n(string text, string pluralText, long n, params object[] args);

    string _p(string context, string text);

    string _p(string context, string text, params object[] args);

    string _pn(string context, string text, string pluralText, long n);

    string _pn(string context, string text, string pluralText, long n, params object[] args);

    string Get(string text) => _(text);

    string Get(string text, params object[] args) => _(text, args);

    Uri GetHelpUrl(string pageName);

    string GetPlural(string text, string pluralText, long n) => _n(text, pluralText, n);

    string GetPlural(string text, string pluralText, long n, params object[] args) => _n(text, pluralText, n, args);

    string GetParticular(string context, string text) => _p(context, text);

    string GetParticular(string context, string text, params object[] args) => _p(context, text, args);

    string GetParticularPlural(string context, string text, string pluralText, long n) =>
        _pn(context, text, pluralText, n);

    string GetParticularPlural(string context, string text, string pluralText, long n, params object[] args) =>
        _pn(context, text, pluralText, n, args);
}
