using System;

namespace Nickvision.Desktop.Application;

public class PreviewVersion
{
    public PreviewVersion(string version)
    {
        if (!Version.TryParse(version[..version.IndexOf('-')], out var baseVersion))
        {
            throw new ArgumentException("Invalid version format", nameof(version));
        }
        BaseVersion = baseVersion;
        PreviewLabel = version[(version.IndexOf('-') + 1)..];
    }

    public Version BaseVersion { get; init; }
    public string PreviewLabel { get; init; }

    public static bool TryParse(string version, out PreviewVersion? previewVersion)
    {
        previewVersion = null;
        if (!version.Contains('-'))
        {
            return false;
        }
        try
        {
            previewVersion = new PreviewVersion(version);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static bool operator <(PreviewVersion pv1, PreviewVersion pv2)
    {
        if (pv1.BaseVersion < pv2.BaseVersion)
        {
            return true;
        }
        else if (pv1.BaseVersion == pv2.BaseVersion)
        {
            return string.Compare(pv1.PreviewLabel, pv2.PreviewLabel, StringComparison.Ordinal) < 0;
        }
        return false;
    }

    public static bool operator <(PreviewVersion pv, Version v) => pv.BaseVersion <= v;

    public static bool operator >(PreviewVersion pv1, PreviewVersion pv2)
    {
        if (pv1.BaseVersion > pv2.BaseVersion)
        {
            return true;
        }
        else if (pv1.BaseVersion == pv2.BaseVersion)
        {
            return string.Compare(pv1.PreviewLabel, pv2.PreviewLabel, StringComparison.Ordinal) > 0;
        }
        return false;
    }

    public static bool operator >(PreviewVersion pv, Version v) => pv.BaseVersion > v;

    public static bool operator ==(PreviewVersion pv1, PreviewVersion pv2) => pv1.BaseVersion == pv2.BaseVersion && pv1.PreviewLabel == pv2.PreviewLabel;

    public static bool operator ==(PreviewVersion pv, Version v) => pv.BaseVersion == v && string.IsNullOrEmpty(pv.PreviewLabel);

    public static bool operator !=(PreviewVersion pv1, PreviewVersion pv2) => !(pv1 == pv2);

    public static bool operator !=(PreviewVersion pv, Version v) => !(pv == v);

    public override bool Equals(object? obj)
    {
        return obj switch
        {
            PreviewVersion pv => this == pv,
            Version v => this == v,
            var _ => false
        };
    }

    public override int GetHashCode() => ToString().GetHashCode();

    public override string ToString() => $"{BaseVersion}-{PreviewLabel}";
}
