using System.Linq;

namespace Nickvision.Desktop.Application;

/// <summary>
/// A service for accessing application arguments.
/// </summary>
public class ArgumentsService : IArgumentsService
{
    /// <summary>
    /// The raw arguments.
    /// </summary>
    public string[] Data { get; }

    /// <summary>
    /// Constructs an ArgumentService.
    /// </summary>
    /// <param name="args">The raw arguments</param>
    public ArgumentsService(string[] args)
    {
        Data = args;
    }

    /// <summary>
    /// Checks if the arguments contains a specific argument
    /// </summary>
    /// <param name="arg">The argument to check for</param>
    /// <returns>True if the arguments contain the specified argument, else false</returns>
    public bool Contains(string arg) => Data.Contains(arg);

    /// <summary>
    /// Gets the next argument after a specific argument.
    /// </summary>
    /// <param name="arg">The argument to start from</param>
    /// <returns>The argument after the specified argument if exists, else null</returns>
    public string? GetNext(string arg)
    {
        for (int i = 0; i < Data.Length - 1; i++)
        {
            if (Data[i] == arg)
            {
                return Data[i + 1];
            }
        }
        return null;
    }
}
