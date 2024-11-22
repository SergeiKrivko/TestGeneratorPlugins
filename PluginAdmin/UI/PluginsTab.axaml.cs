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
            var plugins = await PluginsHttpService.Instance.GetAllPlugins();
            
            Items.Clear();
            foreach (var plugin in plugins)
            {
                Items.Add(plugin);
            }
        }
        catch (HttpServiceException)
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