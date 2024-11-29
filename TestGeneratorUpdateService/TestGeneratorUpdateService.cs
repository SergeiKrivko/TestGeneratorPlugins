using Avalonia.Controls;
using TestGenerator.Shared.Settings;

namespace TestGeneratorUpdateService;

public class TestGeneratorUpdateService : TestGenerator.Shared.Plugin
{
    public TestGeneratorUpdateService()
    {
        Name = "NewPlugin";

        MainTabs = [];
        SideTabs = [];

        BuildTypes = [];
        ProjectTypes = [];

        SettingsControls?.Add(new SettingsNode("Обновления", new UpdatePage()));
    }
}