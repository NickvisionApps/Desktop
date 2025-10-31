namespace Nickvision.Desktop.Application;

public class WindowGeometry
{
    public WindowGeometry()
    {
        Width = 800;
        Height = 600;
        IsMaximized = false;
        X = 10;
        Y = 10;
    }

    public WindowGeometry(int width, int height, bool isMaximized)
    {
        Width = width;
        Height = height;
        IsMaximized = isMaximized;
        X = 10;
        Y = 10;
    }

    public WindowGeometry(int width,
        int height,
        bool isMaximized,
        int x,
        int y)
    {
        Width = width;
        Height = height;
        IsMaximized = isMaximized;
        X = x;
        Y = y;
    }

    public int Width { get; set; }
    public int Height { get; set; }
    public bool IsMaximized { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
}
