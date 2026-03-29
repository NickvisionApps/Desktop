using System;

namespace Nickvision.Desktop.Helpers;

public static class UriExtensions
{
    private static readonly Uri EmptyUri;

    static UriExtensions()
    {
        EmptyUri = new Uri("about:blank");
    }

    extension(Uri)
    {
        public static Uri Empty => EmptyUri;
    }

    extension(Uri uri)
    {
        public bool IsEmpty => uri == EmptyUri || uri.ToString() == "about:blank";
    }
}
