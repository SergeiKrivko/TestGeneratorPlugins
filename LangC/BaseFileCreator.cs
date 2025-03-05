using AvaluxUI.Utils;
using TestGenerator.Shared.Settings;
using TestGenerator.Shared.Types;

namespace LangC;

public abstract class BaseFileCreator : IFileCreator
{
    private readonly IProjectsService _projectsService = Injector.Inject<IProjectsService>();

    public virtual string Key => "CreateC";
    public virtual string Name => "C source file";
    public virtual int Priority => 5;
    public virtual string? Icon => null;

    protected abstract string Extension { get; }

    public bool Enabled => _projectsService.Current.Type.Key == "C";

    public SettingsControl GetSettingsControl()
    {
        var settingsControl = new SettingsControl();
        settingsControl.Add(new StringField { Key = "Name", FieldName = "Имя файла" });
        return settingsControl;
    }

    public void Create(string root, ISettingsSection options)
    {
        var filename = options.Get<string>("Name");
        if (filename != null && !filename.EndsWith(Extension))
            filename += Extension;
        if (!string.IsNullOrWhiteSpace(filename))
            File.Create(Path.Join(root, filename.Trim())).Close();
    }
}