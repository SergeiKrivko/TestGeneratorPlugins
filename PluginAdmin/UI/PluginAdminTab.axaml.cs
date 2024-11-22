using Avalonia.Interactivity;
using PluginAdmin.Models;
using PluginAdmin.Services;
using TestGenerator.Shared.Types;

namespace PluginAdmin.UI;

public partial class PluginAdminTab : MainTab
{
    public override string TabKey => "PluginAdmin";
    public override string TabName => "Plugin Admin";

    public PluginAdminTab()
    {
        PluginAdminService.Init();
        InitializeComponent();
    }

    private void CheckAuth()
    {
        var user = PluginAdminService.Instance.Settings.Get<AuthModel>("user");
        if (user == null)
        {
            SignInTab.IsVisible = true;
            UserTab.IsVisible = false;
            PluginsTab.IsVisible = false;
            TokensTab.IsVisible = false;
        }
        else
        {
            SignInTab.Header = user.Login;
        }
    }

    private void SignInButton_OnClick(object? sender, RoutedEventArgs e)
    {
        var auth = new AuthModel
            { Login = LoginEdit.Text ?? "", Password = PasswordEdit?.Text ?? "" };
        PluginsHttpService.Instance.Auth = auth;
        PluginAdminService.Instance.Settings.Set("user", auth);

        SignInTab.IsVisible = false;
        SignInTab.Header = LoginEdit.Text;
        UserTab.IsVisible = true;
        PluginsTab.IsVisible = true;
        TokensTab.IsVisible = true;

        (PluginsTab.Content as PluginsTab)?.Update();
        (TokensTab.Content as TokensTab)?.Update();
    }

    private void SignOutButton_OnClick(object? sender, RoutedEventArgs e)
    {
        PluginAdminService.Instance.Settings.Set("user", null);
        SignInTab.IsVisible = true;
        UserTab.IsVisible = false;
        PluginsTab.IsVisible = false;
        TokensTab.IsVisible = false;
    }
}