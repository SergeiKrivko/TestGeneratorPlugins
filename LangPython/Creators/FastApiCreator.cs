using System.Reflection;
using TestGenerator.Shared.SidePrograms;
using TestGenerator.Shared.Types;
using TestGenerator.Shared.Utils;

namespace LangPython.Creators;

public class FastApiCreator : PythonProjectCreator
{
    public override string Key => "FastApi";
    public override string Name => "FastApi";
    public override string Icon => LangPython.FastApiIcon;

    protected override async Task CreateFiles(AProject project, SettingsSection settings, IBackgroundTask task, CancellationToken token)
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

    protected override async Task InstallDependencies(SideProgramFile python, IBackgroundTask task, CancellationToken token)
    {
        task.Status = "Установка FastApi";
        await python.Execute(new RunProgramArgs{Args = "-m pip install fastapi"}, token);
        task.Progress = 40;
        
        task.Status = "Установка uvicorn";
        await python.Execute(new RunProgramArgs{Args = "-m pip install uvicorn"}, token);
    }

    protected override async Task<ABuild> CreateBuild(AProject project, SettingsSection settings, IBackgroundTask task, CancellationToken token)
    {
        task.Status = "Создание конфигурации запуска";

        var build = await AAppService.Instance.Request<ABuild>("createBuild", "FastApi", token);
        build.Name = "server";
        build.Builder.Settings.Set("entrypoint", "main:app");
        build.Builder.Settings.Set("defaultInterpreter", true);
        return build;
    }
}