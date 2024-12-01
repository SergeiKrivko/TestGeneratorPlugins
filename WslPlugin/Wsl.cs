using TestGenerator.Shared.SidePrograms;
using TestGenerator.Shared.Types;

namespace WslPlugin;

public class Wsl : IVirtualSystem
{
    public string Key => "WSL";
    public string Name => "WSL";

    public ICollection<string> Tags => ["WSL", "Linux", "Default"];

    public async Task<string> ConvertPath(string path)
    {
        if (path[1] == ':')
            return $"/mnt/{path[0].ToString().ToLower()}{path.Replace('\\', '/').AsSpan(2)}";
        return path;
    }

    public async Task<ICompletedProcess> Execute(RunProcessArgs.ProcessRunProvider where, RunProcessArgs args)
    {
        return await AAppService.Instance.RunProcess(where, GetArgs(args));
    }

    public async Task<ICompletedProcess> Execute(RunProcessArgs args)
    {
        return await AAppService.Instance.RunProcess(GetArgs(args));
    }

    public async Task<ICollection<ICompletedProcess>> Execute(RunProcessArgs.ProcessRunProvider where, params RunProcessArgs[] args)
    {
        return await AAppService.Instance.RunProcess(where, args.Select(GetArgs).ToArray());
    }

    public async Task<ICollection<ICompletedProcess>> Execute(params RunProcessArgs[] args)
    {
        return await AAppService.Instance.RunProcess(args.Select(GetArgs).ToArray());
    }

    private RunProcessArgs GetArgs(RunProcessArgs args) => new RunProcessArgs
    {
        Filename = "wsl",
        Args = $"-e {args.Filename} {args.Args}",
        WorkingDirectory = args.WorkingDirectory,
        Stdin = args.Stdin
    };
}