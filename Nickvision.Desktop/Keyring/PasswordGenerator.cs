using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace Nickvision.Desktop.Keyring;

public class PasswordGenerator
{
    private static readonly List<char> NumericChars;
    private static readonly List<char> UpperChars;
    private static readonly List<char> LowerChars;
    private static readonly List<char> SpecialChars;

    static PasswordGenerator()
    {
        NumericChars = ['0', '1', '2', '3', '4', '5', '6', '7', '8', '9'];
        UpperChars =
        [
            'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U',
            'V', 'W', 'X', 'Y', 'Z'
        ];
        LowerChars =
        [
            'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u',
            'v', 'w', 'x', 'y', 'z'
        ];
        SpecialChars =
        [
            '!', '"', '#', '$', '%', '&', '\'', '(', ')', '*', '+', ',', '-', '.', '/', ':', ';', '<', '=', '>', '?',
            '@', '[', '\\', ']', '^', '_', '`', '{', '|', '}', '~'
        ];
    }

    public PasswordGenerator(PasswordContent contentFlags = PasswordContent.All)
    {
        ContentFlags = contentFlags;
    }

    public PasswordContent ContentFlags { get; set; }

    public string Next(int length = 16)
    {
        var password = string.Empty;
        for (var i = 0; i < length; i++)
        {
            var charType = (PasswordContent)(1 << RandomNumberGenerator.GetInt32(5));
            if ((ContentFlags & charType) == 0)
            {
                i--;
                continue;
            }
            password += charType switch
            {
                PasswordContent.Numeric => NumericChars[RandomNumberGenerator.GetInt32(NumericChars.Count)],
                PasswordContent.Uppercase => UpperChars[RandomNumberGenerator.GetInt32(UpperChars.Count)],
                PasswordContent.Lowercase => LowerChars[RandomNumberGenerator.GetInt32(LowerChars.Count)],
                PasswordContent.Special => SpecialChars[RandomNumberGenerator.GetInt32(SpecialChars.Count)],
                PasswordContent.Space => ' ',
                var _ => throw new InvalidOperationException("Invalid password content type")
            };
        }
        return password;
    }
}
