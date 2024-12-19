using TestGenerator.Shared.Settings;
using TestGenerator.Shared.Types;
using TestGenerator.Shared.Utils;

namespace UpdateService;

public class UpdateService : TestGenerator.Shared.Plugin
{
    public static Logger Logger { get; } = AAppService.Instance.GetLogger();
    public static SettingsSection Settings { get; } = AAppService.Instance.GetSettings();
    
    public UpdateService()
    {
        SettingsControls = [new SettingsNode("Обновления", new UpdatePage())];
    }
}