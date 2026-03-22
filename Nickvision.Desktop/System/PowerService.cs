using Microsoft.Extensions.Logging;
using Nickvision.Desktop.FreeDesktop;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Tmds.DBus;

namespace Nickvision.Desktop.System;

/// <summary>
/// A server for managing power options.
/// </summary>
public partial class PowerService : IDisposable, IPowerService
{
    private const uint ES_CONTINUOUS = 0x80000000u;
    private const uint ES_SYSTEM_REQUIRED = 0x00000001u;

    [LibraryImport("kernel32.dll", SetLastError = true)]
    private static partial uint SetThreadExecutionState(uint esFlags);

    private readonly ILogger<PowerService> _logger;
    private bool _disposed;
    private Connection? _dbus;
    private IScreenSaver? _freeDesktopScreenSaver;
    private uint _inhibitCookie;
    private Process? _preventSuspendProcess;

    /// <summary>
    /// Constructs a PowerService.
    /// </summary>
    /// <param name="logger">Logger for the service</param>
    public PowerService(ILogger<PowerService> logger)
    {
        _logger = logger;
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
        _logger.LogInformation("Allowing system suspend...");
        if (OperatingSystem.IsWindows())
        {
            var result = SetThreadExecutionState(ES_CONTINUOUS) != 0;
            if (result)
            {
                _logger.LogInformation("Allowed system suspend.");
            }
            else
            {
                _logger.LogError($"Failed to allow system suspend: {new Win32Exception(Marshal.GetLastPInvokeError())}");
            }
            return result;
        }
        else if (OperatingSystem.IsLinux())
        {
            if (_inhibitCookie == 0 || _freeDesktopScreenSaver is null)
            {
                _logger.LogWarning("System suspend already allowed.");
                return false;
            }
            await _freeDesktopScreenSaver.UnInhibitAsync(_inhibitCookie);
            _inhibitCookie = 0;
            _logger.LogInformation("Allowed system suspend.");
            return true;
        }
        else if (OperatingSystem.IsMacOS())
        {
            if (_preventSuspendProcess is null)
            {
                _logger.LogWarning("System suspend already allowed.");
                return false;
            }
            _preventSuspendProcess.Kill();
            _preventSuspendProcess.Dispose();
            _preventSuspendProcess = null;
            _logger.LogInformation("Allowed system suspend.");
            return true;
        }
        else
        {
            _logger.LogError($"Unable to allow system suspend. The OS is unsupported.");
            return false;
        }
    }

    /// <summary>
    /// Prevents the system from suspending.
    /// </summary>
    /// <returns>True if the action was applied successfully, else false</returns>
    public async Task<bool> PreventSuspendAsync()
    {
        _logger.LogInformation("Preventing system suspend...");
        if (OperatingSystem.IsWindows())
        {
            var result = SetThreadExecutionState(ES_CONTINUOUS | ES_SYSTEM_REQUIRED) != 0;
            if (result)
            {
                _logger.LogInformation("Prevented system suspend.");
            }
            else
            {
                _logger.LogError($"Failed to prevent system suspend: {new Win32Exception(Marshal.GetLastPInvokeError())}");
            }
            return result;
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
                catch (Exception e)
                {
                    _logger.LogError($"Failed to create FreeDesktop ScreenSaver proxy: {e}");
                    return false;
                }
            }
            if (_inhibitCookie != 0)
            {
                _logger.LogWarning("System suspend already prevented.");
                return true;
            }
            try
            {
                _inhibitCookie = await _freeDesktopScreenSaver.InhibitAsync("Nickvision Desktop", "Preventing suspend");
            }
            catch (Exception e)
            {
                _logger.LogError($"Failed to inhibit FreeDesktop ScreenSaver: {e}");
                return false;
            }
            _logger.LogInformation("Prevented system suspend.");
            return true;
        }
        else if (OperatingSystem.IsMacOS())
        {
            if (_preventSuspendProcess is not null)
            {
                _logger.LogWarning("System suspend already prevented.");
                return true;
            }
            if (string.IsNullOrEmpty(Environment.FindDependency("caffeinate")))
            {
                _logger.LogError("Unable to find 'caffeinate' dependency.");
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
            _logger.LogInformation("Prevented system suspend.");
            return true;
        }
        else
        {
            _logger.LogError($"Unable to prevent system suspend. The OS is unsupported.");
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
