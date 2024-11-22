using System.Text.Json.Serialization;

namespace PluginAdmin.Models;

public class TokenRead
{
    [JsonPropertyName("tokenId")] public Guid TokenId { get; init; }
    
    [JsonPropertyName("userId")] public Guid UserId { get; init; }

    [JsonPropertyName("name")] public string Name { get; init; } = string.Empty;

    [JsonPropertyName("createdAt")] public required DateTime CreatedAt { get; init; }

    [JsonPropertyName("deletedAt")] public DateTime? DeletedAt { get; init; }
}