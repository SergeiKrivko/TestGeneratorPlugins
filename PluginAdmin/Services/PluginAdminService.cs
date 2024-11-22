using PluginAdmin.Models;
using TestGenerator.Shared.Types;
using TestGenerator.Shared.Utils;

namespace PluginAdmin.Services;

public class PluginAdminService
{
    private static PluginAdminService? _instance;

    public SettingsSection Settings { get; } = AAppService.Instance.GetSettings("pluginAdmin");

    public static PluginAdminService Instance
    {
        get
        {
            _instance ??= new PluginAdminService();
            return _instance;
        }
    }

    public static void Init()
    {
        _instance ??= new PluginAdminService();
    }

    public PluginAdminService()
    {
        PluginsHttpService.Instance.Auth = Settings.Get<AuthModel>("user");
    }
}