using Git.Services;
using Git.UI;
using Serilog;
using TestGenerator.Shared.Settings;
using TestGenerator.Shared.Types;
using TestGenerator.Shared.Utils;

namespace Git;

public class Git : TestGenerator.Shared.Plugin
{
    public static SettingsSection Settings { get; } = AAppService.Instance.GetSettings();
    public static SettingsSection ProjectSettings => AAppService.Instance.CurrentProject.GetSettings();
    public static SettingsSection ProjectData => AAppService.Instance.CurrentProject.GetData();
    public static Logger Logger { get; } = AAppService.Instance.GetLogger();
    
    public Git()
    {
        SideTabs = [new GitTab()];
        
        SettingsControls =
        [
            new SettingsPage("Git", Settings.Name ?? "", [
                new ProgramField { Program = GitService.GitProgram, FieldName = "Git", Key = "git" }
            ]),
        ];
    }
}