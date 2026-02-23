using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Nickvision.Desktop.Application;
using Nickvision.Desktop.GNOME.Helpers;
using Nickvision.Desktop.Hosting;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Nickvision.Desktop.GNOME.Hosting;

public class AdwUserInterfaceThread : IDisposable, IUserInterfaceThread
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IHostApplicationLifetime _lifetime;
    private readonly AdwUserInterfaceContext _context;
    private readonly ManualResetEvent _serviceManualResetEvent;
    private WindowExtensions.OpenCallback? _openCallback;

    public AdwUserInterfaceThread(IServiceProvider serviceProvider, IHostApplicationLifetime lifetime, AdwUserInterfaceContext context)
    {
        _serviceProvider = serviceProvider;
        _lifetime = lifetime;
        _context = context;
        _serviceManualResetEvent = new ManualResetEvent(false);
        var thread = new Thread(() =>
        {
            var argumentsService = _serviceProvider.GetRequiredService<IArgumentsService>();
            _context.Application = _serviceProvider.GetRequiredService<Adw.Application>();
            _context.Application.OnStartup += (_, _) => _context.Application.AddWindow(_serviceProvider.GetRequiredService<Adw.ApplicationWindow>());
            _context.Application.OnShutdown += (_, _) =>
            {
                _context.IsRunning = false;
                if (!_lifetime.ApplicationStarted.IsCancellationRequested && _lifetime.ApplicationStopping.IsCancellationRequested)
                {
                    _lifetime.StopApplication();
                }
            };
            _context.Application.OnActivate += (_, _) => _serviceProvider.GetRequiredService<Adw.ApplicationWindow>().Present();
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
            _ = _serviceManualResetEvent.WaitOne();
            _context.IsRunning = true;
            _context.Application.RunWithSynchronizationContext(argumentsService.Data.ToArray());
            _context.IsRunning = false;
        })
        {
            IsBackground = true
        };
        thread.Start();
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
        _serviceManualResetEvent.Set();
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
        _serviceManualResetEvent.Dispose();
    }
}
