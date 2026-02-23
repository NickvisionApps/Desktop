using Nickvision.Desktop.Application;

namespace Nickvision.Desktop.GNOME.Helpers;

public static class WindowExtensions
{
    public delegate void OpenCallback(nint application, nint[] files, int n_files, nint hint, nint data);

    extension(Gtk.Window window)
    {
        public WindowGeometry WindowGeometry
        {
            get
            {
                window.GetDefaultSize(out int width, out int height);
                return new WindowGeometry(width, height, window.IsMaximized());
            }

            set
            {
                if (value.IsMaximized)
                {
                    window.Maximize();
                }
                else
                {
                    window.SetDefaultSize(value.Width, value.Height);
                }
            }
        }
    }
}
