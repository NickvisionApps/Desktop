using System.Threading.Tasks;

namespace Nickvision.Desktop.Helpers;

/// <summary>
/// Helpers for Task.
/// </summary>
public static class TaskExtensions
{
    /// <summary>
    /// Starts a Task without awaiting it and ignores any exceptions thrown.
    /// </summary>
    /// <param name="task">The task to fire and forget</param>
    public static async void FireAndForget(this Task task)
    {
        try
        {
            await task.ConfigureAwait(false);
        }
        catch
        {
            // ignored
        }
    }
}
