using Renci.SshNet;

namespace SshPlugin.Services;

public class SftpService
{
    private readonly SftpClient _sftpClient;

    public SftpService(SftpClient client)
    {
        _sftpClient = client;
    }

    public async Task UploadFile(string srcPath, string dstPath)
    {
        var file = File.OpenRead(srcPath);
        await Task.Run(() => _sftpClient.UploadFile(file, dstPath.Replace('\\', '/')));
        file.Close();
    }

    public async Task DownloadFile(string srcPath, string dstPath)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(dstPath) ?? "");
        var file = File.OpenWrite(dstPath);
        await Task.Run(() => _sftpClient.DownloadFile(srcPath, file));
        file.Close();
    }

    public async Task<bool> Exists(string path)
    {
        return await _sftpClient.ExistsAsync(path);
    }

    public async Task CreateDirectory(string path)
    {
        await _sftpClient.CreateDirectoryAsync(path.Replace('\\', '/'));
    }
}