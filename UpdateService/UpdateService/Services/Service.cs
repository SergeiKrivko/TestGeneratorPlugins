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

    private static string ReleasesDirectory => Path.Join(AAppService.Instance.AppDataPath, "Temp", "Releases");

    private string ReleaseLocalPath { get; } = Path.Join(ReleasesDirectory, "release");

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
        LatestVersion = new Version(5, 0);
    }

    private async IAsyncEnumerable<AppFile> ListFiles(string root, string path)
    {
        foreach (var file in Directory.EnumerateFiles(path))
        {
            using (var stream = File.OpenRead(file))
            {
                var hashBytes = await _sha256.ComputeHashAsync(stream);
                var fileHash = BitConverter.ToString(hashBytes);
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
            task.Status = "Подготовка";

            var zipUrl = await _httpService.CreateReleaseZip(await ListFiles().ToListAsync(token));
            token.ThrowIfCancellationRequested();

            task.Progress = 40;
            task.Status = "Загрузка";

            Directory.CreateDirectory(ReleasesDirectory);
            var zipPath = Path.Join(ReleasesDirectory, Guid.NewGuid() + ".zip");
            await _httpService.DownloadFile(zipUrl, zipPath);
            token.ThrowIfCancellationRequested();

            task.Progress = 80;
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

    public async Task InstallRelease()
    {
        if (LatestVersion == null || ReleaseDownloaded != DownloadingStatus.Completed)
            return;
        if (OperatingSystem.IsWindows())
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = Path.Join(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Assets",
                    "copy.bat"),
                Arguments =
                    $"\"{ReleaseLocalPath}\" \"{Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location)}\"",
                CreateNoWindow = false,
                UseShellExecute = true,
                Verb = "runas"
            });
        }
        else if (OperatingSystem.IsMacOS())
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "open",
                Arguments =
                    $"-A Terminal -- sudo bash " +
                    $"{Path.Join(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Assets", "copy.sh")} " +
                    $"{Assembly.GetEntryAssembly()?.Location} {ReleaseLocalPath}",
                CreateNoWindow = false,
                UseShellExecute = true,
                Verb = "runas"
            });
        }
        else if (OperatingSystem.IsLinux())
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "gnome-terminal",
                Arguments =
                    $"-- sudo bash " +
                    $"{Path.Join(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Assets", "copy.sh")} " +
                    $"{Assembly.GetEntryAssembly()?.Location} {ReleaseLocalPath}",
                CreateNoWindow = false,
                UseShellExecute = true,
                Verb = "runas"
            });

            // var scriptPath = Path.Join(ReleasesDirectory, "install.sh");
            // await File.WriteAllTextAsync(scriptPath,
            //     $"echo \"Для установки требуются права суперпользователя. Введите пароль:\"\n" +
            //     $"sudo {Path.Join(Assembly.GetExecutingAssembly().Location, "Assets", "UpdateService.Worker.exe")} " +
            //     $"{Assembly.GetEntryAssembly()?.Location} {ReleaseLocalPath}\n" +
            //     $"/opt/SergeiKrivko/TestGenerator/TestGenerator\n");
            // Process.Start(new ProcessStartInfo
            //     { FileName = "gnome-terminal", Arguments = $"-- bash {scriptPath}", CreateNoWindow = true });
        }
    }

    public async Task Update()
    {
        if (LatestVersion == null || !Directory.Exists(ReleaseLocalPath))
            ReleaseDownloaded = DownloadingStatus.Not;
        await GetLatestVersion();
    }
}