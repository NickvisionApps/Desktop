using System.Collections.Generic;

namespace Nickvision.Desktop.Application;

public interface IArgumentsService
{
    IReadOnlyList<string> Data { get; }
    int Count { get; }

    bool Add(string arg);
    bool Contains(string arg);
    string? GetNext(string arg);
}
