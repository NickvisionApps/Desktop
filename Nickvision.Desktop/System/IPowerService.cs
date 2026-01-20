using System.Threading.Tasks;

namespace Nickvision.Desktop.System;

/// <summary>
///     An interface for a service for managing power options.
/// </summary>
public interface IPowerService : IService
{
    /// <summary>
    ///     Allows the system to suspend.
    /// </summary>
    /// <returns>True if the action was applied successfully, else false</returns>
    Task<bool> AllowSuspendAsync();

    /// <summary>
    ///     Prevents the system from suspending.
    /// </summary>
    /// <returns>True if the action was applied successfully, else false</returns>
    Task<bool> PreventSuspendAsync();
}
