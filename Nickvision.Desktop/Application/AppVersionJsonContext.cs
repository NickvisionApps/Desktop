using System.Text.Json.Serialization;

namespace Nickvision.Desktop.Application;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower, WriteIndented = true)]
[JsonSerializable(typeof(AppVersion))]
internal partial class AppVersionJsonContext : JsonSerializerContext
{

}
