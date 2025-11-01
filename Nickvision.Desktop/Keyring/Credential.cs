using Nickvision.Desktop.Helpers;
using System;

namespace Nickvision.Desktop.Keyring;

/// <summary>
/// A class representing a credential.
/// </summary>
public class Credential
{
    /// <summary>
    /// Constructs a credential.
    /// </summary>
    /// <param name="name">The friendly name of the credential</param>
    /// <param name="username">The username of the credential</param>
    /// <param name="password">The password of the credential</param>
    /// <param name="url">The url of the credential</param>
    public Credential(string name, string username, string password, Uri? url = null)
    {
        Name = name;
        Username = username;
        Password = password;
        Url = UriExtensions.GetEmpty();
    }

    /// <summary>
    /// The friendly name of the credential.
    /// </summary>
    public string Name { get; set; }
    /// <summary>
    /// The username of the credential.
    /// </summary>
    public string Username { get; set; }
    /// <summary>
    /// The password of the credential.
    /// </summary>
    public string Password { get; set; }
    /// <summary>
    /// The url of the credential.
    /// </summary>
    public Uri Url { get; set; }
}
