using TestGenerator.Shared.Types;
using TestGenerator.Shared.Utils;

namespace Tests.Core;

public class Test
{
    public Guid Id { get; }
    private SettingsFile Settings { get; }
    private SettingsSection Results { get; }

    private AProject Project { get; }

    public string Name
    {
        get => Settings.Get<string>("name", "");
        set
        {
            Settings.Set("name", value);
            SetChangingStatus(ChangingStatus.CosmeticChanges);
        }
    }

    public string Args
    {
        get => Settings.Get<string>("args", "");
        set
        {
            Settings.Set("args", value);
            SetChangingStatus(ChangingStatus.CriticalChanges);
        }
    }

    public string Stdin
    {
        get => Settings.Get<string>("stdin", "");
        set
        {
            Settings.Set("stdin", value);
            SetChangingStatus(ChangingStatus.CriticalChanges);
        }
    }

    public string Stdout
    {
        get => Settings.Get<string>("stdout", "");
        set
        {
            Settings.Set("stdout", value);
            SetChangingStatus(ChangingStatus.OutputChanges);
        }
    }

    public string ExitCodeOperator
    {
        get => Settings.Get<string>("exitCodeOperator", "?");
        set
        {
            Settings.Set("exitCodeOperator", value);
            SetChangingStatus(ChangingStatus.OutputChanges);
        }
    }

    public int? ExitCode
    {
        get => Settings.Get<int?>("exitCode");
        set
        {
            Settings.Set("exitCode", value);
            SetChangingStatus(ChangingStatus.OutputChanges);
        }
    }

    public DateTime LastTestTime
    {
        get => Settings.Get<DateTime>("lastTestTime");
        set => Settings.Set("lastTestTime", value);
    }

    public ChangingStatus IsChanged
    {
        get => Settings.Get("isChanged", ChangingStatus.NotChanged);
        set => Settings.Set("isChanged", value);
    }

    public enum ChangingStatus
    {
        NotChanged,
        CosmeticChanges,
        OutputChanges,
        CriticalChanges
    }

    public enum TestStatus
    {
        Unknown,
        InProgress,
        Success,
        Failed,
        Timeout,
        Canceled
    }

    public TestStatus Status
    {
        get => Settings.Get<TestStatus>("status");
        set
        {
            Settings.Set("status", value);
            Changed?.Invoke();
        }
    }

    public int ResExitCode
    {
        get => Results.Get<int>("exitCode");
        set => Results.Set("exitCode", value);
    }

    public string ResStdout
    {
        get => Results.Get<string>("stdout", "");
        set => Results.Set("stdout", value);
    }

    public string ResStderr
    {
        get => Results.Get<string>("stderr", "");
        set => Results.Set("stderr", value);
    }

    public void SetChangingStatus(ChangingStatus status)
    {
        if (status > IsChanged)
        {
            IsChanged = status;
        }

        Changed?.Invoke();
    }

    public event Action? Changed;

    private Test(AProject project, Guid id)
    {
        Id = id;
        Project = project;
        Settings = SettingsFile.Open(Path.Join(Project.Path, AProject.TestGeneratorDir, "Tests", $"{id}.xml"));
        Results = Settings.GetSection("results");
    }

    public static Test FromFile(string path)
    {
        var filename = Path.GetFileNameWithoutExtension(path);
        return new Test(AAppService.Instance.CurrentProject, Guid.Parse(filename));
    }

    public static Test Load(Guid id)
    {
        return new Test(AAppService.Instance.CurrentProject, id);
    }

    public static Test New()
    {
        return new Test(AAppService.Instance.CurrentProject, Guid.NewGuid()) { Name = "-" };
    }

    public async Task Run(ABuild build)
    {
        Status = TestStatus.InProgress;
        var res = await build.Run(Args, Stdin);
        ResExitCode = res.ExitCode;
        ResStdout = res.Stdout;
        ResStderr = res.Stderr;
        Status = CheckOutput() ? TestStatus.Success : TestStatus.Failed;
    }

    private bool CheckExitCode()
    {
        switch (ExitCodeOperator)
        {
            case "==":
                return ResExitCode == ExitCode;
            case "!=":
                return ResExitCode != ExitCode;
            case ">":
                return ResExitCode > ExitCode;
            case ">=":
                return ResExitCode >= ExitCode;
            case "<":
                return ResExitCode < ExitCode;
            case "<=":
                return ResExitCode <= ExitCode;
            default:
                return true;
        }
    }

    private bool CheckOutput()
    {
        return CheckExitCode();
    }
}