using System.Collections.ObjectModel;
using System.Text.Json;
using Git.Models;
using TestGenerator.Shared.SidePrograms;
using TestGenerator.Shared.Types;

namespace Git.Services;

public class GitService
{
    private static GitService? _instance;
    public static GitService Instance => _instance ??= new GitService();


    private readonly GitGroup _modifiedGroup = new GitGroup { Name = "Изменения", Files = [] };
    private readonly GitGroup _unknownGroup = new GitGroup { Name = "Неизвестные файлы", Files = [] };
    private readonly GitGroup _ignoredGroup = new GitGroup { Name = "Игнорируемые файлы", Files = [] };

    public ObservableCollection<GitGroup> Groups { get; }

    private GitService()
    {
        Groups = [_modifiedGroup, _unknownGroup, _ignoredGroup];
    }

    public static SideProgram GitProgram { get; } = new SideProgram
    {
        Key = "Git", Validators = [new ProgramOutputValidator("--version", "git")],
        Locations = new Dictionary<string, ICollection<string>>
        {
            { "Default", ["git"] },
        }
    };

    public static SideProgramFile GetGit() => GitProgram.FromModel(Git.Settings.Get<ProgramFileModel>("git")) ??
                                              throw new Exception("git not found");

    public async Task GitStatus(string path)
    {
        var files = new List<GitFile>();

        var proc = await GetGit().Execute(new RunProgramArgs
            { Args = "status --porcelain --ignored", WorkingDirectory = path });
        foreach (var line in proc.Stdout.Split('\n').Where(l => l.Length > 2))
        {
            var status = line.Substring(0, 2);
            var filename = line.Substring(3).Trim();
            try
            {
                if (filename.StartsWith('"'))
                    filename = JsonSerializer.Deserialize<string>(filename) ?? "";
            }
            catch (JsonException)
            {
                Git.Logger.Warning($"JsonException: can not decode '{filename}'");
                continue;
            }
            filename = Path.GetFullPath(Path.Join(path, filename));
            foreach (var fullPath in File.Exists(filename) ? [filename] : Directory.GetFiles(filename))
            {
                var relativePath = Path.GetRelativePath(path, fullPath);

                var fileStatus = new GitFile
                {
                    FullPath = fullPath,
                    RelativePath = relativePath,
                    Status = GetStatusFromCode(status),
                    IsStaged = status[1] != ' '
                };

                files.Add(fileStatus);
            }
        }

        _modifiedGroup.Merge(files
            .Where(f => f.Status != GitFile.GitFileStatus.Unknown && f.Status != GitFile.GitFileStatus.Ignored)
            .ToArray());
        _unknownGroup.Merge(files
            .Where(f => f.Status == GitFile.GitFileStatus.Unknown).ToArray());
        _ignoredGroup.Merge(files
            .Where(f => f.Status == GitFile.GitFileStatus.Ignored).ToArray());
    }

    private static GitFile.GitFileStatus GetStatusFromCode(string statusCode)
    {
        return statusCode.Trim() switch
        {
            "M" => GitFile.GitFileStatus.Modified,
            "A" => GitFile.GitFileStatus.Added,
            "AM" => GitFile.GitFileStatus.Added,
            "D" => GitFile.GitFileStatus.Deleted,
            "R" => GitFile.GitFileStatus.Renamed,
            "C" => GitFile.GitFileStatus.Copied,
            "!!" => GitFile.GitFileStatus.Ignored,
            _ => GitFile.GitFileStatus.Unknown
        };
    }

    public async Task<int> GitCommit(IEnumerable<string> files, string message)
    {
        var git = GetGit();
        var proc1 = await git.Execute(new RunProgramArgs
        {
            Args = "reset",
            WorkingDirectory = AAppService.Instance.CurrentProject.Path
        });
        if (proc1.ExitCode != 0)
        {
            Git.Logger.Error(proc1.Stderr);
            return proc1.ExitCode;
        }

        var proc2 = await git.Execute(new RunProgramArgs
        {
            Args = $"add {string.Join(' ', files.Select(f => JsonSerializer.Serialize(f)))}",
            WorkingDirectory = AAppService.Instance.CurrentProject.Path
        });
        if (proc2.ExitCode != 0)
        {
            Git.Logger.Error(proc2.Stderr);
            return proc2.ExitCode;
        }

        var proc3 = await git.Execute(new RunProgramArgs
        {
            Args = $"commit -m {JsonSerializer.Serialize(message)}",
            WorkingDirectory = AAppService.Instance.CurrentProject.Path
        });
        if (proc3.ExitCode != 0)
        {
            Git.Logger.Error(proc3.Stderr);
        }

        return proc3.ExitCode;
    }
}