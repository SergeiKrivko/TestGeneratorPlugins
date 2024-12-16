using System.Text.Json.Serialization;

namespace PluginAdmin.Models;

public class TokenPermission
{
    [JsonPropertyName("key")] public required string Key { get; init; }
    [JsonPropertyName("description")] public required string Description { get; init; }
    [JsonPropertyName("tokenTypes")] public required TokenType[] TokenTypes { get; init; }
}