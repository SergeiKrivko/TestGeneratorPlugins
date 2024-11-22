using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using PluginAdmin.Exceptions;
using PluginAdmin.Models;
using PluginAdmin.Services;

namespace PluginAdmin.UI;

public partial class PluginsTab : UserControl
{
    public ObservableCollection<PluginRead> Items { get; } = [];
    
    public PluginsTab()
    {
        InitializeComponent();
        PluginsListBox.ItemsSource = Items;
        Update();
    }

    public async void Update()
    {
        try
        {
            Items.Clear();
            foreach (var plugin in await PluginsHttpService.Instance.GetAllPlugins())
            {
                Items.Add(plugin);
            }
        }
        catch (ConnectionException e)
        {
        }
    }

    private async void CreateButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop &&
            desktop.MainWindow != null)
        {
            var dialog = new CreatePluginWindow();
            await dialog.ShowDialog(desktop.MainWindow);
            Update();
        }
    }
}