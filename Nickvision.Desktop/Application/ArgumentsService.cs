using System.Collections.Generic;
using System.Linq;

namespace Nickvision.Desktop.Application;

public class ArgumentsService : IArgumentsService
{
    private readonly List<string> _args;

    public IReadOnlyList<string> Data => _args;
    public int Count => _args.Count;

    public ArgumentsService(string[] args)
    {
        _args = args.ToList();
    }

    public bool Add(string arg)
    {
        if (_args.Contains(arg))
        {
            return false;
        }
        _args.Add(arg);
        return true;
    }

    public bool Contains(string arg) => _args.Contains(arg);

    public string? GetNext(string arg)
    {
        for (int i = 0; i < _args.Count - 1; i++)
        {
            if (_args[i] == arg)
            {
                return _args[i + 1];
            }
        }
        return null;
    }
}
