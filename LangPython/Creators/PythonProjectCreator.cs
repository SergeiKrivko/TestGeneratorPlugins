using Avalonia.Controls;
using AvaluxUI.Utils;
using TestGenerator.Shared.Settings;
using TestGenerator.Shared.SidePrograms;
using TestGenerator.Shared.Types;

namespace LangPython.Creators;

public class PythonProjectCreator : IProjectCreator
{
    private readonly IAppService _appService = Injector.Inject<IAppService>();

    public virtual string Key => "Python";
    public virtual string Name => "Чистый Python";
    public virtual string Icon => LangPython.PythonIcon;

    public Control GetControl()
    {
        var control = CreateSettingsControl();

        var settings = SettingsSection.Empty("projectCreator");
        ApplyDefaultSettings(settings);
        control.Section = settings;
        return control;
    }

    protected virtual void ApplyDefaultSettings(ISettingsSection settings)
    {
        settings.Set("createVenv", true);
        settings.Set("venvPath", "venv");
    }

    protected virtual SettingsControl CreateSettingsControl()
    {
        var control = new SettingsControl();
        control.Add(new PathField { FieldName = "Папка проекта", Key = "path", Directory = true });
        control.Add(new StringField { FieldName = "Название проекта", Key = "name", });
        control.Add(new DefaultField([
            new StringField { FieldName = "Папка venv", Key = "venvPath" },
            new ProgramField
                { FieldName = "Базовый интерпретатор", Key = "baseInterpreter", Program = LangPython.Python },
            new DefaultField { FieldName = "Наследовать библиотеки", Key = "inheritLibs" }
        ]) { FieldName = "Создать виртуальное окружение (venv)", Key = "createVenv" });
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
        var python = await CreateVenv(project, settings, task, token);
        task.Progress = 40;
        await InstallDependencies(python, task, token);
        task.Progress = 85;
        await CreateFiles(project, settings, task, token);
        task.Progress = 90;
        var build = await CreateBuild(project, settings, task, token);
        task.Progress = 100;

        project.Settings.GetSection("LangPython").Set("defaultPrograms", settings.Get("createVenv", false));
        project.Settings.GetSection("LangPython").Set("interpreter", python.ToModel());
        project.Settings.Set("selectedBuild", build.Id);
    }

    protected virtual async Task<SideProgramFile> CreateVenv(IProject project, ISettingsSection settings,
        IBackgroundTask task, CancellationToken token)
    {
        task.Status = "Создание виртуального окружения";

        var python = LangPython.Python.FromModel(settings.Get<ProgramFileModel>("baseInterpreter")) ??
                     throw new Exception("Base interpreter not found");
        if (!settings.Get("createVenv", false))
            return python;
        var venvPath = settings.Get<string>("venvPath");
        var args = $"-m venv {venvPath}";
        if (settings.Get("inheritLibs", false))
            args += " --system-site-packages";
        await python.Execute(new RunProgramArgs
            { Args = args, WorkingDirectory = project.Path }, token);
        venvPath = Path.IsPathRooted(venvPath) ? venvPath : Path.Join(project.Path, venvPath);
        var venvPythonPath = Path.Join(venvPath, OperatingSystem.IsWindows() ? "Scripts\\python.exe" : "bin/python3");

        return LangPython.Python.FromPath(venvPythonPath, python.VirtualSystem);
    }

    protected virtual Task InstallDependencies(SideProgramFile python, IBackgroundTask task,
        CancellationToken token)
    {
        task.Status = "Установка зависимостей";
        return Task.CompletedTask;
    }

    protected virtual async Task CreateFiles(IProject project, ISettingsSection settings, IBackgroundTask task,
        CancellationToken token)
    {
        task.Status = "Создание файлов проекта";

        await File.WriteAllTextAsync(Path.Join(project.Path, "main.py"), "print('Hello world!')\n", token);
    }

    protected virtual async Task<IBuild> CreateBuild(IProject project, ISettingsSection settings, IBackgroundTask task,
        CancellationToken token)
    {
        task.Status = "Создание конфигурации запуска";

        var build = await _appService.Request<IBuild>("createBuild", "Python", token);
        build.Name = "main";
        build.Builder.Settings.Set("mainFile", "main.py");
        build.Builder.Settings.Set("defaultInterpreter", true);
        return build;
    }
}