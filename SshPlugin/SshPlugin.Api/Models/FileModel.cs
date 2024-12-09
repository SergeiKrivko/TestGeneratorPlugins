using SshPlugin.Api.Services;

namespace SshPlugin.Api.Models;

public class FileModel
{
    public required Guid Id { get; init; }
    public required string OriginPath { get; init; }
    public DateTime? HostUpdateTime { get; set; }
    public bool? HostExists { get; set; }
    public FileOrigin Origin { get; init; }
}