using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using AvaluxUI.Utils;
using PluginAdmin.Services;

namespace PluginAdmin.UI;

public partial class UserTab : UserControl
{
    private readonly PluginAdminService _pluginAdminService = Injector.Inject<PluginAdminService>();
    
    public UserTab()
    {
        InitializeComponent();
    }

    public event Action? UserChanged;

    private void SignOutButton_OnClick(object? sender, RoutedEventArgs e)
    {
        _pluginAdminService.Settings.Set("user", null);
        UserChanged?.Invoke();
    }
}