using Avalonia.Controls;
using TestGenerator.Shared.Settings;
using TestGenerator.Shared.Types;
using TestGenerator.Shared.Utils;

namespace TestGeneratorUpdateService;

public class TestGeneratorUpdateService : TestGenerator.Shared.Plugin
{
    public static Logger Logger { get; } = AAppService.Instance.GetLogger();
    
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