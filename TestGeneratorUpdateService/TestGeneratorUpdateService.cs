using Avalonia.Controls;

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

        SettingsControls?.Add("Обновления", new UpdatePage());
    }
}