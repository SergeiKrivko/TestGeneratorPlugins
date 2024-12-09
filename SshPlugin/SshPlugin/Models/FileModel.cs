namespace SshPlugin.Models;

public class FileModel
{
    public required Guid Id { get; init; }
    public required string OriginPath { get; init; }
    public DateTime? HostUpdateTime { get; init; }
    public bool? HostExists { get; init; }
    public FileOrigin Origin { get; init; }
}