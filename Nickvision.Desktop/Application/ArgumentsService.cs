using System.Collections.Generic;
using System.Linq;

namespace Nickvision.Desktop.Application;

/// <summary>
/// A service for accessing application arguments.
/// </summary>
public class ArgumentsService : IArgumentsService
{
    private readonly List<string> _args;

    /// <summary>
    /// The raw arguments.
    /// </summary>
    public IReadOnlyList<string> Data => _args;

    /// <summary>
    /// Constructs an ArgumentService.
    /// </summary>
    /// <param name="args">The raw arguments</param>
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

    /// <summary>
    /// Checks if the arguments contains a specific argument
    /// </summary>
    /// <param name="arg">The argument to check for</param>
    /// <returns>True if the arguments contain the specified argument, else false</returns>
    public bool Contains(string arg) => _args.Contains(arg);

    /// <summary>
    /// Gets the next argument after a specific argument.
    /// </summary>
    /// <param name="arg">The argument to start from</param>
    /// <returns>The argument after the specified argument if exists, else null</returns>
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
