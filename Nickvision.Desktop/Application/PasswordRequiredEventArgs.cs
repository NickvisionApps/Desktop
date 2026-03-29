using System;

namespace Nickvision.Desktop.Application;

public class PasswordRequiredEventArgs : EventArgs
{
    public string Password { get; set; }

    public PasswordRequiredEventArgs()
    {
        Password = string.Empty;
    }
}
