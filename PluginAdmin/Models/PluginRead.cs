using System.Text.Json.Serialization;

namespace PluginAdmin.Models;

public class PluginRead
{
    [JsonPropertyName("pluginId")] public required Guid PluginId { get; set; }
    [JsonPropertyName("ownerId")] public required Guid OwnerId { get; set; }

    [JsonPropertyName("key")] public required string Key { get; set; } = string.Empty;
    [JsonPropertyName("createdAt")] public required DateTime CreatedAt { get; set; }
    [JsonPropertyName("deletedAt")] public DateTime? DeletedAt { get; set; }
}