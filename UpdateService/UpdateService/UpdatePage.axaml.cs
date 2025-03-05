using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using AvaluxUI.Utils;
using TestGenerator.Shared.Types;
using UpdateService.Services;
using Application = Avalonia.Application;

namespace UpdateService;

public partial class UpdatePage : UserControl
{
    private readonly IAppService _appService = Injector.Inject<IAppService>();
    private readonly Service _service;

    public UpdatePage()
    {
        _service = new Service();
        InitializeComponent();
        Update();
    }

    private async void Update()
    {
        try
        {
            CurrentVersionBlock.Text = $"Текущая версия: {_appService.AppVersion}";

            ButtonDownload.IsVisible = false;
            ProgressPanel.IsVisible = true;
            ProgressNameBlock.Text = "Проверка обновления";
            ButtonInstall.IsVisible = false;

            await _service.Update();
            if (_service.LatestVersion > _appService.AppVersion)
            {
                StatusBlock.Text = $"Доступно обновление до {_service.LatestVersion}";
                ButtonDownload.IsVisible = _service.ReleaseDownloaded == Service.DownloadingStatus.Not;
                ButtonInstall.IsVisible = _service.ReleaseDownloaded == Service.DownloadingStatus.Completed;
            }
            else
            {
                StatusBlock.Text = "Установлена последняя версия";
                ButtonDownload.IsVisible = false;
                ButtonInstall.IsVisible = false;
            }

            ProgressPanel.IsVisible = _service.ReleaseDownloaded == Service.DownloadingStatus.InProgress;
        }
        catch (Exception e)
        {
            UpdateService.Logger.Error(e.Message);
        }
    }

    private async Task DownloadRelease()
    {
        ProgressNameBlock.Text = "Загрузка обновления";
        ButtonDownload.IsVisible = false;
        ProgressPanel.IsVisible = true;

        await _service.DownloadRelease();

        ProgressPanel.IsVisible = false;
        ButtonInstall.IsVisible = true;
    }

    private async void InstallRelease()
    {
        await _service.InstallRelease();

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