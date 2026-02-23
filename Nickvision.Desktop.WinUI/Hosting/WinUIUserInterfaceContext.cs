using Microsoft.UI.Dispatching;
using Nickvision.Desktop.Hosting;

namespace Nickvision.Desktop.WinUI.Hosting;

public class WinUIUserInterfaceContext : IUserInterfaceContext<Microsoft.UI.Xaml.Application>
{
    public Microsoft.UI.Xaml.Application? Application { get; set; }
    public bool IsRunning { get; set; }
    public DispatcherQueue? Dispatcher { get; set; }

    public WinUIUserInterfaceContext()
    {
        Application = null;
        IsRunning = false;
        Dispatcher = null;
    }
}
