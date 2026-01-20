using System;
using System.Threading.Tasks;
using Tmds.DBus;

namespace Nickvision.Desktop.FreeDesktop;

[DBusInterface("org.freedesktop.ScreenSaver")]
public interface IScreenSaver : IDBusObject
{
    Task LockAsync();
    Task SimulateUserActivityAsync();
    Task<bool> GetActiveAsync();
    Task<uint> GetActiveTimeAsync();
    Task<uint> GetSessionIdleTimeAsync();
    Task<bool> SetActiveAsync(bool E);
    Task<uint> InhibitAsync(string ApplicationName, string ReasonForInhibit);
    Task UnInhibitAsync(uint Cookie);
    Task<uint> ThrottleAsync(string ApplicationName, string ReasonForInhibit);
    Task UnThrottleAsync(uint Cookie);
    Task<IDisposable> WatchActiveChangedAsync(Action<bool> handler, Action<Exception>? onError = null);
}
