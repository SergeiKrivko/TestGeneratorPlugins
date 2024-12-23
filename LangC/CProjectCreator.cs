using Avalonia.Controls;
using TestGenerator.Shared.Settings;
using TestGenerator.Shared.Types;
using TestGenerator.Shared.Utils;

namespace LangC;

public class CProjectCreator : IProjectCreator
{
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
        return (control as SettingsControl)?.Section?.Get<string>("path") ?? throw new Exception();
    }

    public async Task Initialize(AProject project, Control control)
    {
        var settings = (control as SettingsControl)?.Section;
        if (settings == null)
            return;

        var name = settings.Get<string>("name");
        if (!string.IsNullOrEmpty(name))
            project.Name = name;

        await File.WriteAllTextAsync(Path.Join(project.Path, "main.c"), "#include <stdio.h>\n\n" +
                                                                        "int main(void)\n" +
                                                                        "{\n" +
                                                                        "    printf(\"Hello world!\\n\");\n" +
                                                                        "    return 0;\n" +
                                                                        "}\n");
        var build = await AAppService.Instance.Request<ABuild>("createBuild", "CExe");
        build.Name = "Debug";
        build.Builder.Settings.Set("exePath", "app.exe");
        build.Builder.Settings.Set("files", new [] { "main.c" });
        
        project.Settings.GetSection("LangC").Set("defaultPrograms", true);
        project.Settings.Set("selectedBuild", build.Id);
    }
}