using System.Threading.Tasks;

namespace Nickvision.Desktop.WinUI.Hosting;

public interface IUserInterfaceThread
{
    void StartUserInterface();
    Task StopUserInterfaceAsync();
}
