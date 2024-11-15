using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Octokit;
using TestGenerator.Shared.Types;
using TestGenerator.Shared.Utils;
using Application = Avalonia.Application;

namespace TestGeneratorUpdateService;

public partial class UpdatePage : UserControl
{
    private SettingsSection _settings;
    private GitHubClient _gitHubClient;
    private HttpClient _httpClient;

    private Version? LatestVersion
    {
        get => _settings.Get<Version>("latestVersion");
        set => _settings.Set("latestVersion", value);
    }

    private string? ReleaseUrl
    {
        get => _settings.Get<string>("releaseUrl");
        set => _settings.Set("releaseUrl", value);
    }

    private enum DownloadingStatus
    {
        Not,
        InProgress,
        Completed,
    }

    private string ReleasesDirectory => Path.Join(AAppService.Instance.AppDataPath, "Temp", "Releases");

    private DownloadingStatus ReleaseDownloaded
    {
        get => _settings.Get<DownloadingStatus>("releaseDownloaded", DownloadingStatus.Not);
        set => _settings.Set("releaseDownloaded", value);
    }

    public UpdatePage()
    {
        _settings = AAppService.Instance.Settings.GetSection("testGeneratorUpdateService");
        _gitHubClient = new GitHubClient(new ProductHeaderValue("SergeiKrivko"));
        _httpClient = new HttpClient();
        InitializeComponent();
        Update();
    }

    private string AssetName(Version version)
    {
        var extension = "";
        switch (System.Runtime.InteropServices.RuntimeInformation.RuntimeIdentifier.Split('-')[0])
        {
            case "win":
                extension = ".exe";
                break;
            case "linux":
                extension = ".deb";
                break;
            case "osx":
                extension = ".dmg";
                break;
        }

        var arch = System.Runtime.InteropServices.RuntimeInformation.RuntimeIdentifier.Split('-')[1];
        if (arch == "x64")
            arch = "amd64";
        return $"testgenerator_{version}_{arch}{extension}";
    }

    private async Task GetLatestVersion()
    {
        var githubRelease = await _gitHubClient.Repository.Release.GetLatest("SergeiKrivko", "TestGenerator-cs");
        if (githubRelease.TagName.StartsWith('v'))
        {
            var version = Version.Parse(githubRelease.TagName.AsSpan(1));
            if (version != LatestVersion && version > AAppService.Instance.AppVersion)
            {
                LatestVersion = version;
                var assetName = AssetName(version);
                ReleaseUrl = githubRelease.Assets.Single(a => a.Name == assetName).BrowserDownloadUrl;
                ReleaseDownloaded = DownloadingStatus.Not;
            }
            else if (LatestVersion < AAppService.Instance.AppVersion)
                LatestVersion = AAppService.Instance.AppVersion;
        }
    }

    private async void Update()
    {
        CurrentVersionBlock.Text = $"Текущая версия: {AAppService.Instance.AppVersion}";
        await GetLatestVersion();
        if (LatestVersion > AAppService.Instance.AppVersion)
        {
            StatusBlock.Text = $"Доступно обновление до {LatestVersion}";
            ButtonDownload.IsVisible = ReleaseDownloaded == DownloadingStatus.Not;
            ButtonInstall.IsVisible = ReleaseDownloaded == DownloadingStatus.Completed;
        }
        else
        {
            StatusBlock.Text = "Установлена последняя версия";
            ButtonDownload.IsVisible = false;
            ButtonInstall.IsVisible = false;
        }

        ProgressPanel.IsVisible = ReleaseDownloaded == DownloadingStatus.InProgress;
    }

    private async Task DownloadRelease()
    {
        ButtonDownload.IsVisible = false;
        ProgressPanel.IsVisible = true;
        ReleaseDownloaded = DownloadingStatus.InProgress;

        if (LatestVersion == null || ReleaseUrl == null)
            return;
        Console.WriteLine($"$Downloading {ReleaseUrl}");
        var stream = await _httpClient.GetStreamAsync(ReleaseUrl);
        Directory.CreateDirectory(ReleasesDirectory);
        await stream.CopyToAsync(File.Create(Path.Join(ReleasesDirectory, AssetName(LatestVersion))));

        ProgressPanel.IsVisible = false;
        ButtonInstall.IsVisible = true;
        ReleaseDownloaded = DownloadingStatus.Completed;
    }

    private void InstallRelease()
    {
        if (LatestVersion == null || ReleaseDownloaded != DownloadingStatus.Completed)
            return;
        if (OperatingSystem.IsWindows())
        {
            Process.Start(new ProcessStartInfo
                { FileName = Path.Join(ReleasesDirectory, AssetName(LatestVersion)), CreateNoWindow = true });
        }

        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime lifetime)
            lifetime.Shutdown();
    }

    private void RefreshButton_OnClick(object? sender, RoutedEventArgs e)
    {
        Update();
    }

    private async void ButtonDownload_OnClick(object? sender, RoutedEventArgs e)
    {
        await DownloadRelease();
    }

    private void ButtonInstall_OnClick(object? sender, RoutedEventArgs e)
    {
        InstallRelease();
    }
}