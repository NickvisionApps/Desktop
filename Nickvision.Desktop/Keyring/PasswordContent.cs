using System;

namespace Nickvision.Desktop.Keyring;

/// <summary>
/// Flags for password content types.
/// </summary>
[Flags]
public enum PasswordContent
{
    Numeric = 1 << 0,
    Uppercase = 1 << 1,
    Lowercase = 1 << 2,
    Special = 1 << 3,
    Space = 1 << 4,
    All = Numeric | Uppercase | Lowercase | Special | Space,
    AllNoSpace = Numeric | Uppercase | Lowercase | Special
}
