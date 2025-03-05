using AvaluxUI.Utils;
using PluginAdmin.Models;
using PluginAdmin.Services;
using TestGenerator.Shared.Types;

namespace PluginAdmin.UI;

public partial class PluginAdminTab : MainTab
{
    private readonly PluginAdminService _pluginAdminService = Injector.Inject<PluginAdminService>();
    private readonly PluginsHttpService _pluginsHttpService = Injector.Inject<PluginsHttpService>();

    public override string TabKey => "PluginAdmin";
    public override string TabName => "Plugin Admin";

    public PluginAdminTab()
    {
        InitializeComponent();
        SignInView.UserChanged += Update;
        UserView.UserChanged += Update;

        Update();
    }

    private void Update()
    {
        var user = _pluginAdminService.Settings.Get<AuthModel>("user");
        _pluginsHttpService.Auth = user;

        SignInTab.IsVisible = user == null;
        SignInTab.IsSelected = user == null;
        UserTab.IsVisible = user != null;
        UserTab.Header = user?.Login;
        UserTab.IsSelected = user != null;
        PluginsTab.IsVisible = user != null;
        (PluginsTab.Content as PluginsTab)?.Update();
        TokensTab.IsVisible = user != null;
        (TokensTab.Content as TokensTab)?.Update();
    }
}