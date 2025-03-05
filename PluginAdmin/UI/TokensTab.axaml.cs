using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using AvaluxUI.Utils;
using PluginAdmin.Exceptions;
using PluginAdmin.Models;
using PluginAdmin.Services;

namespace PluginAdmin.UI;

public partial class TokensTab : UserControl
{
    private readonly PluginsHttpService _pluginsHttpService = Injector.Inject<PluginsHttpService>();
    private ObservableCollection<TokenRead> Items { get; } = [];

    public TokensTab()
    {
        InitializeComponent();
        TokensListBox.ItemsSource = Items;
        Update();
    }

    public async void Update()
    {
        try
        {
            var tokens = await _pluginsHttpService.GetAllTokens();
            Items.Clear();
            foreach (var plugin in tokens)
            {
                Items.Add(plugin);
            }
        }
        catch (HttpServiceException)
        {
        }
    }

    private void RefreshButton_OnClick(object? sender, RoutedEventArgs e)
    {
        Update();
    }

    private async void DeleteButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (TokensListBox.SelectedItem is TokenRead token)
        {
            await _pluginsHttpService.DeleteToken(token.TokenId);
            Update();
        }
    }

    private async void CreateButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop &&
            desktop.MainWindow != null)
        {
            var dialog = new NewTokenWindow();
            await dialog.ShowDialog(desktop.MainWindow);
            Update();
        }
    }
}