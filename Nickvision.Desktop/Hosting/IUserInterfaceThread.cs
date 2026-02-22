using System.Threading.Tasks;

namespace Nickvision.Desktop.Hosting;

public interface IUserInterfaceThread
{
    Task StartAsync();
    Task StopAsync();
}
