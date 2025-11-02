using System;
using System.IO;
#if OS_WINDOWS
using Vanara.PInvoke;
#endif

namespace Nickvision.Desktop.Filesystem;

/// <summary>
///     A helper class for getting user directories cross-platform.
/// </summary>
public static class UserDirectories
{
    /// <summary>
    ///     The user's home directory.
    /// </summary>
    public static string Home => Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

    /// <summary>
    ///     The user's config directory.
    /// </summary>
    public static string Config
    {
        get
        {
#if OS_WINDOWS
            var res = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
#elif OS_MAC
            var res = Path.Combine(Home, "Library", "ApplicationSupport");
#elif OS_LINUX
            var res = string.Empty;
            if (Environment.GetEnvironmentVariable("XDG_CONFIG_HOME") is string dir)
            {
                if (!string.IsNullOrEmpty(dir))
                {
                    res = dir;
                }
            }
            else
            {
                res = Path.Combine(Home, ".config");
            }
#else
            var res = Path.Combine(Home, ".config");
#endif
            Directory.CreateDirectory(res);
            return res;
        }
    }

    /// <summary>
    ///     The user's cache directory.
    /// </summary>
    public static string Cache
    {
        get
        {
#if OS_WINDOWS
            var res = $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}/Temp";
#elif OS_MAC
            var res = Path.Combine(Home, "Library", "Caches");
#elif OS_LINUX
            var res = string.Empty;
            if (Environment.GetEnvironmentVariable("XDG_CACHE_HOME") is string dir)
            {
                if (!string.IsNullOrEmpty(dir))
                {
                    res = dir;
                }
            }
            else
            {
                res = Path.Combine(Home, ".cache");
            }
#else
            var res = Path.Combine(Home, ".cache");
#endif
            Directory.CreateDirectory(res);
            return res;
        }
    }

    /// <summary>
    ///     The user's local data directory.
    /// </summary>
    public static string LocalData
    {
        get
        {
#if OS_WINDOWS
            var res = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
#elif OS_MAC
            var res = Cache;
#elif OS_LINUX
            var res = string.Empty;
            if (Environment.GetEnvironmentVariable("XDG_DATA_HOME") is string dir)
            {
                if (!string.IsNullOrEmpty(dir))
                {
                    res = dir;
                }
            }
            else
            {
                res = Path.Combine(Home, ".local", "share");
            }
#else
            var res = Path.Combine(Home, ".local", "share");
#endif
            Directory.CreateDirectory(res);
            return res;
        }
    }

    /// <summary>
    ///     The user's desktop directory.
    /// </summary>
    public static string Desktop
    {
        get
        {
#if OS_WINDOWS
            var res = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
#elif OS_MAC
            var res = Path.Combine(Home, "Desktop");
#elif OS_LINUX
            var res = string.Empty;
            if (GetXdgUserDir("XDG_DESKTOP_DIR") is string dir)
            {
                if (!string.IsNullOrEmpty(dir))
                {
                    res = dir;
                }
            }
            else
            {
                res = Path.Combine(Home, "Desktop");
            }
#else
            var res = Path.Combine(Home, "Desktop");
#endif
            Directory.CreateDirectory(res);
            return res;
        }
    }

    /// <summary>
    ///     The user's documents directory.
    /// </summary>
    public static string Documents
    {
        get
        {
#if OS_WINDOWS
            var res = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
#elif OS_MAC
            var res = Path.Combine(Home, "Documents");
#elif OS_LINUX
            var res = string.Empty;
            if (GetXdgUserDir("XDG_DOCUMENTS_DIR") is string dir)
            {
                if (!string.IsNullOrEmpty(dir))
                {
                    res = dir;
                }
            }
            else
            {
                res = Path.Combine(Home, "Documents");
            }
#else
            var res = Path.Combine(Home, "Documents");
#endif
            return res;
        }
    }

    /// <summary>
    ///     The user's downloads directory.
    /// </summary>
    public static string Downloads
    {
        get
        {
#if OS_WINDOWS
#pragma warning disable CA1416
            if (Shell32.SHGetKnownFolderPath(Shell32.KNOWNFOLDERID.FOLDERID_Downloads.Guid(), 0, nint.Zero, out var res) != HRESULT.S_OK)
            {
                res = Path.Combine(Home, "Downloads");
            }
#pragma warning restore CA1416
#elif OS_MAC
            var res = Path.Combine(Home, "Downloads");
#elif OS_LINUX
            var res = string.Empty;
            if (GetXdgUserDir("XDG_DOWNLOAD_DIR") is string dir)
            {
                if (!string.IsNullOrEmpty(dir))
                {
                    res = dir;
                }
            }
            else
            {
                res = Path.Combine(Home, "Downloads");
            }
#else
            var res = Path.Combine(Home, "Downloads");
#endif
            return res;
        }
    }

    /// <summary>
    ///     The user's music directory.
    /// </summary>
    public static string Music
    {
        get
        {
#if OS_WINDOWS
            var res = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
#elif OS_MAC
            var res = Path.Combine(Home, "Music");
#elif OS_LINUX
            var res = string.Empty;
            if (GetXdgUserDir("XDG_MUSIC_DIR") is string dir)
            {
                if (!string.IsNullOrEmpty(dir))
                {
                    res = dir;
                }
            }
            else
            {
                res = Path.Combine(Home, "Music");
            }
#else
            var res = Path.Combine(Home, "Music");
#endif
            Directory.CreateDirectory(res);
            return res;
        }
    }

    /// <summary>
    ///     The user's pictures directory.
    /// </summary>
    public static string Pictures
    {
        get
        {
#if OS_WINDOWS
            var res = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
#elif OS_MAC
            var res = Path.Combine(Home, "Pictures");
#elif OS_LINUX
            var res = string.Empty;
            if (GetXdgUserDir("XDG_PICTURES_DIR") is string dir)
            {
                if (!string.IsNullOrEmpty(dir))
                {
                    res = dir;
                }
            }
            else
            {
                res = Path.Combine(Home, "Pictures");
            }
#else
            var res = Path.Combine(Home, "Pictures");
#endif
            Directory.CreateDirectory(res);
            return res;
        }
    }

    /// <summary>
    ///     The user's templates directory.
    /// </summary>
    public static string Templates
    {
        get
        {
#if OS_WINDOWS
            var res = Environment.GetFolderPath(Environment.SpecialFolder.Templates);
#elif OS_MAC
            var res = Path.Combine(Home, "Templates");
#elif OS_LINUX
            var res = string.Empty;
            if (GetXdgUserDir("XDG_TEMPLATES_DIR") is string dir)
            {
                if (!string.IsNullOrEmpty(dir))
                {
                    res = dir;
                }
            }
            else
            {
                res = Path.Combine(Home, "Templates");
            }
#else
            var res = Path.Combine(Home, "Templates");
#endif
            Directory.CreateDirectory(res);
            return res;
        }
    }

    /// <summary>
    ///     The user's videos directory.
    /// </summary>
    public static string Videos
    {
        get
        {
#if OS_WINDOWS
            var res = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
#elif OS_MAC
            var res = Path.Combine(Home, "Videos");
#elif OS_LINUX
            var res = string.Empty;
            if (GetXdgUserDir("XDG_VIDEOS_DIR") is string dir)
            {
                if (!string.IsNullOrEmpty(dir))
                {
                    res = dir;
                }
            }
            else
            {
                res = Path.Combine(Home, "Videos");
            }
#else
            var res = Path.Combine(Home, "Videos");
#endif
            Directory.CreateDirectory(res);
            return res;
        }
    }

#if OS_LINUX
    /// <summary>
    /// Gets an XDG user directory from the user-dirs.dirs file or environment variable.
    /// </summary>
    /// <param name="name">The name of the XDG user directory to get</param>
    /// <returns>The path of the XDG user directory if found, else null</returns>
    private static string? GetXdgUserDir(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return null;
        }
        if (Environment.GetEnvironmentVariable(name) is string path)
        {
            if (!string.IsNullOrEmpty(path))
            {
                return path;
            }
        }
        var dirsPath = $"{Config}/user-dirs.dirs";
        if (!File.Exists(dirsPath))
        {
            return null;
        }
        foreach (var line in File.ReadLines(dirsPath))
        {
            if (string.IsNullOrEmpty(line) ||
                line.StartsWith("#"))
            {
                continue;
            }
            var splits = line.Split('=');
            if (splits.Length != 2)
            {
                continue;
            }
            if (splits[0].Trim() == name)
            {
                return splits[1].Trim().Replace("$HOME", Home).Trim('"');
            }
        }
        return null;
    }
#endif
}
