using Avalonia.Controls;
using Avalonia.Interactivity;
using AvaluxUI.Utils;
using PluginAdmin.Models;
using PluginAdmin.Services;

namespace PluginAdmin.UI;

public partial class SignInTab : UserControl
{
    private readonly PluginAdminService _pluginAdminService = Injector.Inject<PluginAdminService>();
    private readonly PluginsHttpService _pluginsHttpService = Injector.Inject<PluginsHttpService>();

    public SignInTab()
    {
        InitializeComponent();
    }

    public event Action? UserChanged;

    private void SignInButton_OnClick(object? sender, RoutedEventArgs e)
    {
        var auth = new AuthModel
            { Login = LoginEdit.Text ?? "", Password = PasswordEdit?.Text ?? "" };
        _pluginsHttpService.Auth = auth;
        _pluginAdminService.Settings.Set("user", auth);
        UserChanged?.Invoke();
    }
}