using System;
using System.Collections.Generic;

namespace Nickvision.Desktop.Application;

/// <summary>
///     A collection of services for an application.
/// </summary>
public class ServiceCollection : IDisposable
{
    private readonly Dictionary<Type, IService> _services;
    private bool _disposed;

    /// <summary>
    ///     Constructs a ServiceCollection.
    /// </summary>
    public ServiceCollection()
    {
        _disposed = false;
        _services = new Dictionary<Type, IService>();
    }

    /// <summary>
    ///     Finalizes a ServiceCollection.
    /// </summary>
    ~ServiceCollection()
    {
        Dispose(false);
    }

    /// <summary>
    ///     Disposes a ServiceCollection and its services.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///     Adds a service to the collection.
    /// </summary>
    /// <param name="implementation">The object of the service interface</param>
    /// <typeparam name="T">The service interface</typeparam>
    /// <returns>The object to the service if successfully added, else null</returns>
    public T? Add<T>(T implementation) where T : IService => _services.TryAdd(typeof(T), implementation) ? implementation : default(T?);

    /// <summary>
    ///     Gets a service from the collection.
    /// </summary>
    /// <typeparam name="T">The service interface</typeparam>
    /// <returns>The object matching the service interface if found, else null</returns>
    public T? Get<T>() where T : IService => _services.TryGetValue(typeof(T), out var service) ? (T)service : default(T?);

    /// <summary>
    ///     Gets whether a service from the collection with the interface type exists.
    /// </summary>
    /// <typeparam name="T">The service interface</typeparam>
    /// <returns>True if an object matching the service interface is found, else false</returns>
    public bool Has<T>() where T : IService => _services.ContainsKey(typeof(T));

    /// <summary>
    ///     Removes a service from the collection.
    /// </summary>
    /// <typeparam name="T">The service interface</typeparam>
    /// <returns>True if an object matching the service interface was removed, else false</returns>
    public bool Remove<T>() where T : IService => _services.Remove(typeof(T));

    /// <summary>
    ///     Disposes a ServiceCollection and its services.
    /// </summary>
    /// <param name="disposing">Whether to dispose managed resources</param>
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
