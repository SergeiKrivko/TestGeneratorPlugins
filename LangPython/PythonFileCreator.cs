using TestGenerator.Shared.Settings;
using TestGenerator.Shared.Types;
using TestGenerator.Shared.Utils;

namespace LangPython;

public class PythonFileCreator : IFileCreator
{
    public virtual string Key => "CreatePython";
    public virtual string Name => "Python source file";
    public virtual int Priority => AAppService.Instance.CurrentProject.Type.Key == "Python" ? 6 : 3;
    public virtual string? Icon => LangPython.PythonIcon;

    public SettingsControl GetSettingsControl()
    {
        var settingsControl = new SettingsControl();
        settingsControl.Add(new StringField{Key = "Name", FieldName = "Имя файла"});
        return settingsControl;
    }

    public void Create(string root, SettingsSection options)
    {
        var filename = options.Get<string>("Name");
        if (filename != null && !filename.EndsWith(".py"))
            filename += ".py";
        if (!string.IsNullOrWhiteSpace(filename))
            File.Create(Path.Join(root, filename.Trim())).Close();
    }
}