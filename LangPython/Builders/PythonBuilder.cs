﻿using TestGenerator.Shared.SidePrograms;
using TestGenerator.Shared.Types;
using TestGenerator.Shared.Utils;

namespace LangPython.Builders;

public class PythonBuilder : BaseBuilder
{
    private SideProgramFile? Python => LangPython.Python.FromModel(
        Settings.Get<bool>("defaultInterpreter", true)
            ? LangPython.ProjectSettings.Get<bool>("defaultInterpreter", true)
                ? LangPython.Settings.Get<ProgramFileModel>("interpreter")
                : LangPython.ProjectSettings.Get<ProgramFileModel>("interpreter")
            : Settings.Get<ProgramFileModel>("interpreter"));

    private string MainFile => Settings.Get<string>("mainFile") ?? "";

    public PythonBuilder(Guid id, AProject project, SettingsSection settings) : base(id, project, settings)
    {
    }

    public override async Task<ICompletedProcess> Run(string args = "", string? workingDirectory = null,
        string? stdin = null, CancellationToken token = new())
    {
        var python = Python;
        if (python == null)
            throw new Exception("Python interpreter not found");
        return await python.Execute(new RunProgramArgs
        {
            Args = $"\"{await python.VirtualSystem.ConvertPath(MainFile)}\" {args}", WorkingDirectory = workingDirectory
        }, token);
    }

    public override async Task<ICompletedProcess> RunConsole(string args = "", string? workingDirectory = null,
        string? stdin = null, CancellationToken token = new())
    {
        var python = Python;
        if (python == null)
            throw new Exception("Python interpreter not found");
        return await python.Execute(RunProcessArgs.ProcessRunProvider.RunTab, new RunProgramArgs
        {
            Args = $"\"{await python.VirtualSystem.ConvertPath(MainFile)}\" {args}", WorkingDirectory = workingDirectory
        }, token);
    }
}