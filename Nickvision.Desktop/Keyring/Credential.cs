using Nickvision.Desktop.Helpers;
using System;

namespace Nickvision.Desktop.Keyring;

public class Credential
{
    public string Name { get; set; }

    public string Password { get; set; }

    public Uri Url { get; set; }

    public string Username { get; set; }

    public Credential(string name, string username, string password, Uri? url = null)
    {
        Name = name;
        Username = username;
        Password = password;
        Url = url ?? Uri.Empty;
    }
}
