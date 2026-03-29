using System.Threading.Tasks;
using Tmds.DBus.Protocol;

namespace Nickvision.Desktop.FreeDesktop;

internal static class ScreenSaverProxy
{
    private const string Service = "org.freedesktop.ScreenSaver";
    private const string Path = "/org/freedesktop/ScreenSaver";
    private const string Interface = "org.freedesktop.ScreenSaver";

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
