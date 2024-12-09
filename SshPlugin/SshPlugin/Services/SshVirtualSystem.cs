using System.Text.Json;
using SshPlugin.Models;
using TestGenerator.Shared.SidePrograms;
using TestGenerator.Shared.Types;

namespace SshPlugin.Services;

internal class SshVirtualSystem : IVirtualSystem
{
    public required string Name { get; init; }
    public required string Key { get; init; }
    public required ICollection<string> Tags { get; init; }
    public bool IsActive { get; set; } = true;

    internal required FilesService FilesService { get; init; }
    internal required SshBridgeService BridgeService { get; init; }


    public async Task<ICompletedProcess> Execute(RunProcessArgs.ProcessRunProvider where, RunProcessArgs args)
    {
        return (await Exec([args]))[0];
    }

    public async Task<ICollection<ICompletedProcess>> Execute(RunProcessArgs.ProcessRunProvider where,
        params RunProcessArgs[] args)
    {
        return await Exec(args);
    }

    public async Task<ICompletedProcess> Execute(RunProcessArgs args)
    {
        return (await Exec([args]))[0];
    }

    public async Task<ICollection<ICompletedProcess>> Execute(params RunProcessArgs[] args)
    {
        return await Exec(args);
    }

    private async Task<ICompletedProcess[]> Exec(params RunProcessArgs[] args)
    {
        var (parsedArgs, files) = ParseArgs(args);
        try
        {
            SshPlugin.Logger.Debug("Starting processes " +
                                   string.Join("; ", args.Select(a => $"'{a.Filename} {a.Args}'")));
            await FilesService.PushFiles(files);
            var res = await BridgeService.RunProcesses(parsedArgs);
            await FilesService.PullFiles(files);
            foreach (var valueTuple in args.Zip(res))
            {
                SshPlugin.Logger.Debug(
                    $"Process '{valueTuple.First.Filename} {valueTuple.First.Args}' (exit {valueTuple.Second.ExitCode})");
            }

            return res.Select(r => r as ICompletedProcess).ToArray();
        }
        catch (Exception)
        {
            // throw;
            var res = new ICompletedProcess[args.Length];
            for (int i = 0; i < args.Length; i++)
            {
                SshPlugin.Logger.Debug(
                    $"Process '{args[i].Filename} {args[i].Args}' failed");
                res[i] = new CompletedProcess { ExitCode = 403 };
            }

            return res;
        }
    }

    public async Task<string> ConvertPath(string path)
    {
        return $"#ssh-path{{{path}}}";
    }

    private string ParseCommand(string command, List<string> files)
    {
        var newCommand = command;
        var index = 0;
        do
        {
            index = command.IndexOf("#ssh-path{", index, StringComparison.InvariantCulture);
            if (index < 0) break;
            index += "#ssh-path{".Length;
            var endIndex = command.IndexOf('}', index);
            var localPath = command.Substring(index, endIndex - index);
            files.Add(localPath);
            var hostPath =
                $"#ssh-origin-path{{{JsonSerializer.Serialize(FilesService.GetOrigin(localPath))};" +
                $"{FilesService.GetOriginPath(localPath)}}}";
            newCommand = newCommand.Replace($"#ssh-path{{{localPath}}}", hostPath);
            index++;
        } while (index < command.Length);

        return newCommand;
    }

    private RunProcessArgs ParseArgs(RunProcessArgs args, List<string> files)
    {
        if (args.WorkingDirectory != null)
        {
            files.Add(args.WorkingDirectory);
        }

        return new RunProcessArgs
        {
            Args = ParseCommand(args.Args, files),
            Filename = args.Filename,
            Stdin = args.Stdin,
            WorkingDirectory = args.WorkingDirectory == null
                ? null
                : $"#ssh-origin-path{{{JsonSerializer.Serialize(FilesService.GetOrigin(args.WorkingDirectory))};" +
                  $"{FilesService.GetOriginPath(args.WorkingDirectory)}}}"
        };
    }

    private (RunProcessArgs[], List<string>) ParseArgs(RunProcessArgs[] argsArray)
    {
        var files = new List<string>([AAppService.Instance.CurrentProject.Path]);
        var res = argsArray.Select(p => ParseArgs(p, files)).ToArray();
        return (res, files);
    }
}