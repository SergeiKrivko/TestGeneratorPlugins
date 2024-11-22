using System.Collections.ObjectModel;
using System.Reactive;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using PluginAdmin.Exceptions;
using PluginAdmin.Models;
using PluginAdmin.Services;
using ReactiveUI;

namespace PluginAdmin.UI;

public partial class TokensTab : UserControl
{
    private ObservableCollection<TokenRead> Items { get; } = [];

    // public ReactiveCommand<Guid, Unit> DeleteCommand { get; }
    
    public TokensTab()
    {
        // DeleteCommand = ReactiveCommand.CreateFromTask<Guid>(DeleteToken);
        
        InitializeComponent();
        TokensListBox.ItemsSource = Items;
        Update();
    }
    
    public async void Update()
    {
        var tokens = await PluginsHttpService.Instance.GetAllTokens();
        try
        {
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
            await PluginsHttpService.Instance.DeleteToken(token.TokenId);
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