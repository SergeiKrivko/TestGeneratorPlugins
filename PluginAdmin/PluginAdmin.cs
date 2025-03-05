using AvaluxUI.Utils;
using PluginAdmin.Services;
using PluginAdmin.UI;

namespace PluginAdmin;

public class PluginAdmin : TestGenerator.Shared.Plugin
{
    public PluginAdmin()
    {
        Injector.AddService<PluginsHttpService>();
        Injector.AddService<PluginAdminService>();

        MainTabs = [new PluginAdminTab()];
    }
}