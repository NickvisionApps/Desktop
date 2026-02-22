using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Dispatching;
using System;
using System.Threading;
using System.Threading.Tasks;
using WinRT;

namespace Nickvision.Desktop.WinUI.Hosting;

public partial class UserInterfaceThread : IDisposable, IUserInterfaceThread
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IHostApplicationLifetime _lifetime;
    private readonly HostingContext _context;
    private readonly ILogger<UserInterfaceThread> _logger;
    private readonly ManualResetEvent _serviceManualResetEvent;
    private readonly ManualResetEvent _uiThreadManualResetEvent;

    public UserInterfaceThread(IServiceProvider serviceProvider, IHostApplicationLifetime lifetime, HostingContext context, ILogger<UserInterfaceThread> logger)
    {
        _serviceProvider = serviceProvider;
        _lifetime = lifetime;
        _context = context;
        _logger = logger;
        _serviceManualResetEvent = new ManualResetEvent(false);
        _uiThreadManualResetEvent = new ManualResetEvent(false);
        var thread = new Thread(() =>
        {
            BeforeStart();
            _ = _serviceManualResetEvent.WaitOne();
            _context.IsRunning = true;
            DoStart();
            OnCompletion();
        })
        {
            IsBackground = true
        };
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
    }

    ~UserInterfaceThread()
    {
        Dispose(false);
    }

    public void AwaitUiThreadCompletion() => _uiThreadManualResetEvent.WaitOne();

    public void BeforeStart() => ComWrappersSupport.InitializeComWrappers();

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        Dispose(true);
    }

    public void DoStart() => Microsoft.UI.Xaml.Application.Start(_ =>
    {
        _context.Dispatcher = DispatcherQueue.GetForCurrentThread();
        var context = new DispatcherQueueSynchronizationContext(_context.Dispatcher);
        SynchronizationContext.SetSynchronizationContext(context);
        _context.Application = _serviceProvider.GetRequiredService<Microsoft.UI.Xaml.Application>();
    });

    public void StartUserInterface() => _serviceManualResetEvent.Set();

    public Task StopUserInterfaceAsync()
    {
        var completion = new TaskCompletionSource();
        _context.Dispatcher!.TryEnqueue(() =>
        {
            _context.Application?.Exit();
            completion.SetResult();
        });
        return completion.Task;
    }

    private void OnCompletion()
    {
        _context.IsRunning = false;
        if(_context.IsLifetimeLinked)
        {
            StoppingHostApplication();
            if(!_lifetime.ApplicationStarted.IsCancellationRequested && _lifetime.ApplicationStopping.IsCancellationRequested)
            {
                _lifetime.StopApplication();
            }
            _uiThreadManualResetEvent.Set();
        }
    }

    private void Dispose(bool disposing)
    {
        if (!disposing)
        {
            return;
        }
        _serviceManualResetEvent.Dispose();
        _uiThreadManualResetEvent.Dispose();
    }

    [LoggerMessage(SkipEnabledCheck = true, Level = LogLevel.Debug, Message = "Stopping hosted application due to user interface thread exit.")]
    private partial void StoppingHostApplication();
}
