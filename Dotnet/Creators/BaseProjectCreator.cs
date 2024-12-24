using Avalonia.Controls;
using TestGenerator.Shared.Settings;
using TestGenerator.Shared.SidePrograms;
using TestGenerator.Shared.Types;
using TestGenerator.Shared.Utils;

namespace Dotnet.Creators;

public abstract class BaseProjectCreator : IProjectCreator
{
    public abstract string Key { get; }
    public abstract string Name { get; }
    public virtual string Icon => LangCSharp.CSharpIcon;

    public Control GetControl()
    {
        var control = new SettingsControl();
        control.Add(new PathField { FieldName = "Папка решения", Key = "path", Directory = true });
        control.Add(new StringField { FieldName = "Название решения", Key = "solutionName", });
        control.Add(new StringField { FieldName = "Название проекта", Key = "projectName", });
        control.Section = SettingsSection.Empty("projectCreator");
        return control;
    }

    public string GetPath(Control control)
    {
        if (control is not SettingsControl settingsControl)
            throw new Exception();
        return Path.Join(settingsControl.Section?.Get<string>("path"),
            settingsControl.Section?.Get<string>("solutionName"));
    }

    protected abstract string TemplateName { get; }
    protected virtual bool CreateBuild => true;

    public async Task Initialize(AProject project, Control control)
    {
        var settings = (control as SettingsControl)?.Section;
        if (settings == null)
            return;
        var dotnet = LangCSharp.Dotnet.FromModel(LangCSharp.Settings.Get<ProgramFileModel>("dotnet")) ??
                     throw new Exception("dotnet not found");

        var projectName = settings.Get("projectName", TemplateName + "App");

        await dotnet.Execute(new RunProgramArgs
            { Args = $"new solution --name {settings.Get<string>("solutionName")}", WorkingDirectory = project.Path });
        await dotnet.Execute(new RunProgramArgs
            { Args = $"new {TemplateName} --name {projectName}", WorkingDirectory = project.Path });
        await dotnet.Execute(new RunProgramArgs
            { Args = $"sln add {projectName}", WorkingDirectory = project.Path });

        if (CreateBuild)
        {
            var build = await AAppService.Instance.Request<ABuild>("createBuild", "Dotnet");
            build.Name = projectName;
            build.Builder.Settings.Set("project", projectName);
            build.Builder.Settings.Set("configuration", "Debug");
            project.Settings.Set("selectedBuild", build.Id);
        }

        project.Settings.Set("defaultPrograms", true);
    }
}