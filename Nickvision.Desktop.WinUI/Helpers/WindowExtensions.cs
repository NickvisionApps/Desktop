using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Nickvision.Desktop.Application;
using System;
using Windows.Graphics;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Shell;
using Windows.Win32.UI.WindowsAndMessaging;
using WinRT.Interop;

namespace Nickvision.Desktop.WinUI.Helpers;

public static partial class WindowExtensions
{
    extension(Window window)
    {
        public HWND Hwnd => (HWND)WindowNative.GetWindowHandle(window);

        public WindowGeometry Geometry
        {
            get => new WindowGeometry(window.AppWindow.Size.Width, window.AppWindow.Size.Height, PInvoke.IsZoomed(window.Hwnd), window.AppWindow.Position.X, window.AppWindow.Position.Y);

            set
            {
                if (value.IsMaximized)
                {
                    window.AppWindow.Resize(new SizeInt32
                    {
                        Width = 900,
                        Height = 700
                    });
                    PInvoke.ShowWindow(window.Hwnd, SHOW_WINDOW_CMD.SW_SHOWMAXIMIZED);
                }
                else
                {
                    if (window.IsGeometryValid(value))
                    {
                        window.AppWindow.MoveAndResize(new RectInt32
                        {
                            X = value.X,
                            Y = value.Y,
                            Width = value.Width,
                            Height = value.Height
                        });
                    }
                    else
                    {
                        window.AppWindow.Resize(new SizeInt32
                        {
                            Width = Math.Max(900, value.Width),
                            Height = Math.Max(700, value.Height)
                        });
                    }
                }
            }
        }

        public bool IsGeometryValid(WindowGeometry windowGeometry)
        {
            var workArea = DisplayArea.GetFromWindowId(window.AppWindow.Id, DisplayAreaFallback.Nearest).WorkArea;
            var windowRect = new RectInt32(windowGeometry.X, windowGeometry.Y, windowGeometry.Width, windowGeometry.Height);
            return Math.Max(0, Math.Min(windowRect.X + windowRect.Width, workArea.X + workArea.Width) - Math.Max(windowRect.X, workArea.X)) >= 100 &&
                Math.Max(0, Math.Min(windowRect.Y + windowRect.Height, workArea.Y + workArea.Height) - Math.Max(windowRect.Y, workArea.Y)) >= 100;
        }

        public void EnsureMinimumSize(int minWidth, int minHeight)
        {
            var dpi = PInvoke.GetDpiForWindow(window.Hwnd);
            var scale = dpi / 96.0;
            var scaledMinWidth = (int)(minWidth * scale);
            var scaledMinHeight = (int)(minHeight * scale);
            var size = window.AppWindow.Size;
            if (size.Width < scaledMinWidth || size.Height < scaledMinHeight)
            {
                window.AppWindow.Resize(new SizeInt32(
                    Math.Max(size.Width, scaledMinWidth),
                    Math.Max(size.Height, scaledMinHeight)));
            }
        }

        public bool SetWindowSubclass(SUBCLASSPROC subclassProc) =>
            PInvoke.SetWindowSubclass(window.Hwnd, subclassProc, 0, 0);

        public bool RemoveWindowSubclass(SUBCLASSPROC subclassProc) =>
            PInvoke.RemoveWindowSubclass(window.Hwnd, subclassProc, 0);
    }
}
