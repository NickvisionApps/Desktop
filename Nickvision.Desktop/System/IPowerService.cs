using System;

namespace Nickvision.Desktop.System;

public interface IPowerService : IDisposable, IService
{
    bool AllowSuspend();

    bool Logoff();

    bool PreventSuspend();

    bool Restart();

    bool Shutdown();

    bool Suspend();
}
