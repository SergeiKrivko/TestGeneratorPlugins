using System.Reflection;
using AvaluxUI.Utils;
using TestGenerator.Shared.SidePrograms;
using TestGenerator.Shared.Types;

namespace LangPython.Creators;

public class FastApiCreator : PythonProjectCreator
{
    private readonly IAppService _appService = Injector.Inject<IAppService>();

    public override string Key => "FastApi";
    public override string Name => "FastApi";
    public override string Icon => LangPython.FastApiIcon;

    protected override async Task CreateFiles(IProject project, ISettingsSection settings, IBackgroundTask task,
        CancellationToken token)
    {
        task.Status = "Создание файлов проекта";

        await Task.Run(() =>
        {
            foreach (var file in Directory.EnumerateFiles(
                         Path.Join(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Assets",
                             "FastApiTemplate")))
            {
                File.Copy(file, Path.Join(project.Path, Path.GetFileName(file)));
            }
        }, token);
    }

    protected override async Task InstallDependencies(SideProgramFile python, IBackgroundTask task,
        CancellationToken token)
    {
        task.Status = "Установка FastApi";
        await python.Execute(new RunProgramArgs { Args = "-m pip install fastapi" }, token);
        task.Progress = 65;

        task.Status = "Установка uvicorn";
        await python.Execute(new RunProgramArgs { Args = "-m pip install uvicorn" }, token);
    }

    protected override async Task<IBuild> CreateBuild(IProject project, ISettingsSection settings, IBackgroundTask task,
        CancellationToken token)
    {
        task.Status = "Создание конфигурации запуска";

        var build = await _appService.Request<IBuild>("createBuild", "FastApi", token);
        build.Name = "server";
        build.Builder.Settings.Set("entrypoint", "main:app");
        build.Builder.Settings.Set("defaultInterpreter", true);
        return build;
    }
}