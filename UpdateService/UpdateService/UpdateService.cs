using AvaluxUI.Utils;
using TestGenerator.Shared.Settings;
using TestGenerator.Shared.Types;
using TestGenerator.Shared.Utils;

namespace UpdateService;

public class UpdateService : TestGenerator.Shared.Plugin
{
    public static Logger Logger { get; } = Injector.Inject<IAppService>().GetLogger();
    public static ISettingsSection Settings { get; } = Injector.Inject<IAppService>().GetSettings();

    public UpdateService()
    {
        SettingsControls = [new SettingsNode("Обновления", new UpdatePage())];
    }
}