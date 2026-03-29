using Nickvision.Desktop.Hosting;

namespace Nickvision.Desktop.GNOME.Hosting;

public class AdwUserInterfaceContext : IUserInterfaceContext<Adw.Application>
{
    public Adw.Application? Application { get; set; }
    public bool IsRunning { get; set; }
    public bool HandlesOpen { get; set; }
    public string ResourceBasePath { get; set; }

    public AdwUserInterfaceContext(bool handlesOpen)
    {
        Application = null;
        IsRunning = false;
        HandlesOpen = handlesOpen;
        ResourceBasePath = string.Empty;
    }
}
