using System.Threading.Tasks;

namespace Nickvision.Desktop.Helpers;

public static class TaskExtensions
{
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
