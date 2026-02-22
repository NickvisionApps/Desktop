using Microsoft.UI.Dispatching;

namespace Nickvision.Desktop.WinUI.Hosting;

public class HostingContext
{
    public bool IsLifetimeLinked { get; }
    public bool IsRunning { get; set; }
    public DispatcherQueue? Dispatcher {  get; set; }
    public Microsoft.UI.Xaml.Application? Application { get; set; }

    public HostingContext(bool isLifetimeLinked)
    {
        IsLifetimeLinked = isLifetimeLinked;
        IsRunning = false;
        Dispatcher = null;
        Application = null;
    }
}
