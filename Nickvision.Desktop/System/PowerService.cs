using System;
using System.Diagnostics;
using System.Threading.Tasks;
#if OS_WINDOWS
using Vanara.PInvoke;
#elif OS_LINUX
using Nickvision.Desktop.FreeDesktop;
using Tmds.DBus;
#endif

namespace Nickvision.Desktop.System;

/// <summary>
///     A server for managing power options.
/// </summary>
public class PowerService : IDisposable, IPowerService
{
    private bool _disposed;
#if OS_LINUX
    private Connection? _dbus;
    private IScreenSaver? _freeDesktopScreenSaver;
    private uint _inhibitCookie;
#elif OS_MAC
    private Process? _preventSuspendProcess;
#endif

    /// <summary>
    ///     Constructs a PowerService.
    /// </summary>
    public PowerService()
    {
        _disposed = false;
#if OS_LINUX
        _inhibitCookie = 0;
#endif
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
    public async Task<bool> AllowSuspendAsync()
    {
#if OS_WINDOWS
#pragma warning disable CA1416
        return await Task.FromResult(Kernel32.SetThreadExecutionState(Kernel32.EXECUTION_STATE.ES_CONTINUOUS) != 0);
#pragma warning restore CA1416
#elif OS_LINUX
        if(_inhibitCookie == 0 || _freeDesktopScreenSaver is null)
        {
            return false;
        }
        await _freeDesktopScreenSaver.UnInhibitAsync(_inhibitCookie);
        _inhibitCookie = 0;
        return true;
#elif OS_MAC
        if(_preventSuspendProcess is null)
        {
            return await Task.FromResult(false);
        }
        _preventSuspendProcess.Kill();
        _preventSuspendProcess.Dispose();
        _preventSuspendProcess = null;
        return await Task.FromResult(true);
#else
        return await Task.FromResult(false);
#endif
    }

    /// <summary>
    ///     Prevents the system from suspending.
    /// </summary>
    /// <returns>True if the action was applied successfully, else false</returns>
    public async Task<bool> PreventSuspendAsync()
    {
#if OS_WINDOWS
#pragma warning disable CA1416
        return await Task.FromResult(Kernel32.SetThreadExecutionState(Kernel32.EXECUTION_STATE.ES_CONTINUOUS | Kernel32.EXECUTION_STATE.ES_SYSTEM_REQUIRED) != 0);
#pragma warning restore CA1416
#elif OS_LINUX
        if(_dbus is null)
        {
            _dbus = new Connection(Address.Session);
            await _dbus.ConnectAsync();
        }
        if (_freeDesktopScreenSaver is null)
        {
            _freeDesktopScreenSaver = _dbus.CreateProxy<IScreenSaver>("org.freedesktop.ScreenSaver", new ObjectPath("/org/freedesktop/ScreenSaver"));
        }
        if (_inhibitCookie != 0)
        {
            return true;
        }
        _inhibitCookie = await _freeDesktopScreenSaver.InhibitAsync("Nickvision Desktop", "Preventing suspend");
        return true;
#elif OS_MAC
        if (_preventSuspendProcess is not null)
        {
            return await Task.FromResult(true);
        }
        if (string.IsNullOrEmpty(Environment.FindDependency("caffeinate")))
        {
            return await Task.FromResult(false);
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
        return await Task.FromResult(true);
#else
        return await Task.FromResult(false);
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
#if OS_LINUX
            AllowSuspendAsync().Wait();
            _dbus?.Dispose();
            _dbus = null;
#elif OS_MAC
            _preventSuspendProcess?.Kill();
            _preventSuspendProcess?.Dispose();
            _preventSuspendProcess = null;
#endif
            _disposed = true;
        }
    }
}
