using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using PluginAdmin.Exceptions;
using PluginAdmin.Models;
using PluginAdmin.Services;

namespace PluginAdmin.UI;

public partial class NewTokenWindow : Window
{
    private ObservableCollection<PluginRead> Plugins { get; } = [];

    public NewTokenWindow()
    {
        InitializeComponent();
        PluginsList.ItemsSource = Plugins;
        Update();
    }

    public async void Update()
    {
        try
        {
            Plugins.Clear();
            foreach (var plugin in await PluginsHttpService.Instance.GetAllPlugins())
            {
                Plugins.Add(plugin);
            }
        }
        catch (ConnectionException e)
        {
        }
    }

    private void CancelButton_OnClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private async void CreateButton_OnClick(object? sender, RoutedEventArgs e)
    {
        var plugins = new List<string>();
        if (PluginsList.SelectedItems != null)
            foreach (var item in PluginsList.SelectedItems)
            {
                if (item is PluginRead plugin)
                    plugins.Add(plugin.Key);
            }

        OptionsView.IsVisible = false;
        SpinnerView.IsVisible = true;

        var token = await PluginsHttpService.Instance.CreateToken(new TokenCreate
            { Name = TokenNameBox.Text ?? "", Plugins = plugins.ToArray() });

        SpinnerView.IsVisible = false;
        CreatedView.IsVisible = true;
        TokenBox.Text = token;
    }
}