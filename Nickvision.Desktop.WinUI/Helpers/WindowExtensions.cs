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
    private sealed class WindowMinimumSizeRegistration : IDisposable
    {
        private readonly HWND _hWnd;
        private readonly SUBCLASSPROC _proc;

        public WindowMinimumSizeRegistration(HWND hWnd, SUBCLASSPROC proc)
        {
            _hWnd = hWnd;
            _proc = proc;
        }

        public void Dispose() => PInvoke.RemoveWindowSubclass(_hWnd, _proc, 0);
    }

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

        public IDisposable RegisterMinimumSizeProc(int minWidth, int minHeight)
        {
            window.EnsureMinimumSize(minWidth, minHeight);
            var hwnd = window.Hwnd;
            unsafe LRESULT MinimumSizeSubclassProc(HWND hWnd, uint uMsg, WPARAM wParam, LPARAM lParam, nuint uIdSubclass, nuint dwRefData)
            {
                if (uMsg == PInvoke.WM_GETMINMAXINFO)
                {
                    PInvoke.DefSubclassProc(hWnd, uMsg, wParam, lParam);
                    var dpi = PInvoke.GetDpiForWindow(hWnd);
                    var scale = dpi / 96.0;
                    var minMaxInfo = (MINMAXINFO*)lParam.Value;
                    minMaxInfo->ptMinTrackSize.x = (int)(minWidth * scale);
                    minMaxInfo->ptMinTrackSize.y = (int)(minHeight * scale);
                    return (LRESULT)(nint)0;
                }
                return PInvoke.DefSubclassProc(hWnd, uMsg, wParam, lParam);
            }
            var proc = new SUBCLASSPROC(MinimumSizeSubclassProc);
            if (!PInvoke.SetWindowSubclass(hwnd, proc, 0, 0))
            {
                throw new InvalidOperationException("Failed to install window subclass for minimum size enforcement.");
            }
            return new WindowMinimumSizeRegistration(hwnd, proc);
        }
    }
}
