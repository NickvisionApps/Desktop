using System;
using System.IO;
using Vanara.PInvoke;

namespace Nickvision.Desktop.Filesystem;

/// <summary>
/// A helper class for getting user directories cross-platform.
/// </summary>
public static class UserDirectories
{
    /// <summary>
    /// The user's home directory.
    /// </summary>
    public static string Home => Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

    /// <summary>
    /// The user's config directory.
    /// </summary>
    public static string Config
    {
        get
        {
            var res = string.Empty;
            if (OperatingSystem.IsWindows())
            {
                res = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            }
            else if (OperatingSystem.IsMacOS())
            {
                res = Path.Combine(Home, "Library", "ApplicationSupport");
            }
            else if (OperatingSystem.IsLinux())
            {
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
            }
            else
            {
                res = Path.Combine(Home, ".config");
            }
            Directory.CreateDirectory(res);
            return res;
        }
    }

    /// <summary>
    /// The user's cache directory.
    /// </summary>
    public static string Cache
    {
        get
        {
            var res = string.Empty;
            if (OperatingSystem.IsWindows())
            {
                res = $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}/Temp";
            }
            else if (OperatingSystem.IsMacOS())
            {
                res = Path.Combine(Home, "Library", "Caches");
            }
            else if (OperatingSystem.IsLinux())
            {
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
            }
            else
            {
                res = Path.Combine(Home, ".cache");
            }
            Directory.CreateDirectory(res);
            return res;
        }
    }

    /// <summary>
    /// The user's local data directory.
    /// </summary>
    public static string LocalData
    {
        get
        {
            var res = string.Empty;
            if (OperatingSystem.IsWindows())
            {
                res = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            }
            else if (OperatingSystem.IsMacOS())
            {
                res = Cache;
            }
            else if (OperatingSystem.IsLinux())
            {
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
            }
            else
            {
                res = Path.Combine(Home, ".local", "share");
            }
            Directory.CreateDirectory(res);
            return res;
        }
    }

    /// <summary>
    /// The user's desktop directory.
    /// </summary>
    public static string Desktop
    {
        get
        {
            var res = string.Empty;
            if (OperatingSystem.IsWindows())
            {
                res = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            }
            else if (OperatingSystem.IsMacOS())
            {
                res = Path.Combine(Home, "Desktop");
            }
            else if (OperatingSystem.IsLinux())
            {
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
            }
            else
            {
                res = Path.Combine(Home, "Desktop");
            }
            Directory.CreateDirectory(res);
            return res;
        }
    }

    /// <summary>
    /// The user's documents directory.
    /// </summary>
    public static string Documents
    {
        get
        {
            var res = string.Empty;
            if (OperatingSystem.IsWindows())
            {
                res = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            }
            else if (OperatingSystem.IsMacOS())
            {
                res = Path.Combine(Home, "Documents");
            }
            else if (OperatingSystem.IsLinux())
            {
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
            }
            else
            {
                res = Path.Combine(Home, "Documents");
            }
            return res;
        }
    }

    /// <summary>
    /// The user's downloads directory.
    /// </summary>
    public static string Downloads
    {
        get
        {
            var res = string.Empty;
            if (OperatingSystem.IsWindows())
            {
                if (Shell32.SHGetKnownFolderPath(Shell32.KNOWNFOLDERID.FOLDERID_Downloads.Guid(), 0, nint.Zero, out res) != HRESULT.S_OK)
                {
                    res = Path.Combine(Home, "Downloads");
                }
            }
            else if (OperatingSystem.IsMacOS())
            {
                res = Path.Combine(Home, "Downloads");
            }
            else if (OperatingSystem.IsLinux())
            {
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
            }
            else
            {
                res = Path.Combine(Home, "Downloads");
            }
            return res;
        }
    }

    /// <summary>
    /// The user's music directory.
    /// </summary>
    public static string Music
    {
        get
        {
            var res = string.Empty;
            if (OperatingSystem.IsWindows())
            {
                res = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
            }
            else if (OperatingSystem.IsMacOS())
            {
                res = Path.Combine(Home, "Music");
            }
            else if (OperatingSystem.IsLinux())
            {
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
            }
            else
            {
                res = Path.Combine(Home, "Music");
            }
            Directory.CreateDirectory(res);
            return res;
        }
    }

    /// <summary>
    /// The user's pictures directory.
    /// </summary>
    public static string Pictures
    {
        get
        {
            var res = string.Empty;
            if (OperatingSystem.IsWindows())
            {
                res = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            }
            else if (OperatingSystem.IsMacOS())
            {
                res = Path.Combine(Home, "Pictures");
            }
            else if (OperatingSystem.IsLinux())
            {
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
            }
            else
            {
                res = Path.Combine(Home, "Pictures");
            }
            Directory.CreateDirectory(res);
            return res;
        }
    }

    /// <summary>
    /// The user's templates directory.
    /// </summary>
    public static string Templates
    {
        get
        {
            var res = string.Empty;
            if (OperatingSystem.IsWindows())
            {
                res = Environment.GetFolderPath(Environment.SpecialFolder.Templates);
            }
            else if (OperatingSystem.IsMacOS())
            {
                res = Path.Combine(Home, "Templates");
            }
            else if (OperatingSystem.IsLinux())
            {
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
            }
            else
            {
                res = Path.Combine(Home, "Templates");
            }
            Directory.CreateDirectory(res);
            return res;
        }
    }

    /// <summary>
    /// The user's videos directory.
    /// </summary>
    public static string Videos
    {
        get
        {
            var res = string.Empty;
            if (OperatingSystem.IsWindows())
            {
                res = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
            }
            else if (OperatingSystem.IsMacOS())
            {
                res = Path.Combine(Home, "Videos");
            }
            else if (OperatingSystem.IsLinux())
            {
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
            }
            else
            {
                res = Path.Combine(Home, "Videos");
            }
            Directory.CreateDirectory(res);
            return res;
        }
    }

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
            if (string.IsNullOrEmpty(line) || line.StartsWith("#"))
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
}
