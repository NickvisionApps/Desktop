using System;

namespace Nickvision.Desktop.Helpers;

/// <summary>
///     Helpers for Uri.
/// </summary>
public static class UriExtensions
{
    private static readonly Uri Empty;

    /// <summary>
    ///     Constructs a static UriExtensions.
    /// </summary>
    static UriExtensions()
    {
        Empty = new Uri("about:blank");
    }

    /// <summary>
    ///     Gets an empty Uri (about:blank).
    /// </summary>
    /// <returns>The empty Uri</returns>
    public static Uri GetEmpty() => Empty;

    /// <summary>
    ///     Gets whether the Uri is empty (about:blank).
    /// </summary>
    /// <param name="uri">The Uri to check</param>
    /// <returns>True if the Uri is empty, else false</returns>
    public static bool IsEmpty(this Uri uri) => uri == Empty || uri.ToString() == "about:blank";
}
