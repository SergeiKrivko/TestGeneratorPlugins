using System.Diagnostics;
using System.IO.Compression;
using System.Reflection;
using System.Security.Cryptography;
using TestGenerator.Shared.Types;
using TestGenerator.Shared.Utils;
using UpdateService.Models;

namespace UpdateService.Services;

public class Service
{
    private readonly ReleasesHttpService _httpService;
    private readonly SettingsSection _settings = UpdateService.Settings;
    private readonly SHA256 _sha256 = SHA256.Create();

    public Version? LatestVersion
    {
        get => _settings.Get<Version>("latestVersion");
        private set => _settings.Set("latestVersion", value);
    }

    public enum DownloadingStatus
    {
        Not,
        InProgress,
        Completed,
    }

    public string[] FilesToDelete
    {
        get => _settings.Get<string[]>("filesToDelete", []);
        private set => _settings.Set("filesToDelete", value);
    }

    private static string ReleasesDirectory => Path.Join(AAppService.Instance.AppDataPath, "Temp", "Releases");

    private string ReleaseLocalPath { get; } = Path.Join(ReleasesDirectory, "release");

    private static string ScriptPath => OperatingSystem.IsWindows()
        ? Path.Join(ReleasesDirectory, "script.bat")
        : Path.Join(ReleasesDirectory, "script.sh");

    private static string AppLocation => Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location) ??
                                         throw new Exception("Invalid program location");

    public DownloadingStatus ReleaseDownloaded
    {
        get => _settings.Get("releaseDownloaded", DownloadingStatus.Not);
        private set => _settings.Set("releaseDownloaded", value);
    }

    public Service()
    {
        _httpService = new ReleasesHttpService();
    }

    private async Task GetLatestVersion()
    {
        LatestVersion = await _httpService.GetLatestVersion();
    }

    private async IAsyncEnumerable<AppFile> ListFiles(string root, string path)
    {
        foreach (var file in Directory.EnumerateFiles(path))
        {
            using (var stream = File.OpenRead(file))
            {
                var hashBytes = await _sha256.ComputeHashAsync(stream);
                var fileHash = BitConverter.ToString(hashBytes).Replace("-", "");
                yield return new AppFile { Filename = Path.GetRelativePath(root, file), Hash = fileHash };
                UpdateService.Logger.Debug($"{file} - {fileHash}");
            }
        }

        foreach (var directory in Directory.EnumerateDirectories(path))
        {
            await foreach (var el in ListFiles(root, directory))
            {
                yield return el;
            }
        }
    }

    private IAsyncEnumerable<AppFile> ListFiles(string path) => ListFiles(path, path);

    private IAsyncEnumerable<AppFile> ListFiles() => ListFiles(
        Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location) ??
        throw new Exception("Invalid program location"));

    public async Task DownloadRelease()
    {
        if (LatestVersion == null)
            return;
        ReleaseDownloaded = DownloadingStatus.InProgress;

        UpdateService.Logger.Debug($"App entrypoint: {Assembly.GetEntryAssembly()?.Location}");

        async Task<int> BackgroundTaskFunc(IBackgroundTask task, CancellationToken token)
        {
            task.Progress = 10;
            task.Status = "Подготовка";

            var zipUrl = (await _httpService.CreateReleaseZip(await ListFiles().ToListAsync(token))).Url;
            token.ThrowIfCancellationRequested();

            task.Progress = 45;
            task.Status = "Загрузка";

            Directory.CreateDirectory(ReleasesDirectory);
            var zipPath = Path.Join(ReleasesDirectory, Guid.NewGuid() + ".zip");
            await _httpService.DownloadFile(zipUrl, zipPath);
            token.ThrowIfCancellationRequested();

            task.Progress = 90;
            task.Status = "Распаковка";

            if (Directory.Exists(ReleaseLocalPath))
                Directory.Delete(ReleaseLocalPath, recursive: true);
            await Task.Run(() => ZipFile.ExtractToDirectory(zipPath, ReleaseLocalPath), token);
            File.Delete(zipPath);

            return 0;
        }

        await AAppService.Instance.RunBackgroundTask("Загрузка обновления", BackgroundTaskFunc).Wait();
        ReleaseDownloaded = DownloadingStatus.Completed;
    }


    private async Task WriteBatchScript()
    {
        await File.WriteAllTextAsync(ScriptPath,
            "@echo off\n" +
            "timeout /t 1 >nul\n\n" +
            await CreateCopyCommands("copy") + "\n\n" +
            CreateDeleteCommands("del", ["unins000.dat", "unins000.exe"]) + "\n" +
            $"del /Q {ReleaseLocalPath}\n" +
            $"start {AppLocation.Replace(":\\", ":\\\"")}\\Nachert.App.exe\"\n"
        );
    }

    private async Task WriteBashScript()
    {
        await File.WriteAllTextAsync(ScriptPath,
            "sleep 1\n\n" +
            await CreateCopyCommands("sudo cp") + "\n\n" +
            CreateDeleteCommands("sudo rm",
            [
                "icon.png", "icon_16.png", "icon_24.png", "icon_32.png", "icon_48.png", "icon_64.png", "icon_128.png",
                "icon_256.png"
            ]) + "\n" +
            $"rm -rf \"{ReleaseLocalPath}\"\n" +
            $"\"{AppLocation}/Nachert.App\"\n"
        );
    }

    private async Task WriteMacosScript()
    {
        await File.WriteAllTextAsync(ScriptPath,
            "sleep 1\n\n" +
            await CreateCopyCommands() + "\n\n" +
            CreateDeleteCommands() + "\n" +
            $"rm -rf \"{ReleaseLocalPath}\"\n" +
            "open -a Nachert\n"
        );
    }

    private async Task<string> CreateCopyCommands(string command = "cp")
    {
        return string.Join('\n', await ListFiles(ReleaseLocalPath)
            .Select(f =>
                $"{command} \"{Path.Join(ReleaseLocalPath, f.Filename)}\" \"{Path.Join(AppLocation, f.Filename)}\"")
            .ToArrayAsync());
    }

    private string CreateDeleteCommands(string command = "rm", string[]? ignore = null)
    {
        return string.Join('\n', FilesToDelete
            .Where(f => ignore == null || !ignore.Contains(f))
            .Select(f => $"{command} \"{Path.Join(AppLocation, f)}\""));
    }

    public async Task InstallRelease()
    {
        if (LatestVersion == null || ReleaseDownloaded != DownloadingStatus.Completed)
            return;
        if (OperatingSystem.IsWindows())
        {
            await WriteBatchScript();
            Process.Start(new ProcessStartInfo
            {
                FileName = ScriptPath,
                CreateNoWindow = false,
                UseShellExecute = true,
                Verb = "runas"
            });
        }
        else if (OperatingSystem.IsLinux())
        {
            await WriteBashScript();
            Process.Start(new ProcessStartInfo
            {
                FileName = "gnome-terminal",
                Arguments =
                    $"-- bash \"{ScriptPath}\""
            });
        }
        else if (OperatingSystem.IsMacOS())
        {
            await WriteMacosScript();
            Process.Start(new ProcessStartInfo
            {
                FileName = "bash",
                Arguments = ScriptPath
            });
        }
    }

    public async Task Update()
    {
        if (LatestVersion <= AAppService.Instance.AppVersion || !Directory.Exists(ReleaseLocalPath))
            ReleaseDownloaded = DownloadingStatus.Not;
        await GetLatestVersion();
    }
}