namespace Nickvision.Desktop.Application;

public class WindowGeometry
{
    public int Height { get; set; }

    public bool IsMaximized { get; set; }

    public int Width { get; set; }

    public int X { get; set; }

    public int Y { get; set; }

    public WindowGeometry() : this(900, 700, false, 10, 10)
    {

    }

    public WindowGeometry(int width, int height, bool isMaximized) : this(width, height, isMaximized, 10, 10)
    {

    }

    public WindowGeometry(int width, int height, bool isMaximized, int x, int y)
    {
        Width = width;
        Height = height;
        IsMaximized = isMaximized;
        X = x;
        Y = y;
    }
}
