using TestGenerator.Shared.SidePrograms;
using TestGenerator.Shared.Types;
using TestGenerator.Shared.Utils;

namespace LangPython;

public class PythonBuilder : BaseBuilder
{
    private SideProgramFile? Python => LangPython.Python.FromModel(
        Settings.Get<bool>("defaultInterpreter")
            ? LangPython.ProjectSettings.Get<bool>("defaultInterpreter")
                ? LangPython.Settings.Get<ProgramFileModel>("interpreter")
                : LangPython.ProjectSettings.Get<ProgramFileModel>("interpreter")
            : Settings.Get<ProgramFileModel>("interpreter"));

    private string MainFile => Settings.Get<string>("mainFile") ?? "";

    public PythonBuilder(Guid id, AProject project, SettingsSection settings) : base(id, project, settings)
    {
    }

    public override async Task<int> Run(string args = "", string? workingDirectory = null)
    {
        var python = Python;
        if (python == null)
            return -1;
        var res = await python.Execute($"\"{MainFile}\" {args}");
        return res.ExitCode;
    }

    public override async Task<int> RunConsole(string args, string? workingDirectory = null)
    {
        var python = Python;
        Console.WriteLine($"{Settings.Get<ProgramFileModel>("interpreter")?.ToString() ?? "null"} " +
                          $"{LangPython.ProjectSettings.Get<ProgramFileModel>("interpreter")?.ToString() ?? "null"} " +
                          $"{LangPython.Settings.Get<ProgramFileModel>("interpreter")?.ToString() ?? "null"} ");
        Console.WriteLine($"Python: {python?.Path ?? "null"}");
        if (python == null)
            return -1;
        var terminalController = python.ExecuteInConsole($"{MainFile} {args}", workingDirectory);
        return await terminalController.RunAsync();
    }
}