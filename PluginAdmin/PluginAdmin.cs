using PluginAdmin.UI;

namespace PluginAdmin;

public class PluginAdmin : TestGenerator.Shared.Plugin
{
    public PluginAdmin()
    {
        Name = "NewPlugin";

        MainTabs = [new PluginAdminTab()];
        SideTabs = [];

        BuildTypes = [];
        ProjectTypes = [];
    }
}