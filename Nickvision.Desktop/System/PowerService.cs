using System;
using System.Diagnostics;
#if OS_WINDOWS
using Vanara.PInvoke;
#endif

namespace Nickvision.Desktop.System;

public class PowerService : IPowerService
{
    private bool _disposed;
#if OS_MAC || OS_LINUX
    private Process? _preventSuspendProcess;
#endif

    public PowerService()
    {
        _disposed = false;
    }

    ~PowerService()
    {
        Dispose(false);
    }

    public bool AllowSuspend()
    {
#if OS_WINDOWS
        return Kernel32.SetThreadExecutionState(Kernel32.EXECUTION_STATE.ES_CONTINUOUS) != 0;
#elif OS_MAC || OS_LINUX
        _preventSuspendProcess?.Kill();
        _preventSuspendProcess?.Dispose();
        _preventSuspendProcess = null;
        return true;
#else
        return false;
#endif
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public bool Logoff()
    {
#if OS_WINDOWS
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "shutdown",
                Arguments = "/l /t 0",
                CreateNoWindow = true,
                UseShellExecute = false
            }
        };
        process.Start();
        return true;
#elif OS_MAC
        if (string.IsNullOrEmpty(Environment.FindDependency("osascript")))
        {
            return false;
        }
        Process.Start(
            new ProcessStartInfo()
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
        Process.Start(
            new ProcessStartInfo()
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

    public bool PreventSuspend()
    {
#if OS_WINDOWS
        return Kernel32.SetThreadExecutionState(
                   Kernel32.EXECUTION_STATE.ES_CONTINUOUS | Kernel32.EXECUTION_STATE.ES_SYSTEM_REQUIRED) !=
               0;
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

    public bool Restart()
    {
#if OS_WINDOWS
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "shutdown",
                Arguments = "/r /t 0",
                CreateNoWindow = true,
                UseShellExecute = false
            }
        };
        process.Start();
        return true;
#elif OS_MAC
        if (string.IsNullOrEmpty(Environment.FindDependency("osascript")))
        {
            return false;
        }
        Process.Start(
            new ProcessStartInfo()
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
        Process.Start(
            new ProcessStartInfo()
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

    public bool Shutdown()
    {
#if OS_WINDOWS
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "shutdown",
                Arguments = "/s /t 0",
                CreateNoWindow = true,
                UseShellExecute = false
            }
        };
        process.Start();
        return true;
#elif OS_MAC
        if (string.IsNullOrEmpty(Environment.FindDependency("systemctl")))
        {
            return false;
        }
        Process.Start(
            new ProcessStartInfo()
            {
                FileName = "osascript",
                Arguments = "-e 'tell application \"System Events\" to shut down'",
                CreateNoWindow = true,
                UseShellExecute = false
            });
        return true;
#elif OS_LINUX
        Process.Start(
            new ProcessStartInfo()
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

    public bool Suspend()
    {
#if OS_WINDOWS
        return PowrProf.SetSuspendState(false, true, true);
#elif OS_MAC
        if (string.IsNullOrEmpty(Environment.FindDependency("osascript")))
        {
            return false;
        }
        Process.Start(
            new ProcessStartInfo()
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
        Process.Start(
            new ProcessStartInfo()
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
