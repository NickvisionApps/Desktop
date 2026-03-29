using System.Threading.Tasks;

namespace Nickvision.Desktop.System;

public interface IPowerService
{
    Task<bool> AllowSuspendAsync();

    Task<bool> PreventSuspendAsync();
}
