using System.Collections.Generic;

namespace Nickvision.Desktop.Application;

/// <summary>
/// An interface for a service that access application arguments.
/// </summary>
public interface IArgumentsService
{
    /// <summary>
    /// The raw arguments.
    /// </summary>
    public IReadOnlyList<string> Data { get; }

    /// <summary>
    /// Adds the unique argument.
    /// </summary>
    /// <param name="arg">The argument to add</param>
    /// <returns>True if the argument was added, else false</returns>
    public bool Add(string arg);
    /// <summary>
    /// Checks if the arguments contains a specific argument
    /// </summary>
    /// <param name="arg">The argument to check for</param>
    /// <returns>True if the arguments contain the specified argument, else false</returns>
    public bool Contains(string arg);
    /// <summary>
    /// Gets the next argument after a specific argument.
    /// </summary>
    /// <param name="arg">The argument to start from</param>
    /// <returns>The argument after the specified argument if exists, else null</returns>
    public string? GetNext(string arg);
}
