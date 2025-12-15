using System;
using System.Text.Json.Serialization;

namespace Nickvision.Desktop.Application;

public class AppVersion : IComparable<AppVersion>, IEquatable<AppVersion>
{
    public Version BaseVersion { get; init; }
    public string PreviewLabel { get; init; }

    public AppVersion(string version)
    {
        var dashIndex = version.IndexOf('-');
        version = version.TrimStart('v');
        if (!Version.TryParse(dashIndex == -1 ? version : version[..dashIndex], out var baseVersion))
        {
            throw new ArgumentException("Invalid version format", nameof(version));
        }
        BaseVersion = baseVersion;
        PreviewLabel = dashIndex == -1 ? string.Empty : version[(dashIndex + 1)..];
    }

    public AppVersion(Version version)
    {
        BaseVersion = version;
        PreviewLabel = string.Empty;
    }

    [JsonConstructor]
    public AppVersion(Version baseVersion, string previewLabel)
    {
        BaseVersion = baseVersion;
        PreviewLabel = previewLabel;
    }

    [JsonIgnore]
    public bool IsPreview => !string.IsNullOrEmpty(PreviewLabel);

    public static bool TryParse(string version, out AppVersion? appVersion)
    {
        appVersion = null;
        try
        {
            appVersion = new AppVersion(version);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static bool operator <(AppVersion? pv1, AppVersion? pv2)
    {
        if (pv1?.BaseVersion < pv2?.BaseVersion)
        {
            return true;
        }
        else if (pv1?.BaseVersion == pv2?.BaseVersion)
        {
            return string.Compare(pv1?.PreviewLabel, pv2?.PreviewLabel, StringComparison.Ordinal) < 0;
        }
        return false;
    }

    public static bool operator <(AppVersion? pv, Version? v) => pv is null ? v is not null : pv.BaseVersion <= v;

    public static bool operator <=(AppVersion? pv1, AppVersion? pv2) => pv1 is null ? pv2 is null : pv1 < pv2 || pv1 == pv2;

    public static bool operator >(AppVersion? pv1, AppVersion? pv2)
    {
        if (pv1 is null)
        {
            return false;
        }
        if (pv1.BaseVersion > pv2?.BaseVersion)
        {
            return true;
        }
        if (pv1.BaseVersion == pv2?.BaseVersion)
        {
            return string.Compare(pv1.PreviewLabel, pv2?.PreviewLabel, StringComparison.Ordinal) > 0;
        }
        return false;
    }

    public static bool operator >(AppVersion? pv, Version? v) => pv is null ? v is null : pv.BaseVersion > v;

    public static bool operator >=(AppVersion? pv1, AppVersion? pv2) => pv1 is null ? pv2 is null : pv1 > pv2 || pv1 == pv2;

    public static bool operator ==(AppVersion? pv1, AppVersion? pv2) => pv1 is null ? pv2 is null : pv1.BaseVersion == pv2?.BaseVersion && pv1.PreviewLabel == pv2?.PreviewLabel;

    public static bool operator ==(AppVersion? pv, Version? v) => pv is null ? v is null : pv.BaseVersion == v && string.IsNullOrEmpty(pv.PreviewLabel);

    public static bool operator !=(AppVersion? pv1, AppVersion? pv2) => !(pv1 == pv2);

    public static bool operator !=(AppVersion? pv, Version? v) => !(pv == v);

    public int CompareTo(AppVersion? other)
    {
        if (this < other)
        {
            return -1;
        }
        else if (this > other)
        {
            return 1;
        }
        return 0;
    }

    public bool Equals(AppVersion? other) => this == other;

    public override bool Equals(object? obj) => obj switch
    {
        AppVersion pv => this == pv,
        Version v => this == v,
        var _ => false
    };

    public override int GetHashCode() => ToString().GetHashCode();

    public override string ToString() => string.IsNullOrEmpty(PreviewLabel) ? BaseVersion.ToString() : $"{BaseVersion}-{PreviewLabel}";
}
