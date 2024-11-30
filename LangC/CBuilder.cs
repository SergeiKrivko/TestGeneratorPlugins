using System.Diagnostics;
using TestGenerator.Shared;
using TestGenerator.Shared.Types;
using TestGenerator.Shared.Utils;

namespace LangC;

public class CBuilder : BaseBuilder
{
    public CBuilder(Guid id, AProject project, SettingsSection settings) : base(id, project, settings)
    {
    }

    public string ExePath
    {
        get
        {
            var path = Settings.Get<string>("exePath", "app.exe");
            if (Path.IsPathRooted(path))
                return path;
            return Path.Join(AAppService.Instance.CurrentProject.Path, path);
        }
    }

    public override async Task<int> Compile()
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
            var res = await gcc.Execute($"\"{await gcc.VirtualSystem.ConvertPath(Path.Join(Project.Path, file))}\" " +
                                        $"-c {Settings.Get<string>("compilerKeys")} " +
                                        $"-o \"{objFile}\"");
            if (res.ExitCode != 0)
                return res.ExitCode;
        }

        return (await gcc.Execute(
            $"{string.Join(' ', oFiles.Select(f => "\"" + f + "\""))} " +
            $"-o \"{await gcc.VirtualSystem.ConvertPath(ExePath)}\"")).ExitCode;
    }

    public override async Task<int> Run(string args = "", string? workingDirectory = null)
    {
        var gcc = Programs.GetGcc();
        if (gcc == null)
            return await base.Run(args, workingDirectory);
        return (await gcc.VirtualSystem.Execute(await gcc.VirtualSystem.ConvertPath(ExePath), args)).ExitCode;
    }

    public override async Task<int> RunConsole(string args, string? workingDirectory = null)
    {
        var gcc = Programs.GetGcc();
        if (gcc == null)
            return await base.RunConsole(args, workingDirectory);
        return await gcc.VirtualSystem
            .ExecuteInConsole($"{await gcc.VirtualSystem.ConvertPath(ExePath)} {args}", workingDirectory).RunAsync();
    }

    public override string Command => Path.Join(Project.Path, ExePath);
}