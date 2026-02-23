namespace Nickvision.Desktop.Hosting;

/// <summary>
/// An interface for context for a user interface application.
/// </summary>
/// <typeparam name="T">The application class</typeparam>
public interface IUserInterfaceContext<T> where T : class
{
    /// <summary>
    /// The application instance.
    /// </summary>
    public T? Application { get; set; }
    /// <summary>
    /// Whether or not the application is running.
    /// </summary>
    public bool IsRunning { get; set; }
}
