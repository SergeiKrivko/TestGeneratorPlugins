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

    public async Task<ICompletedProcess> Execute(RunProcessArgs.ProcessRunProvider where, RunProcessArgs args, CancellationToken token = new())
    {
        return await AAppService.Instance.RunProcess(where, GetArgs(args), token);
    }

    public async Task<ICompletedProcess> Execute(RunProcessArgs args, CancellationToken token = new())
    {
        return await AAppService.Instance.RunProcess(GetArgs(args), token);
    }

    public async Task<ICollection<ICompletedProcess>> Execute(RunProcessArgs.ProcessRunProvider where, RunProcessArgs[] args, CancellationToken token = new())
    {
        return await AAppService.Instance.RunProcess(where, args.Select(GetArgs).ToArray(), token);
    }

    public async Task<ICollection<ICompletedProcess>> Execute(RunProcessArgs[] args, CancellationToken token = new())
    {
        return await AAppService.Instance.RunProcess(args.Select(GetArgs).ToArray(), token);
    }

    private static RunProcessArgs GetArgs(RunProcessArgs args) => new RunProcessArgs
    {
        Filename = "wsl",
        Args = $"-e {args.Filename} {args.Args}",
        WorkingDirectory = args.WorkingDirectory,
        Stdin = args.Stdin
    };
}