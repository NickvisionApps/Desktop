using System;

namespace Nickvision.Desktop.Helpers;

public static class UriExtensions
{
    private static readonly Uri Empty;

    static UriExtensions()
    {
        Empty = new Uri("about:blank");
    }

    public static Uri GetEmpty() => Empty;

    public static bool IsEmpty(this Uri uri) => uri == Empty || uri.ToString() == "about:blank";
}
