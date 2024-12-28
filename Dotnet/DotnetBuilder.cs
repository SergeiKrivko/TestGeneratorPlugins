using TestGenerator.Shared.Settings.Shared;
using TestGenerator.Shared.SidePrograms;
using TestGenerator.Shared.Types;
using TestGenerator.Shared.Utils;

namespace Dotnet;

public class DotnetBuilder : BaseBuilder
{
    public DotnetBuilder(Guid id, AProject project, SettingsSection settings) : base(id, project, settings)
    {
    }

    public override async Task<int> Compile(CancellationToken token = new())
    {
        LangCSharp.Logger.Info("Compiling");

        var dotnet = LangCSharp.GetDotnet();
        if (dotnet == null)
            return -1;

        var command = $"build {Path.Join(Project.Path, Settings.Get<string>("project"))}";
        if (!string.IsNullOrEmpty(Settings.Get<string>("configuration")))
            command += $" -c {Settings.Get<string>("configuration")}";

        var res = await dotnet.Execute(new RunProgramArgs { Args = command, WorkingDirectory = Project.Path }, token);
        return res.ExitCode;
    }

    public override async Task<ICompletedProcess> Run(string args = "", string? workingDirectory = null,
        string? stdin = null, EnvironmentModel? environment = null, CancellationToken token = new())
    {
        var dotnet = LangCSharp.GetDotnet();
        if (dotnet == null)
            throw new Exception(".Net not found");

        var command = $"run {args} --project {Path.Join(Project.Path, Settings.Get<string>("project"))} --no-build";
        if (!string.IsNullOrEmpty(Settings.Get<string>("configuration")))
            command += $" --configuration {Settings.Get<string>("configuration")}";

        return await dotnet.Execute(new RunProgramArgs
            { Args = command, WorkingDirectory = workingDirectory, Stdin = stdin, Environment = environment }, token);
    }

    public override async Task<ICompletedProcess> RunConsole(string args = "", string? workingDirectory = null,
        string? stdin = null, EnvironmentModel? environment = null, CancellationToken token = new())
    {
        var dotnet = LangCSharp.GetDotnet();
        if (dotnet == null)
            throw new Exception(".Net not found");

        var command = $"run {args} --project {Path.Join(Project.Path, Settings.Get<string>("project"))} --no-build";
        if (!string.IsNullOrEmpty(Settings.Get<string>("configuration")))
            command += $" --configuration {Settings.Get<string>("configuration")}";

        return await dotnet.Execute(RunProcessArgs.ProcessRunProvider.RunTab, new RunProgramArgs
            { Args = command, WorkingDirectory = workingDirectory, Environment = environment, Stdin = stdin }, token);
    }
}