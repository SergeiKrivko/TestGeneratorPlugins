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

    public async Task<ICompletedProcess> Execute(string filename, string args)
    {
        return await AAppService.Instance.RunProcess("wsl", $"-e {filename} {args}");
    }

    public async Task<ICompletedProcess> Execute(string command)
    {
        return await AAppService.Instance.RunProcess("wsl", $"-e {command}");
    }

    public ITerminalController ExecuteInConsole(string command, string? workingDirectory = null)
    {
        return AAppService.Instance.RunInConsole($"wsl -e {command}", workingDirectory);
    }
}