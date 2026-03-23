using Microsoft.Extensions.Logging;
using Nickvision.Desktop.FreeDesktop;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Tmds.DBus.Protocol;
using Windows.Win32;
using Windows.Win32.System.Power;

namespace Nickvision.Desktop.System;

/// <summary>
/// A server for managing power options.
/// </summary>
public class PowerService : IDisposable, IPowerService
{
    private readonly ILogger<PowerService> _logger;
    private bool _disposed;
    private DBusConnection? _dbus;
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
#pragma warning disable CA1416
            var result = PInvoke.SetThreadExecutionState(EXECUTION_STATE.ES_CONTINUOUS) != (EXECUTION_STATE)0;
#pragma warning restore CA1416
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
            if (_inhibitCookie == 0 || _dbus is null)
            {
                _logger.LogWarning("System suspend already allowed.");
                return false;
            }
            await ScreenSaverProxy.UnInhibitAsync(_dbus, _inhibitCookie);
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
#pragma warning disable CA1416
            var result = PInvoke.SetThreadExecutionState(EXECUTION_STATE.ES_CONTINUOUS | EXECUTION_STATE.ES_SYSTEM_REQUIRED) != (EXECUTION_STATE)0;
#pragma warning restore CA1416
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
                var sessionAddress = DBusAddress.Session;
                if (sessionAddress is null)
                {
                    _logger.LogError("Failed to prevent system suspend: DBUS_SESSION_BUS_ADDRESS is not set.");
                    return false;
                }
                _dbus = new DBusConnection(sessionAddress);
                await _dbus.ConnectAsync();
            }
            if (_inhibitCookie != 0)
            {
                _logger.LogWarning("System suspend already prevented.");
                return true;
            }
            try
            {
                _inhibitCookie = await ScreenSaverProxy.InhibitAsync(_dbus, "Nickvision Desktop", "Preventing suspend");
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
