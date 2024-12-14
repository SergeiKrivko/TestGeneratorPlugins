using System.Collections.ObjectModel;
using System.Text.Json;
using Git.Models;
using TestGenerator.Shared.SidePrograms;
using TestGenerator.Shared.Types;

namespace Git.Services;

public class BranchService
{
    private static BranchService? _instance;
    public static BranchService Instance => _instance ??= new BranchService();
    private static SideProgramFile? GetGit() => GitService.GetGit();

    private readonly GitBranchGroup _localGroup = new GitBranchGroup { Name = "Local" };
    private readonly GitBranchGroup _remoteGroup = new GitBranchGroup { Name = "Remote" };
    public ObservableCollection<GitBranchGroup> Groups { get; }

    private BranchService()
    {
        Groups = [_localGroup, _remoteGroup];
    }

    private async Task<int> RefreshLocalBranches()
    {
        var git = GetGit();
        if (git == null)
            return -1;
        var proc = await git.Execute(new RunProgramArgs
            { Args = "branch", WorkingDirectory = AAppService.Instance.CurrentProject.Path });
        if (proc.ExitCode != 0)
        {
            Git.Logger.Error(proc.Stderr);
            return proc.ExitCode;
        }

        foreach (var line in proc.Stdout.Split('\n'))
        {
            if (line.Length < 3)
                continue;
            var branchName = line.Substring(2).Trim();
            try
            {
                if (branchName.StartsWith('"'))
                    branchName = JsonSerializer.Deserialize<string>(branchName) ?? "";
            }
            catch (JsonException)
            {
                // Git.Logger.Warning($"JsonException: can not decode '{branchName}'");
                continue;
            }

            _localGroup.Add(new GitBranch { Name = branchName, IsCurrent = line.StartsWith('*') });
        }

        return 0;
    }

    private async Task<int> RefreshRemoteBranches()
    {
        var git = GetGit();
        if (git == null)
            return -1;
        var proc = await git.Execute(new RunProgramArgs
            { Args = "branch --remotes", WorkingDirectory = AAppService.Instance.CurrentProject.Path });
        if (proc.ExitCode != 0)
        {
            Git.Logger.Error(proc.Stderr);
            return proc.ExitCode;
        }

        foreach (var line in proc.Stdout.Split('\n'))
        {
            if (line.Length < 3)
                continue;
            var branchName = line.Substring(2).Trim();
            try
            {
                if (branchName.StartsWith('"'))
                    branchName = JsonSerializer.Deserialize<string>(branchName) ?? "";
            }
            catch (JsonException)
            {
                // Git.Logger.Warning($"JsonException: can not decode '{branchName}'");
                continue;
            }

            _remoteGroup.Add(new GitBranch { Name = branchName, IsCurrent = line.StartsWith('*') });
        }

        return 0;
    }

    public GitBranch? Current { get; private set; }
    public event Action<GitBranch?>? CurrentChanged;

    public async Task Refresh()
    {
        var code = await RefreshLocalBranches();
        if (code != 0)
            return;
        code = await RefreshRemoteBranches();
        if (code != 0)
            return;

        var current = Groups.Select(g => g.CurrentBranch).FirstOrDefault(g => g != null);
        if (current != Current)
        {
            Current = current;
            CurrentChanged?.Invoke(current);
        }
    }
}