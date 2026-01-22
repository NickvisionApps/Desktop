using System;

namespace Nickvision.Desktop.Helpers;

/// <summary>
/// Helpers for Uri.
/// </summary>
public static class UriExtensions
{
    private static readonly Uri EmptyUri;

    /// <summary>
    /// Constructs a static UriExtensions.
    /// </summary>
    static UriExtensions()
    {
        EmptyUri = new Uri("about:blank");
    }

    extension(Uri)
    {
        /// <summary>
        /// An empty Uri (about:blank).
        /// </summary>
        /// <returns>The empty Uri</returns>
        public static Uri Empty => EmptyUri;
    }

    extension(Uri uri)
    {
        /// <summary>
        /// Whether the Uri is empty (about:blank).
        /// </summary>
        /// <returns>True if the Uri is empty, else false</returns>
        public bool IsEmpty => uri == EmptyUri || uri.ToString() == "about:blank";
    }
}
