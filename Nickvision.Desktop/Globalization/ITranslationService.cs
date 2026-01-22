using System;
using System.Collections.Generic;

namespace Nickvision.Desktop.Globalization;

/// <summary>
/// An interface for a service for translations.
/// </summary>
public interface ITranslationService : IService
{
    /// <summary>
    /// The list of available language codes for translations.
    /// </summary>
    IEnumerable<string> AvailableLanguages { get; }
    /// <summary>
    /// The language code for translations.
    /// </summary>
    /// <remarks>
    /// An empty string will use the system's language code for translations. The language code "C" will cause strings
    /// to remain untranslated
    /// </remarks>
    string Language { get; set; }

    /// <summary>
    /// Translates a string.
    /// </summary>
    /// <param name="text">The string to translate.</param>
    /// <returns>The translated string</returns>
    string _(string text);

    /// <summary>
    /// Translates a format string.
    /// </summary>
    /// <param name="text">The format string to translate.</param>
    /// <param name="args">The arguments for the format string</param>
    /// <returns>The translated format string</returns>
    string _(string text, params object[] args);

    /// <summary>
    /// Translates a possible plural string.
    /// </summary>
    /// <param name="text">The non-plural string to translate</param>
    /// <param name="pluralText">The plural string to translate</param>
    /// <param name="n">The number of objects</param>
    /// <returns>The translated plural string if n != 1, else the translated non-plural string</returns>
    string _n(string text, string pluralText, long n);

    /// <summary>
    /// Translates a possible plural format string.
    /// </summary>
    /// <param name="text">The non-plural format string to translate</param>
    /// <param name="pluralText">The plural format string to translate</param>
    /// <param name="n">The number of objects</param>
    /// <param name="args">The arguments for the format string</param>
    /// <returns>The translated format plural string if n != 1, else the translated non-plural format string</returns>
    string _n(string text, string pluralText, long n, params object[] args);

    /// <summary>
    /// Translates a string for a particular context.
    /// </summary>
    /// <param name="context">The context of the string</param>
    /// <param name="text">The string to translate</param>
    /// <returns>The translated string for the context</returns>
    string _p(string context, string text);

    /// <summary>
    /// Translates a format string for a particular context.
    /// </summary>
    /// <param name="context">The context of the string</param>
    /// <param name="text">The format string to translate</param>
    /// <param name="args">The arguments for the format string</param>
    /// <returns>The translated format string for the context</returns>
    string _p(string context, string text, params object[] args);

    /// <summary>
    /// Translates a possible plural string for a particular context.
    /// </summary>
    /// <param name="context">The context of the string</param>
    /// <param name="text">The non-plural string to translate</param>
    /// <param name="pluralText">The plural string to translate</param>
    /// <param name="n">The number of objects</param>
    /// <returns>The translated plural string if n != 1, else the translated non-plural string for the context</returns>
    string _pn(string context, string text, string pluralText, long n);

    /// <summary>
    /// Translates a possible plural format string for a particular context.
    /// </summary>
    /// <param name="context">The context of the string</param>
    /// <param name="text">The non-plural format string to translate</param>
    /// <param name="pluralText">The plural format string to translate</param>
    /// <param name="n">The number of objects</param>
    /// <param name="args">The arguments for the format string</param>
    /// <returns>The translated plural format string if n != 1, else the translated non-plural format string for the context</returns>
    string _pn(string context, string text, string pluralText, long n, params object[] args);

    /// <summary>
    /// Translates a string.
    /// </summary>
    /// <param name="text">The string to translate.</param>
    /// <returns>The translated string</returns>
    string Get(string text) => _(text);

    /// <summary>
    /// Translates a format string.
    /// </summary>
    /// <param name="text">The format string to translate.</param>
    /// <param name="args">The arguments for the format string</param>
    /// <returns>The translated format string</returns>
    string Get(string text, params object[] args) => _(text, args);

    /// <summary>
    /// Gets the localized help page url for a given page name.
    /// </summary>
    /// <param name="pageName">The name of the help page</param>
    /// <returns>The help page url for the current system locale</returns>
    Uri GetHelpUrl(string pageName);

    /// <summary>
    /// Translates a possible plural string.
    /// </summary>
    /// <param name="text">The non-plural string to translate</param>
    /// <param name="pluralText">The plural string to translate</param>
    /// <param name="n">The number of objects</param>
    /// <returns>The translated plural string if n != 1, else the translated non-plural string</returns>
    string GetPlural(string text, string pluralText, long n) => _n(text, pluralText, n);

    /// <summary>
    /// Translates a possible plural format string.
    /// </summary>
    /// <param name="text">The non-plural format string to translate</param>
    /// <param name="pluralText">The plural format string to translate</param>
    /// <param name="n">The number of objects</param>
    /// <param name="args">The arguments for the format string</param>
    /// <returns>The translated format plural string if n != 1, else the translated non-plural format string</returns>
    string GetPlural(string text, string pluralText, long n, params object[] args) => _n(text, pluralText, n, args);

    /// <summary>
    /// Translates a string for a particular context.
    /// </summary>
    /// <param name="context">The context of the string</param>
    /// <param name="text">The string to translate</param>
    /// <returns>The translated string for the context</returns>
    string GetParticular(string context, string text) => _p(context, text);

    /// <summary>
    /// Translates a format string for a particular context.
    /// </summary>
    /// <param name="context">The context of the string</param>
    /// <param name="text">The format string to translate</param>
    /// <param name="args">The arguments for the format string</param>
    /// <returns>The translated format string for the context</returns>
    string GetParticular(string context, string text, params object[] args) => _p(context, text, args);

    /// <summary>
    /// Translates a possible plural string for a particular context.
    /// </summary>
    /// <param name="context">The context of the string</param>
    /// <param name="text">The non-plural string to translate</param>
    /// <param name="pluralText">The plural string to translate</param>
    /// <param name="n">The number of objects</param>
    /// <returns>The translated plural string if n != 1, else the translated non-plural string for the context</returns>
    string GetParticularPlural(string context, string text, string pluralText, long n) => _pn(context, text, pluralText, n);

    /// <summary>
    /// Translates a possible plural format string for a particular context.
    /// </summary>
    /// <param name="context">The context of the string</param>
    /// <param name="text">The non-plural format string to translate</param>
    /// <param name="pluralText">The plural format string to translate</param>
    /// <param name="n">The number of objects</param>
    /// <param name="args">The arguments for the format string</param>
    /// <returns>The translated plural format string if n != 1, else the translated non-plural format string for the context</returns>
    string GetParticularPlural(string context, string text, string pluralText, long n, params object[] args) => _pn(context, text, pluralText, n, args);
}
