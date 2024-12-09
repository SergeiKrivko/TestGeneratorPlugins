using SQLite;

namespace SshPlugin.Models;

public class FileEntity
{
    [PrimaryKey] public Guid Id { get; init; }
    public Guid ConnectionId { get; init; }
    public FileOrigin Origin { get; init; }
    public string OriginPath { get; init; }
    public string LocalPath { get; init; }
    public DateTime? LocalUpdateTime { get; set; }
    public DateTime? HostUpdateTime { get; set; }

    public FileModel ToModel()
    {
        return new FileModel { Id = Id, OriginPath = OriginPath, Origin = Origin, HostUpdateTime = HostUpdateTime };
    }

    public static FileEntity FromModel(Guid connectionId, FileModel m, string localPath)
    {
        return new FileEntity
        {
            ConnectionId = connectionId,
            Id = m.Id, OriginPath = m.OriginPath, Origin = m.Origin, HostUpdateTime = m.HostUpdateTime,
            LocalPath = localPath
        };
    }
}