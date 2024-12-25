using System.Text.Json.Serialization;

namespace UpdateService.Models;

public class ZipResponseModel
{
    [JsonPropertyName("url")] public required string Url { get; init; }
    [JsonPropertyName("deletedFiles")] public string[] DeletedFiles { get; init; } = [];
}