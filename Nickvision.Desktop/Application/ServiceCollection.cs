using System;
using System.Collections.Generic;

namespace Nickvision.Desktop.Application;

public class ServiceCollection : IDisposable
{
    private readonly Dictionary<Type, IService> _services;
    private bool _disposed;

    public ServiceCollection()
    {
        _disposed = false;
        _services = new Dictionary<Type, IService>();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~ServiceCollection()
    {
        Dispose(false);
    }

    public bool Add<T, U>(U implementation) where T : IService where U : class, T => _services.TryAdd(typeof(T), implementation);

    public T? Get<T>() where T : IService => _services.TryGetValue(typeof(T), out var service) ? (T)service : default(T?);

    public bool Has<T>() where T : IService => _services.ContainsKey(typeof(T));

    public bool Remove<T>() where T : IService => _services.Remove(typeof(T));

    private void Dispose(bool disposing)
    {
        if (_disposed || !disposing)
        {
            return;
        }
        foreach (var service in _services.Values)
        {
            if (service is IDisposable disposableService)
            {
                disposableService.Dispose();
            }
        }
        _services.Clear();
        _disposed = true;
    }
}
