namespace Nickvision.Desktop.Hosting;

public interface IUserInterfaceContext<T> where T : class
{
    public T? Application { get; set; }
    public bool IsRunning { get; set; }
}
