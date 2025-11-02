using System;
using System.Diagnostics;
#if OS_WINDOWS
using Vanara.PInvoke;
#endif

namespace Nickvision.Desktop.System;

/// <summary>
///     A server for managing power options.
/// </summary>
public class PowerService : IPowerService
{
    private bool _disposed;
#if OS_MAC || OS_LINUX
    private Process? _preventSuspendProcess;
#endif

    /// <summary>
    ///     Constructs a PowerService.
    /// </summary>
    public PowerService()
    {
        _disposed = false;
    }

    /// <summary>
    ///     Finalizes a PowerService.
    /// </summary>
    ~PowerService()
    {
        Dispose(false);
    }

    /// <summary>
    ///     Disposes a PowerService.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///     Allows the system to suspend.
    /// </summary>
    /// <returns>True if the action was applied successfully, else false</returns>
    public bool AllowSuspend()
    {
#if OS_WINDOWS
#pragma warning disable CA1416
        return Kernel32.SetThreadExecutionState(Kernel32.EXECUTION_STATE.ES_CONTINUOUS) != 0;
#pragma warning restore CA1416
#elif OS_MAC || OS_LINUX
        _preventSuspendProcess?.Kill();
        _preventSuspendProcess?.Dispose();
        _preventSuspendProcess = null;
        return true;
#else
        return false;
#endif
    }

    /// <summary>
    ///     Logs the user off the system.
    /// </summary>
    /// <returns>True if the action was applied successfully, else false</returns>
    public bool Logoff()
    {
#if OS_WINDOWS
        Process.Start(new ProcessStartInfo
        {
            FileName = "shutdown",
            Arguments = "/l /t 0",
            CreateNoWindow = true,
            UseShellExecute = false
        });
        return true;
#elif OS_MAC
        if (string.IsNullOrEmpty(Environment.FindDependency("osascript")))
        {
            return false;
        }
        Process.Start(new ProcessStartInfo()
        {
            FileName = "osascript",
            Arguments = "-e 'tell application \"System Events\" to log out'",
            CreateNoWindow = true,
            UseShellExecute = false
        });
        return true;
#elif OS_LINUX
        if (string.IsNullOrEmpty(Environment.FindDependency("gnome-session-quit")))
        {
            return false;
        }
        Process.Start(new ProcessStartInfo()
        {
            FileName = "gnome-session-quit",
            Arguments = "--logout",
            CreateNoWindow = true,
            UseShellExecute = false
        });
        return true;
#else
        return false;
#endif
    }

    /// <summary>
    ///     Prevents the system from suspending.
    /// </summary>
    /// <returns>True if the action was applied successfully, else false</returns>
    public bool PreventSuspend()
    {
#if OS_WINDOWS
#pragma warning disable CA1416
        return Kernel32.SetThreadExecutionState(Kernel32.EXECUTION_STATE.ES_CONTINUOUS | Kernel32.EXECUTION_STATE.ES_SYSTEM_REQUIRED) != 0;
#pragma warning restore CA1416
#elif OS_MAC
        if (_preventSuspendProcess != null)
        {
            return true;
        }
        if (string.IsNullOrEmpty(Environment.FindDependency("caffeinate")))
        {
            return false;
        }
        _preventSuspendProcess = new Process()
        {
            StartInfo = new ProcessStartInfo()
            {
                FileName = "caffeinate",
                Arguments = "-dimsu",
                CreateNoWindow = true,
                UseShellExecute = false
            }
        };
        _preventSuspendProcess.Start();
        return true;
#elif OS_LINUX
        if (_preventSuspendProcess != null)
        {
            return true;
        }
        if (string.IsNullOrEmpty(Environment.FindDependency("systemd-inhibit")))
        {
            return false;
        }
        _preventSuspendProcess = new Process()
        {
            StartInfo = new ProcessStartInfo()
            {
                FileName = "systemd-inhibit",
                Arguments = "--what=handle-lid-switch:sleep:idle sleep infinity",
                CreateNoWindow = true,
                UseShellExecute = false
            }
        };
        _preventSuspendProcess.Start();
        return true;
#else
        return false;
#endif
    }

    /// <summary>
    ///     Restarts the system.
    /// </summary>
    /// <returns>True if the action was applied successfully, else false</returns>
    public bool Restart()
    {
#if OS_WINDOWS
        Process.Start(new ProcessStartInfo
        {
            FileName = "shutdown",
            Arguments = "/r /t 0",
            CreateNoWindow = true,
            UseShellExecute = false
        });
        return true;
#elif OS_MAC
        if (string.IsNullOrEmpty(Environment.FindDependency("osascript")))
        {
            return false;
        }
        Process.Start(new ProcessStartInfo()
        {
            FileName = "osascript",
            Arguments = "-e 'tell application \"System Events\" to restart'",
            CreateNoWindow = true,
            UseShellExecute = false
        });
        return true;
#elif OS_LINUX
        if (string.IsNullOrEmpty(Environment.FindDependency("systemctl")))
        {
            return false;
        }
        Process.Start(new ProcessStartInfo()
        {
            FileName = "systemctl",
            Arguments = "reboot",
            CreateNoWindow = true,
            UseShellExecute = false
        });
        return true;
#else
        return false;
#endif
    }

    /// <summary>
    ///     Shuts down the system.
    /// </summary>
    /// <returns>True if the action was applied successfully, else false</returns>
    public bool Shutdown()
    {
#if OS_WINDOWS
        Process.Start(new ProcessStartInfo
        {
            FileName = "shutdown",
            Arguments = "/s /t 0",
            CreateNoWindow = true,
            UseShellExecute = false
        });
        return true;
#elif OS_MAC
        if (string.IsNullOrEmpty(Environment.FindDependency("systemctl")))
        {
            return false;
        }
        Process.Start(new ProcessStartInfo()
        {
            FileName = "osascript",
            Arguments = "-e 'tell application \"System Events\" to shut down'",
            CreateNoWindow = true,
            UseShellExecute = false
        });
        return true;
#elif OS_LINUX
        Process.Start(new ProcessStartInfo()
        {
            FileName = "systemctl",
            Arguments = "poweroff",
            CreateNoWindow = true,
            UseShellExecute = false
        });
        return true;
#else
        return false;
#endif
    }

    /// <summary>
    ///     Suspends the system.
    /// </summary>
    /// <returns>True if the action was applied successfully, else false</returns>
    public bool Suspend()
    {
#if OS_WINDOWS
#pragma warning disable CA1416
        return PowrProf.SetSuspendState(false, true, true);
#pragma warning restore CA1416
#elif OS_MAC
        if (string.IsNullOrEmpty(Environment.FindDependency("osascript")))
        {
            return false;
        }
        Process.Start(new ProcessStartInfo()
        {
            FileName = "osascript",
            Arguments = "-e 'tell application \"System Events\" to sleep'",
            CreateNoWindow = true,
            UseShellExecute = false
        });
        return true;
#elif OS_LINUX
        if (string.IsNullOrEmpty(Environment.FindDependency("systemctl")))
        {
            return false;
        }
        Process.Start(new ProcessStartInfo()
        {
            FileName = "systemctl",
            Arguments = "suspend",
            CreateNoWindow = true,
            UseShellExecute = false
        });
        return true;
#else
        return false;
#endif
    }

    /// <summary>
    ///     Disposes a PowerService.
    /// </summary>
    /// <param name="disposing">Whether to dispose managed resources</param>
    private void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
#if OS_MAC || OS_LINUX
            _preventSuspendProcess?.Kill();
            _preventSuspendProcess?.Dispose();
            _preventSuspendProcess = null;
#endif
            _disposed = true;
        }
    }
}
