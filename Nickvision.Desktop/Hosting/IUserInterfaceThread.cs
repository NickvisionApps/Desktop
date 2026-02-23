using System.Threading.Tasks;

namespace Nickvision.Desktop.Hosting;

/// <summary>
/// An interface for a thread of a user interface application.
/// </summary>
public interface IUserInterfaceThread
{
    /// <summary>
    /// Asynchronously starts the user interface.
    /// </summary>
    Task StartAsync();
    /// <summary>
    /// Asynchronously stops the user interface.
    /// </summary>
    Task StopAsync();
}
