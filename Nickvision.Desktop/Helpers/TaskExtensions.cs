using System.Threading.Tasks;

namespace Nickvision.Desktop.Helpers;

/// <summary>
///     Helpers for Task.
/// </summary>
public static class TaskExtensions
{
    extension(Task task)
    {
        /// <summary>
        ///     Starts a Task without awaiting it and ignores any exceptions thrown.
        /// </summary>
        public async void FireAndForget()
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
}
