using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using PluginAdmin.Services;

namespace PluginAdmin.UI;

public partial class UserTab : UserControl
{
    public UserTab()
    {
        InitializeComponent();
    }

    public event Action? UserChanged;

    private void SignOutButton_OnClick(object? sender, RoutedEventArgs e)
    {
        PluginAdminService.Instance.Settings.Set("user", null);
        UserChanged?.Invoke();
    }
}