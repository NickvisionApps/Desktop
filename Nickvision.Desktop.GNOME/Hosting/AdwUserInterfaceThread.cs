using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Nickvision.Desktop.Application;
using Nickvision.Desktop.GNOME.Helpers;
using Nickvision.Desktop.Hosting;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Nickvision.Desktop.GNOME.Hosting;

public class AdwUserInterfaceThread : IDisposable, IUserInterfaceThread
{
    private readonly ILogger<AdwUserInterfaceThread> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IHostApplicationLifetime _lifetime;
    private readonly AdwUserInterfaceContext _context;
    private readonly ManualResetEvent? _serviceManualResetEvent;
    private WindowExtensions.OpenCallback? _openCallback;

    public AdwUserInterfaceThread(ILogger<AdwUserInterfaceThread> logger, IServiceProvider serviceProvider, IHostApplicationLifetime lifetime, AdwUserInterfaceContext context)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _lifetime = lifetime;
        _context = context;
        if (OperatingSystem.IsMacOS())
        {
            InitializeApplication();
        }
        else
        {
            _serviceManualResetEvent = new ManualResetEvent(false);
            var thread = new Thread(() =>
            {
                InitializeApplication();
                var argumentsService = _serviceProvider.GetRequiredService<IArgumentsService>();
                _ = _serviceManualResetEvent.WaitOne();
                _context.IsRunning = true;
                _context.Application!.RunWithSynchronizationContext(argumentsService.Data.ToArray());
                _context.IsRunning = false;
            })
            {
                IsBackground = true
            };
            thread.Start();
        }
    }

    ~AdwUserInterfaceThread()
    {
        Dispose(false);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        Dispose(true);
    }

    public Task StartAsync()
    {
        if (OperatingSystem.IsMacOS())
        {
            var argumentsService = _serviceProvider.GetRequiredService<IArgumentsService>();
            _lifetime.ApplicationStopping.Register(() => _context.Application?.Quit());
            _context.IsRunning = true;
            _context.Application!.RunWithSynchronizationContext(argumentsService.Data.ToArray());
            _context.IsRunning = false;
            _lifetime.StopApplication();
        }
        else
        {
            _serviceManualResetEvent!.Set();
        }
        return Task.CompletedTask;
    }

    public Task StopAsync()
    {
        _context.Application?.Quit();
        return Task.CompletedTask;
    }

    private void Dispose(bool disposing)
    {
        if (!disposing)
        {
            return;
        }
        _serviceManualResetEvent?.Dispose();
    }

    private void InitializeApplication()
    {
        var argumentsService = _serviceProvider.GetRequiredService<IArgumentsService>();
        _context.Application = _serviceProvider.GetRequiredService<Adw.Application>();
        _context.Application.OnStartup += (_, _) =>
        {
            if (!string.IsNullOrEmpty(_context.ResourceBasePath))
            {
                var display = Gdk.Display.GetDefault();
                if (display is not null)
                {
                    Gtk.IconTheme.GetForDisplay(display).AddResourcePath(_context.ResourceBasePath);
                }
            }
            _context.Application.AddWindow(_serviceProvider.GetRequiredService<Adw.ApplicationWindow>());
        };
        _context.Application.OnShutdown += (_, _) =>
        {
            _context.IsRunning = false;
            if (!OperatingSystem.IsMacOS() && !_lifetime.ApplicationStarted.IsCancellationRequested && _lifetime.ApplicationStopping.IsCancellationRequested)
            {
                _lifetime.StopApplication();
            }
        };
        _context.Application.OnActivate += (_, _) => _serviceProvider.GetRequiredService<Adw.ApplicationWindow>().Present();
        GLib.Functions.LogSetWriterFunc((logLevel, fields) =>
        {
            foreach (var field in fields)
            {
                if (field.Key != "MESSAGE")
                {
                    continue;
                }
                var ptr = field.Handle.DangerousGetHandle();
                var length = Marshal.ReadIntPtr(ptr, IntPtr.Size * 2).ToInt64();
                var message = length >= 0 ? Marshal.PtrToStringUTF8(Marshal.ReadIntPtr(ptr, IntPtr.Size), (int)length) : Marshal.PtrToStringUTF8(Marshal.ReadIntPtr(ptr, IntPtr.Size));
                _logger.Log(logLevel switch
                {
                    GLib.LogLevelFlags.LevelError => LogLevel.Error,
                    GLib.LogLevelFlags.LevelCritical => LogLevel.Critical,
                    GLib.LogLevelFlags.LevelWarning => LogLevel.Warning,
                    GLib.LogLevelFlags.LevelMessage => LogLevel.Information,
                    GLib.LogLevelFlags.LevelInfo => LogLevel.Information,
                    GLib.LogLevelFlags.LevelDebug => LogLevel.Debug,
                    _ => LogLevel.None
                }, message?.Trim());
            }
            return GLib.LogWriterOutput.Handled;
        });
        if (_context.HandlesOpen)
        {
            _openCallback = (nint application, nint[] files, int n_files, nint hint, nint data) =>
            {
                foreach (var file in files)
                {
                    if (OperatingSystem.IsWindows())
                    {
                        argumentsService.Add(WindowsImports.g_file_get_uri(file));
                    }
                    else if (OperatingSystem.IsLinux())
                    {
                        argumentsService.Add(LinuxImports.g_file_get_uri(file));
                    }
                    else if (OperatingSystem.IsMacOS())
                    {
                        argumentsService.Add(MacOSImports.g_file_get_uri(file));
                    }
                }
                _context.Application.Activate();
            };
            if (OperatingSystem.IsWindows())
            {
                WindowsImports.g_signal_connect_data(_context.Application.Handle.DangerousGetHandle(), "open", _openCallback, nint.Zero, nint.Zero, 0);
            }
            else if (OperatingSystem.IsLinux())
            {
                LinuxImports.g_signal_connect_data(_context.Application.Handle.DangerousGetHandle(), "open", _openCallback, nint.Zero, nint.Zero, 0);
            }
            else if (OperatingSystem.IsMacOS())
            {
                MacOSImports.g_signal_connect_data(_context.Application.Handle.DangerousGetHandle(), "open", _openCallback, nint.Zero, nint.Zero, 0);
            }
        }
    }
}