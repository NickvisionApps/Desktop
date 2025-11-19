namespace Nickvision.Desktop.Application;

/// <summary>
///     A class containing window geometry information.
/// </summary>
public class WindowGeometry
{
    /// <summary>
    ///     The height of the window.
    /// </summary>
    public int Height { get; set; }

    /// <summary>
    ///     Whether the window is maximized.
    /// </summary>
    public bool IsMaximized { get; set; }

    /// <summary>
    ///     The width of the window.
    /// </summary>
    public int Width { get; set; }

    /// <summary>
    ///     The x position of the window.
    /// </summary>
    public int X { get; set; }

    /// <summary>
    ///     The y position of the window.
    /// </summary>
    public int Y { get; set; }

    /// <summary>
    ///     Constructs a WindowGeometry.
    /// </summary>
    public WindowGeometry() : this(900, 700, false, 10, 10)
    {
    }

    /// <summary>
    ///     Constructs a WindowGeometry.
    /// </summary>
    /// <param name="width">The width of the window</param>
    /// <param name="height">The height of the window</param>
    /// <param name="isMaximized">Whether the window is maximized</param>
    public WindowGeometry(int width, int height, bool isMaximized) : this(width, height, isMaximized, 10, 10)
    {
    }

    /// <summary>
    ///     Constructs a WindowGeometry.
    /// </summary>
    /// <param name="width">The width of the window</param>
    /// <param name="height">The height of the window</param>
    /// <param name="isMaximized">Whether the window is maximized</param>
    /// <param name="x">The x position of the window</param>
    /// <param name="y">The y position of the window</param>
    public WindowGeometry(int width, int height, bool isMaximized, int x, int y)
    {
        Width = width;
        Height = height;
        IsMaximized = isMaximized;
        X = x;
        Y = y;
    }
}
