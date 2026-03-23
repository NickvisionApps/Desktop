using System.Threading.Tasks;
using Tmds.DBus.Protocol;

namespace Nickvision.Desktop.FreeDesktop;

/// <summary>
/// Internal proxy for the org.freedesktop.ScreenSaver D-Bus interface.
/// </summary>
internal static class ScreenSaverProxy
{
    private const string Service = "org.freedesktop.ScreenSaver";
    private const string Path = "/org/freedesktop/ScreenSaver";
    private const string Interface = "org.freedesktop.ScreenSaver";

    /// <summary>
    /// Inhibits the screen saver, preventing system suspend.
    /// </summary>
    /// <param name="connection">The D-Bus connection</param>
    /// <param name="applicationName">The name of the inhibiting application</param>
    /// <param name="reasonForInhibit">The reason for inhibiting</param>
    /// <returns>A cookie that can be used to uninhibit</returns>
    internal static async Task<uint> InhibitAsync(DBusConnection connection, string applicationName, string reasonForInhibit)
    {
        MessageBuffer buffer;
        {
            using var writer = connection.GetMessageWriter();
            writer.WriteMethodCallHeader(Service, Path, Interface, "Inhibit", "ss", MessageFlags.None);
            writer.WriteString(applicationName);
            writer.WriteString(reasonForInhibit);
            buffer = writer.CreateMessage();
        }
        return await connection.CallMethodAsync(buffer, static (Message m, object? _) =>
        {
            var reader = m.GetBodyReader();
            return reader.ReadUInt32();
        }, null);
    }

    /// <summary>
    /// Removes the screen saver inhibit, re-allowing system suspend.
    /// </summary>
    /// <param name="connection">The D-Bus connection</param>
    /// <param name="cookie">The cookie returned by InhibitAsync</param>
    internal static async Task UnInhibitAsync(DBusConnection connection, uint cookie)
    {
        MessageBuffer buffer;
        {
            using var writer = connection.GetMessageWriter();
            writer.WriteMethodCallHeader(Service, Path, Interface, "UnInhibit", "u", MessageFlags.None);
            writer.WriteUInt32(cookie);
            buffer = writer.CreateMessage();
        }
        await connection.CallMethodAsync(buffer);
    }
}
