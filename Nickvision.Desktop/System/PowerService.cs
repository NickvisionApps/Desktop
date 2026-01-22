using Nickvision.Desktop.FreeDesktop;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Tmds.DBus;
using Vanara.PInvoke;

namespace Nickvision.Desktop.System;

/// <summary>
/// A server for managing power options.
/// </summary>
public class PowerService : IDisposable, IPowerService
{
    private bool _disposed;
    private Connection? _dbus;
    private IScreenSaver? _freeDesktopScreenSaver;
    private uint _inhibitCookie;
    private Process? _preventSuspendProcess;

    /// <summary>
    /// Constructs a PowerService.
    /// </summary>
    public PowerService()
    {
        _disposed = false;
        _inhibitCookie = 0;
    }

    /// <summary>
    /// Finalizes a PowerService.
    /// </summary>
    ~PowerService()
    {
        Dispose(false);
    }

    /// <summary>
    /// Disposes a PowerService.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Allows the system to suspend.
    /// </summary>
    /// <returns>True if the action was applied successfully, else false</returns>
    public async Task<bool> AllowSuspendAsync()
    {
        if (OperatingSystem.IsWindows())
        {
            return Kernel32.SetThreadExecutionState(Kernel32.EXECUTION_STATE.ES_CONTINUOUS) != 0;
        }
        else if (OperatingSystem.IsLinux())
        {
            if (_inhibitCookie == 0 || _freeDesktopScreenSaver is null)
            {
                return false;
            }
            await _freeDesktopScreenSaver.UnInhibitAsync(_inhibitCookie);
            _inhibitCookie = 0;
            return true;
        }
        else if (OperatingSystem.IsMacOS())
        {
            if (_preventSuspendProcess is null)
            {
                return false;
            }
            _preventSuspendProcess.Kill();
            _preventSuspendProcess.Dispose();
            _preventSuspendProcess = null;
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Prevents the system from suspending.
    /// </summary>
    /// <returns>True if the action was applied successfully, else false</returns>
    public async Task<bool> PreventSuspendAsync()
    {
        if (OperatingSystem.IsWindows())
        {
            return Kernel32.SetThreadExecutionState(Kernel32.EXECUTION_STATE.ES_CONTINUOUS | Kernel32.EXECUTION_STATE.ES_SYSTEM_REQUIRED) != 0;
        }
        else if (OperatingSystem.IsLinux())
        {
            if (_dbus is null)
            {
                _dbus = new Connection(Address.Session);
                await _dbus.ConnectAsync();
            }
            if (_freeDesktopScreenSaver is null)
            {
                try
                {
                    _freeDesktopScreenSaver = _dbus.CreateProxy<IScreenSaver>("org.freedesktop.ScreenSaver", "/org/freedesktop/ScreenSaver");
                }
                catch
                {
                    return false;
                }
            }
            if (_inhibitCookie != 0)
            {
                return true;
            }
            _inhibitCookie = await _freeDesktopScreenSaver.InhibitAsync("Nickvision Desktop", "Preventing suspend");
            return true;
        }
        else if (OperatingSystem.IsMacOS())
        {
            if (_preventSuspendProcess is not null)
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
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Disposes a PowerService.
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
            if (OperatingSystem.IsLinux())
            {
                AllowSuspendAsync().Wait();
                _dbus?.Dispose();
                _dbus = null;
            }
            else if (OperatingSystem.IsMacOS())
            {
                _preventSuspendProcess?.Kill();
                _preventSuspendProcess?.Dispose();
                _preventSuspendProcess = null;
            }
            _disposed = true;
        }
    }
}
