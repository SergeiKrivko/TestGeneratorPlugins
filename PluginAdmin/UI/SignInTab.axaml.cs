using Avalonia.Controls;
using Avalonia.Interactivity;
using PluginAdmin.Models;
using PluginAdmin.Services;

namespace PluginAdmin.UI;

public partial class SignInTab : UserControl
{
    public SignInTab()
    {
        InitializeComponent();
    }

    public event Action? UserChanged;

    private void SignInButton_OnClick(object? sender, RoutedEventArgs e)
    {
        var auth = new AuthModel
            { Login = LoginEdit.Text ?? "", Password = PasswordEdit?.Text ?? "" };
        PluginsHttpService.Instance.Auth = auth;
        PluginAdminService.Instance.Settings.Set("user", auth);
        UserChanged?.Invoke();
    }
}