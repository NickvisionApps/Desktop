using Microsoft.Extensions.Logging;
using System;

namespace Nickvision.Desktop.Tests.Mocks;

public class MockLogger<T> : ILogger<T> where T : class
{
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) => Console.WriteLine($"[{logLevel}] {formatter(state, exception)}");
    public bool IsEnabled(LogLevel logLevel) => true;
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
}
