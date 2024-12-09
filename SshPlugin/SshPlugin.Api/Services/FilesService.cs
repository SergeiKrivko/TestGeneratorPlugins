using System.IO.Compression;
using SshPlugin.Api.Models;

namespace SshPlugin.Api.Services;

public class FilesService
{
    public Guid ConnectionId { get; set; }

    public string DataPath { get; } =
        Path.GetFullPath(Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SergeiKrivko",
            "TestGenerator",
            "PluginData", "SSH", "HostData"));

    public string GetTempFilePath(string? extension = null)
    {
        Directory.CreateDirectory(Path.Join(DataPath, "Temp"));
        var path = Path.Join(DataPath, "Temp", Guid.NewGuid().ToString());
        if (extension != null)
            path = Path.ChangeExtension(path, extension);
        return path;
    }

    public string GetPath(FileModel file)
    {
        if (file.Origin == FileOrigin.Host)
            return file.OriginPath;

        var path = file.OriginPath.Replace('\\', '/').Trim('/');
        if (path[1] == ':')
            path = '/' + path[0].ToString() + path.Substring(2);
        return Path.Join(DataPath, ConnectionId.ToString(), path);
    }

    public string GetPath(string file, FileOrigin origin)
    {
        if (origin == FileOrigin.Host)
            return file;

        var path = file.Replace('\\', '/').Trim('/');
        if (path[1] == ':')
            path = '/' + path[0].ToString() + path.Substring(2);
        return Path.Join(DataPath, ConnectionId.ToString(), path);
    }

    public FileOrigin GetOrigin(string path) => path.StartsWith(DataPath) ? FileOrigin.Client : FileOrigin.Host;

    public string GetOriginPath(string path)
    {
        if (GetOrigin(path) == FileOrigin.Client)
        {
            var clientPath = path.Substring(Path.Join(DataPath, ConnectionId.ToString()).Length);
            if (clientPath[2] == '/')
                clientPath = clientPath[1] + ":\\" + clientPath.Substring(3);
            else
                clientPath = clientPath.Replace('\\', '/');
            return clientPath;
        }

        return path;
    }

    public string ParseCommand(string command)
    {
        var newCommand = command;
        var index = 0;
        do
        {
            index = command.IndexOf("#ssh-origin-path{", index, StringComparison.InvariantCulture);
            if (index < 0) break;
            index += "#ssh-origin-path{".Length;
            var endIndex = command.IndexOf('}', index);
            var substring = command.Substring(index, endIndex - index);
            var origin = substring[0] == '0' ? FileOrigin.Client : FileOrigin.Host;
            var originPath = substring.Substring(2);
            newCommand = newCommand.Replace($"#ssh-origin-path{{{substring}}}", GetPath(originPath, origin));
            index++;
        } while (index < command.Length);

        return newCommand;
    }

    public async Task<FilesBucketModel> UnpackBucket(FilesBucketModel bucket)
    {
        var extractedPath = GetTempFilePath();
        Console.WriteLine($"Extracting {bucket.HostZipPath} to {extractedPath}");
        await Task.Run(() => ZipFile.ExtractToDirectory(bucket.HostZipPath, extractedPath));
        File.Delete(bucket.HostZipPath);
        foreach (var fileModel in bucket.Files)
        {
            var newPath = GetPath(fileModel);
            Directory.CreateDirectory(Path.GetDirectoryName(newPath) ?? "");
            File.Move(Path.Join(extractedPath, fileModel.Id.ToString()), newPath, overwrite: true);
            fileModel.HostUpdateTime = File.GetLastWriteTimeUtc(newPath);
            fileModel.HostExists = true;
        }

        Directory.Delete(extractedPath, recursive: true);

        return bucket;
    }

    public FileModel[] Fetch(FileModel[] files)
    {
        foreach (var file in files)
        {
            var path = GetPath(file);
            file.HostExists = File.Exists(path);
            file.HostUpdateTime = file.HostExists == true ? File.GetLastWriteTimeUtc(path) : null;
        }

        return files;
    }

    public FileModel[] List(string rootPath, FileOrigin origin = FileOrigin.Host)
    {
        var res = new List<FileModel>();
        foreach (var path in WalkFiles(GetPath(new FileModel { Origin = origin, OriginPath = rootPath, Id = default })))
        {
            res.Add(new FileModel
            {
                Origin = origin, Id = Guid.NewGuid(), HostExists = true, OriginPath = GetOriginPath(path),
                HostUpdateTime = File.GetLastWriteTimeUtc(path)
            });
        }

        return res.ToArray();
    }

    private IEnumerable<string> WalkFiles(string rootPath)
    {
        if (Directory.Exists(rootPath))
        {
            foreach (var d in Directory.GetDirectories(rootPath))
            {
                foreach (var e in WalkFiles(d))
                {
                    yield return e;
                }
            }

            foreach (var d in Directory.GetFiles(rootPath))
            {
                yield return d;
            }
        }
        else
            yield return rootPath;
    }

    public async Task<FilesBucketModel> PackBucket(FileModel[] files)
    {
        var zipPath = GetTempFilePath(".zip");
        var bucket = new FilesBucketModel { Files = files, HostZipPath = zipPath };

        var zip = ZipFile.Open(zipPath, ZipArchiveMode.Create);
        foreach (var file in files)
        {
            var path = GetPath(file);
            file.HostExists = File.Exists(path);
            if (file.HostExists == true)
            {
                file.HostUpdateTime = File.GetLastWriteTimeUtc(path);
                await Task.Run(() => zip.CreateEntryFromFile(path, file.Id.ToString()));
            }
        }

        zip.Dispose();

        return bucket;
    }
}