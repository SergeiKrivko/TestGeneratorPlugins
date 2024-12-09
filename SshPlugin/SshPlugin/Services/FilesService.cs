using System.IO.Compression;
using System.Text.Json;
using SshPlugin.Models;

namespace SshPlugin.Services;

public class FilesService
{
    private readonly Repository _repository;
    private readonly SshBridgeService _bridgeService;
    private readonly SftpService _sftpService;

    internal FilesService(Guid connectionId, Repository repository, SshBridgeService bridgeService,
        SftpService sftpClient)
    {
        ConnectionId = connectionId;
        _repository = repository;
        _bridgeService = bridgeService;
        _sftpService = sftpClient;
    }

    private Guid ConnectionId { get; }
    private string DataPath => SshPlugin.DataPath;

    public string GetPath(FileModel file)
    {
        if (file.Origin == FileOrigin.Client)
            return file.OriginPath;

        var path = file.OriginPath.Replace('\\', '/').Trim('/');
        if (path[1] == ':')
            path = '/' + path[0].ToString() + path.Substring(2);
        return Path.Join(DataPath, ConnectionId.ToString(), path);
    }

    public FileOrigin GetOrigin(string path) => path.StartsWith(DataPath) ? FileOrigin.Host : FileOrigin.Client;

    public string GetOriginPath(string path)
    {
        if (GetOrigin(path) == FileOrigin.Host)
        {
            var hostPath = path.Substring(Path.Join(DataPath, ConnectionId.ToString()).Length);
            if (hostPath[2] == '/')
                hostPath = hostPath[1] + ":\\" + hostPath.Substring(3);
            else
                hostPath = hostPath.Replace('\\', '/');
            return hostPath;
        }

        return path;
    }

    public async Task PushFiles(ICollection<string> filePaths)
    {
        var files = (await FilterFiles(filePaths)).ToArray();
        var fetchedFiles = await _bridgeService.GetFetch(files);
        // var filesToPush = files
        //     .Join(fetchedFiles, f => f.Id, f => f.Id,
        //         (f1, f2) => f2.HostUpdateTime > f1.HostUpdateTime || f2.HostExists != true
        //             ? f2
        //             : new FileModel { Id = f1.Id, OriginPath = "" })
        //     .Where(f => !string.IsNullOrEmpty(f.OriginPath)).ToArray();
        var filesToPush = fetchedFiles;
        if (filesToPush.Length == 0)
            return;

        var zipPath = await PackFiles(filesToPush);

        var hostZipPath = await _bridgeService.GetTemp(".zip");

        await _sftpService.UploadFile(zipPath, hostZipPath);
        File.Delete(zipPath);

        await _bridgeService.PostUnpackBucket(new FilesBucketModel { Files = filesToPush, HostZipPath = hostZipPath });
        SshPlugin.Logger.Info($"Pushed: {string.Join(", ", filesToPush.Select(f => f.OriginPath))}");
    }

    private async Task<List<FileModel>> FilterFiles(ICollection<string> files)
    {
        var res = new List<FileModel>();
        var set = new HashSet<string>();
        foreach (var path in WalkFiles(files))
        {
            try
            {
                if (set.Contains(path))
                    continue;
                set.Add(path);
                var file = await _repository.GetFileByLocalPath(path);
                if (File.GetLastWriteTimeUtc(file.LocalPath) == file.LocalUpdateTime)
                    continue;
                file.LocalUpdateTime = File.GetLastWriteTimeUtc(path);
                await _repository.UpdateFile(file);
                res.Add(file.ToModel());
            }
            catch (Exception)
            {
                var id = Guid.NewGuid();
                await _repository.InsertFile(new FileEntity
                {
                    Id = id, OriginPath = path, Origin = FileOrigin.Client, LocalPath = path,
                    ConnectionId = ConnectionId, LocalUpdateTime = File.GetLastWriteTimeUtc(path)
                });
                res.Add(new FileModel { Id = id, OriginPath = path, Origin = FileOrigin.Client });
            }
        }

        return res;
    }

    private IEnumerable<string> WalkFiles(ICollection<string> files)
    {
        foreach (var el in files)
        {
            if (Directory.Exists(el))
            {
                foreach (var d in Directory.GetDirectories(el))
                {
                    foreach (var e in WalkFiles([d]))
                    {
                        yield return e;
                    }
                }

                foreach (var d in Directory.GetFiles(el))
                {
                    yield return d;
                }
            }
            else
                yield return el;
        }
    }

    private async Task<string> PackFiles(ICollection<FileModel> files)
    {
        var dstPath = Path.Join(DataPath, Guid.NewGuid() + ".zip");
        var zip = ZipFile.Open(dstPath, ZipArchiveMode.Create);
        foreach (var file in files)
        {
            var entity = await _repository.GetFile(file.Id);
            await Task.Run(() => zip.CreateEntryFromFile(entity.LocalPath, entity.Id.ToString()));
        }

        zip.Dispose();

        return dstPath;
    }

    private async Task<FileModel?> CompareFiles(FileModel hostFile)
    {
        try
        {
            var entity = await _repository.GetFileByLocalPath(hostFile.OriginPath);
            if (entity.HostUpdateTime >= hostFile.HostUpdateTime && File.Exists(entity.LocalPath))
                return null;
            entity.HostUpdateTime = hostFile.HostUpdateTime;
            await _repository.UpdateFile(entity);
            return entity.ToModel();
        }
        catch (Exception)
        {
            var entity = FileEntity.FromModel(ConnectionId, hostFile, GetPath(hostFile));
            await _repository.InsertFile(entity);
            return hostFile;
        }
    }

    public async Task PullFiles(ICollection<string> filePaths)
    {
        var filesToPull = new HashSet<FileModel>();
        foreach (var filePath in filePaths)
        {
            foreach (var hostFile in await _bridgeService.GetList(filePath, GetOrigin(filePath)))
            {
                var localFile = await CompareFiles(hostFile);
                if (localFile != null)
                    filesToPull.Add(localFile);
            }
        }

        if (filesToPull.Count == 0)
            return;

        var bucket = await _bridgeService.PostPackBucket(filesToPull.ToArray());

        var zipPath = Path.Join(DataPath, Guid.NewGuid() + ".zip");
        Directory.CreateDirectory(DataPath);
        await _sftpService.DownloadFile(bucket.HostZipPath, zipPath);

        await UnpackFiles(zipPath, bucket);
        File.Delete(zipPath);

        SshPlugin.Logger.Info($"Pulled: {string.Join(", ", filesToPull.Select(f => f.OriginPath))}");
    }

    private async Task UnpackFiles(string zipPath, FilesBucketModel bucket)
    {
        var zip = ZipFile.Open(zipPath, ZipArchiveMode.Read);
        foreach (var file in bucket.Files)
        {
            var entity = await _repository.GetFile(file.Id);
            Directory.CreateDirectory(Path.GetDirectoryName(entity.LocalPath) ?? "");
            if (File.Exists(entity.LocalPath))
                File.Delete(entity.LocalPath);
            await Task.Run(() => zip.GetEntry(entity.Id.ToString())?.ExtractToFile(entity.LocalPath));
            entity.LocalUpdateTime = File.GetLastWriteTimeUtc(entity.LocalPath);
            await _repository.UpdateFile(entity);
        }

        zip.Dispose();
    }
}