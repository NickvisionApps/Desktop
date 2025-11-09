namespace Nickvision.Desktop.System;

/// <summary>
///     A class representing a secret.
/// </summary>
public class Secret
{
    public string Name { get; init; }
    public string Value { get; init; }

    public Secret(string name, string value)
    {
        Name = name;
        Value = value;
    }

    public bool Empty => string.IsNullOrEmpty(Name) || string.IsNullOrEmpty(Value);
}
