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

    public async override Task<int> Compile()
    {
        AAppService.Instance.GetLogger("C").Info("Compiling");

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
            var objFile = Path.Join(TempPath, Path.ChangeExtension(Path.GetFileName(file), ".o"));
            oFiles.Add(objFile);
            var res = await AAppService.Instance.RunProcess($"gcc {Path.Join(Project.Path, file)} " +
                                                            $"-c {Settings.Get<string>("compilerKeys")} -o {objFile}");
            if (res.ExitCode != 0)
                return res.ExitCode;
        }

        return (await AAppService.Instance.RunProcess($"gcc {string.Join(' ', oFiles)} " +
                                                      $"-o {Path.Join(Project.Path, "app.exe")}")).ExitCode;
    }

    public override string Command => Path.Join(Project.Path, "app.exe");
}