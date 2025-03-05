using AvaluxUI.Utils;
using TestGenerator.Shared.Settings.Shared;
using TestGenerator.Shared.SidePrograms;
using TestGenerator.Shared.Types;

namespace LangC;

public class CBuilder : BaseBuilder
{
    private readonly IProjectsService _projectsService = Injector.Inject<IProjectsService>();
    
    public CBuilder(Guid id, IProject project, ISettingsSection settings) : base(id, project, settings)
    {
    }

    private string ExePath
    {
        get
        {
            var path = Settings.Get<string>("exePath", "app.exe");
            if (Path.IsPathRooted(path))
                return path;
            return Path.Join(_projectsService.Current.Path, path);
        }
    }

    public override async Task<int> Compile(CancellationToken token = new())
    {
        LangC.Logger.Info("Compiling");

        var gcc = Programs.GetGcc();
        if (gcc == null)
            return -1;

        var cFiles = new List<string>();
        var hFiles = new List<string>();
        var oFiles = new List<string>();

        Directory.CreateDirectory(TempPath);

        foreach (var file in Settings.Get<string[]>("files", []))
        {
            if (Path.GetExtension(file) == ".c")
                cFiles.Add(file);
            else
            {
                hFiles.Add(file);
            }
        }

        foreach (var file in cFiles)
        {
            var objFile = await
                gcc.VirtualSystem.ConvertPath(Path.Join(TempPath, Path.ChangeExtension(Path.GetFileName(file), ".o")));
            oFiles.Add(objFile);
            var res = await gcc.Execute(new RunProgramArgs
            {
                Args = $"\"{await gcc.VirtualSystem.ConvertPath(Path.Join(Project.Path, file))}\" " +
                       $"-c {Settings.Get<string>("compilerKeys")} " +
                       $"-o \"{objFile}\""
            }, token);
            if (res.ExitCode != 0)
                return res.ExitCode;
        }

        return (await gcc.Execute(
            new RunProgramArgs
            {
                Args = $"{string.Join(' ', oFiles.Select(f => "\"" + f + "\""))} " +
                       $"-o \"{await gcc.VirtualSystem.ConvertPath(ExePath)}\""
            }, token)).ExitCode;
    }

    public override async Task<ICompletedProcess> Run(string args = "", string? workingDirectory = null,
        string? stdin = null, EnvironmentModel? environment = null, CancellationToken token = new())
    {
        var gcc = Programs.GetGcc();
        if (gcc == null)
            return await base.Run(args, workingDirectory, stdin, environment, token);
        return await gcc.VirtualSystem.Execute(new RunProcessArgs
        {
            Filename = await gcc.VirtualSystem.ConvertPath(ExePath),
            Args = args,
            WorkingDirectory = workingDirectory,
            Stdin = stdin
        }, token);
    }

    public override async Task<ICompletedProcess> RunConsole(string args = "", string? workingDirectory = null,
        string? stdin = null, EnvironmentModel? environment = null, CancellationToken token = new())
    {
        var gcc = Programs.GetGcc();
        if (gcc == null)
            return await base.RunConsole(args, workingDirectory, stdin, environment, token);
        return await gcc.VirtualSystem.Execute(RunProcessArgs.ProcessRunProvider.RunTab, new RunProcessArgs
        {
            Filename = await gcc.VirtualSystem.ConvertPath(ExePath),
            Args = args,
            WorkingDirectory = workingDirectory,
            Stdin = stdin
        }, token);
    }

    public override string Command => Path.Join(Project.Path, ExePath);
}