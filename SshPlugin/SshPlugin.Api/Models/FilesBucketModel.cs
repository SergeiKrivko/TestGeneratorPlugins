namespace SshPlugin.Api.Models;

public class FilesBucketModel
{
    public FileModel[] Files { get; init; } = [];
    public required string HostZipPath { get; init; }
}