using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Nickvision.Desktop.Hosting;

/// <summary>
/// A hosted service for a user interface application.
/// </summary>
/// <typeparam name="T">The application class</typeparam>
public class UserInterfaceHostedService<T> : IHostedService where T : class
{
    private ILogger<UserInterfaceHostedService<T>> _logger;
    private IUserInterfaceThread _userInterfaceThread;
    private IUserInterfaceContext<T> _userInterfaceContext;

    /// <summary>
    /// Constructs a UserInterfaceHostedService.
    /// </summary>
    /// <param name="logger">The logger</param>
    /// <param name="userInterfaceThread">The thread for the user interface application</param>
    /// <param name="userInterfaceContext">The context for the user interface application</param>
    public UserInterfaceHostedService(ILogger<UserInterfaceHostedService<T>> logger, IUserInterfaceThread userInterfaceThread, IUserInterfaceContext<T> userInterfaceContext)
    {
        _logger = logger;
        _userInterfaceThread = userInterfaceThread;
        _userInterfaceContext = userInterfaceContext;
    }

    /// <summary>
    /// Asynchronously starts the user interface.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token</param>
    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Task.CompletedTask;
        }
        _logger.LogInformation("Starting User Interface Hosted Service.");
        return _userInterfaceThread.StartAsync();
    }

    /// <summary>
    /// Asynchronously stops the user interface.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token</param>
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
