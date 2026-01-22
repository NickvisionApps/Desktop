using System;

namespace Nickvision.Desktop.Filesystem;

/// <summary>
/// A class of event arguments for when a json file is saved.
/// </summary>
public class JsonFileSavedEventArgs : EventArgs
{
    /// <summary>
    /// The object that was saved to a json file.
    /// </summary>
    public object Data { get; init; }

    /// <summary>
    /// The type of the saved object.
    /// </summary>
    public Type DataType { get; init; }

    /// <summary>
    /// The name of the json file (without the .json extension).
    /// </summary>
    public string Name { get; init; }

    public JsonFileSavedEventArgs(object data, Type dataType, string name)
    {
        Data = data;
        DataType = dataType;
        Name = name;
    }
}
