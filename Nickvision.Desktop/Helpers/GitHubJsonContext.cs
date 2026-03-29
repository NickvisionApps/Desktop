using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Nickvision.Desktop.Helpers;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower, WriteIndented = true)]
[JsonSerializable(typeof(List<GitHubRelease>))]
[JsonSerializable(typeof(GitHubReleaseAsset))]
internal partial class GitHubJsonContext : JsonSerializerContext
{

}