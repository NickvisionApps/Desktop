using System.Text.Json.Serialization;

namespace Nickvision.Desktop.Application;

[JsonSourceGenerationOptions(DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull, WriteIndented = true)]
[JsonSerializable(typeof(AppVersion))]
internal partial class AppVersionJsonContext : JsonSerializerContext
{

}
