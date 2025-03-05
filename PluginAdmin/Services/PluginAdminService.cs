using AvaluxUI.Utils;
using PluginAdmin.Models;
using TestGenerator.Shared.Types;
using TestGenerator.Shared.Utils;

namespace PluginAdmin.Services;

public class PluginAdminService
{
    private readonly IAppService _appService = Injector.Inject<IAppService>();
    private readonly PluginsHttpService _pluginsHttpService = Injector.Inject<PluginsHttpService>();

    public ISettingsSection Settings { get; }

    public PluginAdminService()
    {
        Settings = _appService.GetSettings();
        _pluginsHttpService.Auth = Settings.Get<AuthModel>("user");
    }
}