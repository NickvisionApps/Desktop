using System.Threading.Tasks;

namespace Nickvision.Desktop.Helpers;

public static class TaskExtensions
{
    extension(Task task)
    {
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
