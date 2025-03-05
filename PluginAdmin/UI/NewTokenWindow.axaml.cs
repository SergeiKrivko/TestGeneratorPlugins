using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Interactivity;
using AvaluxUI.Utils;
using PluginAdmin.Exceptions;
using PluginAdmin.Models;
using PluginAdmin.Services;

namespace PluginAdmin.UI;

public partial class NewTokenWindow : Window
{
    private readonly PluginsHttpService _pluginsHttpService = Injector.Inject<PluginsHttpService>();

    private ObservableCollection<PluginRead> Plugins { get; } = [];

    public NewTokenWindow()
    {
        InitializeComponent();
        PluginsList.ItemsSource = Plugins;
        Update();
        TokenTypeBox.SelectedIndex = 0;
    }

    private async void Update()
    {
        try
        {
            Plugins.Clear();
            PermissionsBox.ItemsSource =
                (await _pluginsHttpService.GetTokenPermissions()).Where(p => p.TokenTypes.Contains(GetSelectedType()));
            foreach (var plugin in await _pluginsHttpService.GetAllPlugins())
            {
                Plugins.Add(plugin);
            }
        }
        catch (ConnectionException)
        {
        }
    }

    private void CancelButton_OnClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private TokenType GetSelectedType() => TokenTypeBox.SelectedIndex switch
    {
        0 => TokenType.User,
        1 => TokenType.Mask,
        2 => TokenType.Plugins,
        _ => TokenType.User,
    };

    private async void CreateButton_OnClick(object? sender, RoutedEventArgs e)
    {
        var plugins = new List<string>();
        if (PluginsList.SelectedItems != null)
            foreach (var item in PluginsList.SelectedItems)
            {
                if (item is PluginRead plugin)
                    plugins.Add(plugin.Key);
            }

        var permissions = new List<string>();
        if (PermissionsBox.SelectedItems != null)
            foreach (var item in PermissionsBox.SelectedItems)
            {
                if (item is TokenPermission permission)
                    permissions.Add(permission.Key);
            }

        OptionsView.IsVisible = false;
        SpinnerView.IsVisible = true;

        var token = await _pluginsHttpService.CreateToken(new TokenCreate
        {
            Name = TokenNameBox.Text ?? "", Plugins = plugins.ToArray(), Type = GetSelectedType(),
            Permissions = permissions.ToArray(), Mask = MaskBox.Text
        });

        SpinnerView.IsVisible = false;
        CreatedView.IsVisible = true;
        TokenBox.Text = token;
    }

    private async void TokenTypeBox_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        MaskPanel.IsVisible = TokenTypeBox.SelectedIndex == 1;
        PluginsList.IsVisible = TokenTypeBox.SelectedIndex == 2;
        PermissionsBox.ItemsSource =
            (await _pluginsHttpService.GetTokenPermissions()).Where(p => p.TokenTypes.Contains(GetSelectedType()));
    }
}