using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Dispatching;
using Nickvision.Desktop.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;
using WinRT;

namespace Nickvision.Desktop.WinUI.Hosting;

public partial class WinUIUserInterfaceThread : IDisposable, IUserInterfaceThread
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IHostApplicationLifetime _lifetime;
    private readonly WinUIUserInterfaceContext _context;
    private readonly ManualResetEvent _serviceManualResetEvent;

    public WinUIUserInterfaceThread(IServiceProvider serviceProvider, IHostApplicationLifetime lifetime, WinUIUserInterfaceContext context)
    {
        _serviceProvider = serviceProvider;
        _lifetime = lifetime;
        _context = context;
        _serviceManualResetEvent = new ManualResetEvent(false);
        var thread = new Thread(() =>
        {
            ComWrappersSupport.InitializeComWrappers();
            _ = _serviceManualResetEvent.WaitOne();
            _context.IsRunning = true;
            Microsoft.UI.Xaml.Application.Start(_ =>
            {
                _context.Dispatcher = DispatcherQueue.GetForCurrentThread();
                SynchronizationContext.SetSynchronizationContext(new DispatcherQueueSynchronizationContext(_context.Dispatcher));
                _context.Application = _serviceProvider.GetRequiredService<Microsoft.UI.Xaml.Application>();
            });
            _context.IsRunning = false;
            if (!_lifetime.ApplicationStarted.IsCancellationRequested && _lifetime.ApplicationStopping.IsCancellationRequested)
            {
                _lifetime.StopApplication();
            }
        })
        {
            IsBackground = true
        };
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
    }

    ~WinUIUserInterfaceThread()
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
        var completion = new TaskCompletionSource();
        _context.Dispatcher!.TryEnqueue(() =>
        {
            _context.Application?.Exit();
            completion.SetResult();
        });
        return completion.Task;
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
