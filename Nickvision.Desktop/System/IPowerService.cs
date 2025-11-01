using System;

namespace Nickvision.Desktop.System;

/// <summary>
/// An interface for a service for managing power options.
/// </summary>
public interface IPowerService : IDisposable, IService
{
    /// <summary>
    /// Allows the system to suspend.
    /// </summary>
    /// <returns>True if the action was applied successfully, else false</returns>
    bool AllowSuspend();

    /// <summary>
    /// Logs the user off the system.
    /// </summary>
    /// <returns>True if the action was applied successfully, else false</returns>
    bool Logoff();

    /// <summary>
    /// Prevents the system from suspending.
    /// </summary>
    /// <returns>True if the action was applied successfully, else false</returns>
    bool PreventSuspend();

    /// <summary>
    /// Restarts the system.
    /// </summary>
    /// <returns>True if the action was applied successfully, else false</returns>
    bool Restart();

    /// <summary>
    /// Shuts down the system.
    /// </summary>
    /// <returns>True if the action was applied successfully, else false</returns>
    bool Shutdown();

    /// <summary>
    /// Suspends the system.
    /// </summary>
    /// <returns>True if the action was applied successfully, else false</returns>
    bool Suspend();
}
