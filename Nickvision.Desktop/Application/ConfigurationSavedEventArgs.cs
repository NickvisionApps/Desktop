using System;

namespace Nickvision.Desktop.Application;

public class ConfigurationSavedEventArgs : EventArgs
{
    public string ChangedPropertyName { get; }
    public object ChangedPropertyNewValue { get; }
    public Type ChangedPropertyType { get; }

    public ConfigurationSavedEventArgs(string changedPropertyName, object changedPropertyNewValue, Type changedPropertyType)
    {
        ChangedPropertyName = changedPropertyName;
        ChangedPropertyNewValue = changedPropertyNewValue;
        ChangedPropertyType = changedPropertyType;
    }
}
