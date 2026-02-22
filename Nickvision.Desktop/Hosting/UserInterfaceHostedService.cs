using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Nickvision.Desktop.Hosting;

public class UserInterfaceHostedService<T> : IHostedService where T : class
{
    private ILogger<UserInterfaceHostedService<T>> _logger;
    private IUserInterfaceThread _userInterfaceThread;
    private IUserInterfaceContext<T> _userInterfaceContext;

    public UserInterfaceHostedService(ILogger<UserInterfaceHostedService<T>> logger, IUserInterfaceThread userInterfaceThread, IUserInterfaceContext<T> userInterfaceContext)
    {
        _logger = logger;
        _userInterfaceThread = userInterfaceThread;
        _userInterfaceContext = userInterfaceContext;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Task.CompletedTask;
        }
        _logger.LogInformation("Starting User Interface Hosted Service.");
        return _userInterfaceThread.StartAsync();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested || !_userInterfaceContext.IsRunning)
        {
            return Task.CompletedTask;
        }
        _logger.LogInformation("Stopping User Interface Hosted Service.");
        return _userInterfaceThread.StopAsync();
    }
}
