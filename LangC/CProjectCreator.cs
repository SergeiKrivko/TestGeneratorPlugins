using Avalonia.Controls;
using AvaluxUI.Utils;
using TestGenerator.Shared.Settings;
using TestGenerator.Shared.Types;

namespace LangC;

public class CProjectCreator : IProjectCreator
{
    private readonly IAppService _appService = Injector.Inject<IAppService>();

    public string Key => "CExe";
    public string Name => "C executable";
    public string Icon => LangC.CIcon;

    public Control GetControl()
    {
        var control = new SettingsControl();
        control.Add(new PathField { FieldName = "Папка проекта", Key = "path", Directory = true });
        control.Add(new StringField { FieldName = "Название проекта", Key = "name", });
        control.Section = SettingsSection.Empty("projectCreator");
        return control;
    }

    public string GetPath(Control control)
    {
        if (control is not SettingsControl settingsControl)
            throw new Exception();
        return Path.Join(settingsControl.Section?.Get<string>("path"), settingsControl.Section?.Get<string>("name"));
    }

    public async Task Initialize(IProject project, Control control, IBackgroundTask task, CancellationToken token)
    {
        task.Progress = 0;
        var settings = (control as SettingsControl)?.Section;
        if (settings == null)
            return;

        task.Progress = 10;
        task.Status = "Создание main.c";
        await File.WriteAllTextAsync(Path.Join(project.Path, "main.c"), "#include <stdio.h>\n\n" +
                                                                        "int main(void)\n" +
                                                                        "{\n" +
                                                                        "    printf(\"Hello world!\\n\");\n" +
                                                                        "    return 0;\n" +
                                                                        "}\n", token);
        task.Progress = 50;

        task.Status = "Создание конфигурации сборки";
        var build = await _appService.Request<IBuild>("createBuild", "CExe", token);
        build.Name = "Debug";
        build.Builder.Settings.Set("exePath", "app.exe");
        build.Builder.Settings.Set("files", new[] { "main.c" });

        task.Progress = 100;

        project.Settings.GetSection("LangC").Set("defaultPrograms", true);
        project.Settings.Set("selectedBuild", build.Id);
    }
}