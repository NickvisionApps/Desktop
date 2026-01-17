using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Nickvision.Desktop.Application;
using System;
using Vanara.PInvoke;
using Windows.Graphics;
using WinRT.Interop;

namespace Nickvision.Desktop.WinUI.Helpers;

public static class WindowExtensions
{
    extension(Window window)
    {
        public HWND Hwnd => WindowNative.GetWindowHandle(window);

        public WindowGeometry Geometry
        {
            get => new WindowGeometry(window.AppWindow.Size.Width, window.AppWindow.Size.Height, User32.IsZoomed(window.Hwnd), window.AppWindow.Position.X, window.AppWindow.Position.Y);

            set
            {
                if (value.IsMaximized)
                {
                    window.AppWindow.Resize(new SizeInt32
                    {
                        Width = 900,
                        Height = 700
                    });
                    User32.ShowWindow(window.Hwnd, ShowWindowCommand.SW_SHOWMAXIMIZED);
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
    }
}
