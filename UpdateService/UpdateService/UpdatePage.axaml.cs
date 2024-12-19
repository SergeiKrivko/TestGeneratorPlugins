using System.Diagnostics;
using System.Net.Http.Json;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using TestGenerator.Shared.Types;
using TestGenerator.Shared.Utils;
using UpdateService.Services;
using Application = Avalonia.Application;

namespace UpdateService;

public partial class UpdatePage : UserControl
{
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
            CurrentVersionBlock.Text = $"Текущая версия: {AAppService.Instance.AppVersion}";
            await _service.Update();
            if (_service.LatestVersion > AAppService.Instance.AppVersion)
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