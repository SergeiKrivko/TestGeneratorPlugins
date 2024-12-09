using SshPlugin.Api.Models;
using SshPlugin.Api.Services;
using StdioBridge.Api.Attributes;

namespace SshPlugin.Api.Controllers;

[BridgeController("api/v1/files")]
public class FilesController
{
    private readonly FilesService _filesService;

    public FilesController(FilesService filesService)
    {
        _filesService = filesService;
    }

    [BridgePost("unpack")]
    public async Task<FilesBucketModel> PostUnpackHandler([FromBody] FilesBucketModel bucket)
    {
        return await _filesService.UnpackBucket(bucket);
    }

    [BridgePost("pack")]
    public async Task<FilesBucketModel> PostUnpackHandler([FromBody] FileModel[] files)
    {
        return await _filesService.PackBucket(files);
    }

    [BridgeGet("list")]
    public async Task<FileModel[]> GetListHandler([FromQuery] string path, [FromQuery] bool hostPath = true)
    {
        return await Task.Run(() => _filesService.List(path, hostPath ? FileOrigin.Host : FileOrigin.Client));
    }

    [BridgeGet("temp")]
    public async Task<string> GetTempHandler([FromQuery] string? extension = null)
    {
        return _filesService.GetTempFilePath(extension);
    }

    [BridgeGet("fetch")]
    public async Task<FileModel[]> GetFetchHandler([FromBody] FileModel[] files)
    {
        return _filesService.Fetch(files);
    }
}