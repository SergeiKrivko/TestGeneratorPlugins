using System.IO.Compression;
using System.Text.Json;
using Octokit;
using Renci.SshNet;
using TestGenerator.Shared.Types;

namespace SshPlugin.Services;

internal class InstallationService
{
    private readonly SshClient _sshClient;
    private readonly SftpService _sftpService;

    private readonly GitHubClient _gitHubClient = new GitHubClient(new ProductHeaderValue("SergeiKrivko"));
    private readonly HttpClient _httpClient = new HttpClient();

    private Version? _latestVersion;
    private string? _releaseUrl;
    private SshConnection _connection;

    internal InstallationService(SshConnection connection, SshClient sshClient, SftpService sftpService)
    {
        _connection = connection;
        _sshClient = sshClient;
        _sftpService = sftpService;
    }

    private string ProgramExeName => _connection.HostProgramPath + '/' + (_connection.OperatingSystem == "win-x64"
        ? "SshPlugin.Api.exe"
        : "SshPlugin.Api");

    private string AssetName => $"ssh-api_{_connection.OperatingSystem}.zip";

    private string ProgramVersionFileName => _connection.HostProgramPath + '/' + "version.txt";

    private async Task<Version?> GetInstalledVersion()
    {
        if (string.IsNullOrEmpty(_connection.HostProgramPath))
            return null;
        if (!await _sftpService.Exists(ProgramExeName) || !await _sftpService.Exists(ProgramVersionFileName))
            return null;
        var versionTxtPath = Path.Join(SshPlugin.DataPath, "Temp", "version.txt");
        await _sftpService.DownloadFile(ProgramVersionFileName, versionTxtPath);
        var version = Version.Parse(await File.ReadAllTextAsync(versionTxtPath));
        File.Delete(versionTxtPath);
        return version;
    }

    private async Task<Version> GetLatestVersion()
    {
        var githubRelease =
            (await _gitHubClient.Repository.Release.GetAll("SergeiKrivko", "TestGeneratorPlugins")).Last(r =>
                r.TagName.StartsWith("SshApi-"));
        _latestVersion = Version.Parse(githubRelease.TagName.AsSpan("SshApi-".Length));
        _releaseUrl = githubRelease.Assets.Single(a => a.Name == AssetName).BrowserDownloadUrl;
        return _latestVersion;
    }

    private async Task<string?> DownloadRelease()
    {
        if (_latestVersion == null)
            await GetLatestVersion();
        if (_latestVersion == null || _releaseUrl == null)
            return null;

        var stream = await _httpClient.GetStreamAsync(_releaseUrl);
        Directory.CreateDirectory(Path.Join(SshPlugin.DataPath, "Temp"));
        var downloadedPath = Path.Join(SshPlugin.DataPath, "Temp", Guid.NewGuid() + ".zip");
        var file = File.Create(downloadedPath);
        await stream.CopyToAsync(file);
        file.Close();
        var extractedPath = Path.Join(SshPlugin.DataPath, "Temp", Guid.NewGuid().ToString());
        ZipFile.ExtractToDirectory(downloadedPath, extractedPath);
        File.Delete(downloadedPath);
        return extractedPath;
    }

    private async Task<int> InstallRelease(IBackgroundTask task)
    {
        try
        {
            task.Status = "Загрузка ПО";
            var localPath = await DownloadRelease();
            task.Progress = 45;
            if (string.IsNullOrEmpty(localPath))
                return -1;

            task.Status = "Создание директории";
            await _sftpService.CreateDirectory(_connection.HostProgramPath);
            task.Progress = 50;

            var files = Directory.GetFiles(localPath);
            var i = 0;
            foreach (var file in files)
            {
                task.Status = $"Установка {Path.GetFileName(file)}";
                await _sftpService.UploadFile(file, Path.Join(_connection.HostProgramPath, Path.GetFileName(file)));
                i++;
                task.Progress = 50 + 50 * i / files.Length;
            }
        }
        catch (Exception e)
        {
            SshPlugin.Logger.Error($"Ошибка при установке ПО: {e.Message}");
            return 1;
        }

        return 0;
    }

    public async Task<ShellStream?> StartProgram()
    {
        var installedVersion = await GetInstalledVersion();
        SshPlugin.Logger.Debug(installedVersion == null
            ? "Установленного ПО не обнаружено"
            : $"Обнаружена установленная версия ПО: {installedVersion}");
        if (installedVersion == null || _connection.Autoupdate)
        {
            var latestVersion = await GetLatestVersion();
            if (latestVersion > installedVersion)
            {
                var status = await AAppService.Instance
                    .RunBackgroundTask($"Установка ПО на '{_connection.Name}'", InstallRelease).Wait();
                if (status != 0)
                    return null;
            }
        }

        return await RunProgram();
    }

    private async Task<ShellStream> RunProgram()
    {
        var stream = _sshClient.CreateShellStreamNoTerminal(bufferSize: 1024);
        if (_connection.OperatingSystem == "win-x64")
            stream.WriteLine("set DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1");
        else
        {
            stream.WriteLine("export DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1");
            stream.WriteLine($"chmod 755 \"{ProgramExeName}\"");
        }

        stream.WriteLine($"\"{ProgramExeName}\"");
        await stream.FlushAsync();
        return stream;
    }

    private async Task<string> RunCommand(string command)
    {
        var c = _sshClient.CreateCommand(command);
        await c.ExecuteAsync();
        return c.Result;
    }

    public async Task<string> DetectOperatingSystem()
    {
        var res = await RunCommand("uname");
        SshPlugin.Logger.Debug($"Uname output: {JsonSerializer.Serialize(res)}");
        if (res.Contains("Linux"))
            return "linux-x64";
        if (res.Contains("Darwin"))
            return "osx-x64";
        return "win-x64";
    }

    public async Task<string> DetectProgramPath()
    {
        switch (_connection.OperatingSystem)
        {
            case "win-x64":
                return (await RunCommand("echo %APPDATA%\\SergeiKrivko\\SshApi")).Trim();
            case "linux-x64":
                // return "~/.local/share/SergeiKrivko/SshApi";
                return "/opt/SergeiKrivko/SshApi";
            case "osx-x64":
                return (await RunCommand("echo \"/Users/$USER/Library/Application Support/SergeiKrivko/SshApi\""))
                    .Trim();
            default:
                return "";
        }
    }
}