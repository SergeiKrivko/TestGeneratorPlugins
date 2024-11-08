using TestPlugin.UI;

namespace TestPlugin;

public class TestPlugin : TestGenerator.Shared.Plugin
{
    public TestPlugin()
    {
        Name = "NewPlugin";

        MainTabs = [new TestMainTab()];
        SideTabs = [];

        BuildTypes = [];
        ProjectTypes = [];
    }
}