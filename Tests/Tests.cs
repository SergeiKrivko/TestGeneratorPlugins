using TestGenerator.Shared.Types;
using TestGenerator.Shared.Utils;
using Tests.Core;
using Tests.Ui;

namespace Tests;

public class Tests : TestGenerator.Shared.Plugin
{
    public static SettingsSection Settings { get; } = AAppService.Instance.GetSettings();
    public static SettingsSection ProjectSettings => AAppService.Instance.CurrentProject.GetSettings();
    public static SettingsSection ProjectData => AAppService.Instance.CurrentProject.GetData();

    public static TestsService Service { get; } = new();
    
    public Tests()
    {
        MainTabs = [new TestsTab()];
        SideTabs = [];

        BuildTypes = [];
        ProjectTypes = [];
    }
}