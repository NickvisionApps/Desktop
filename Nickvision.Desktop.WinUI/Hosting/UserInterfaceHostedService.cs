using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Nickvision.Desktop.WinUI.Hosting;

public partial class UserInterfaceHostedService : IHostedService
{
    private readonly ILogger<UserInterfaceHostedService> _logger;
    private readonly IUserInterfaceThread _userInterfaceThread;
    private readonly HostingContext _context;

    public UserInterfaceHostedService(ILogger<UserInterfaceHostedService> logger, IUserInterfaceThread userInterfaceThread, HostingContext context)
    {
        _logger = logger;
        _userInterfaceThread = userInterfaceThread;
        _context = context;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Task.CompletedTask;
        }
        _userInterfaceThread.StartUserInterface();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested || _context.IsRunning)
        {
            return Task.CompletedTask;
        }
        StoppingUserInterfaceThread();
        return _userInterfaceThread.StopUserInterfaceAsync();
    }

    [LoggerMessage(SkipEnabledCheck = true, Level = LogLevel.Debug, Message = "Stopping user interface thread due to application exiting.")]
    private partial void StoppingUserInterfaceThread();
}
