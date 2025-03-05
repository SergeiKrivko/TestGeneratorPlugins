using AvaluxUI.Utils;
using TestGenerator.Shared.Settings;
using TestGenerator.Shared.Types;

namespace Dotnet;

public class CSharpFileCreator : IFileCreator
{
    private readonly IProjectsService _projectsService = Injector.Inject<IProjectsService>();

    public virtual string Key => "CreateCSharp";
    public virtual string Name => "Class / Interface";
    public virtual int Priority => 5;
    public virtual string Icon => LangCSharp.CSharpIcon;
    public bool Enabled => _projectsService.Current.Type.Key == "CSharp";

    public SettingsControl GetSettingsControl()
    {
        var settingsControl = new SettingsControl();
        settingsControl.Add(new StringField { Key = "Name", FieldName = "Название" });
        return settingsControl;
    }

    public void Create(string root, ISettingsSection options)
    {
        var filename = options.Get<string>("Name");
        if (filename != null && !filename.EndsWith(".cs"))
            filename += ".cs";
        if (!string.IsNullOrWhiteSpace(filename))
            File.Create(Path.Join(root, filename.Trim())).Close();
    }
}