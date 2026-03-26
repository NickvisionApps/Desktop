using System;

namespace Nickvision.Desktop.Filesystem;

public class JsonFileSavedEventArgs : EventArgs
{
    public object Data { get; init; }

    public Type DataType { get; init; }

    public string Name { get; init; }

    public JsonFileSavedEventArgs(object data, Type dataType, string name)
    {
        Data = data;
        DataType = dataType;
        Name = name;
    }
}
