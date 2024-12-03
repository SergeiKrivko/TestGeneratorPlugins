using System.Collections.ObjectModel;
using System.Text.Json.Serialization;
using TestGenerator.Shared.Types;
using TestGenerator.Shared.Utils;

namespace Tests.Core;

public class Test
{
    public Guid Id { get; }
    private SettingsFile Settings { get; }
    internal SettingsSection ResultsSection { get; }

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
            if (value == Args)
                return;
            Settings.Set("args", value);
            SetChangingStatus(ChangingStatus.CriticalChanges);
        }
    }

    public string Stdin
    {
        get => Settings.Get<string>("stdin", "");
        set
        {
            if (value == Stdin)
                return;
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

    public ObservableCollection<TestResult> Results { get; } = [];

    private void SetChangingStatus(ChangingStatus status)
    {
        if (status > IsChanged)
        {
            IsChanged = status;
        }
        if (status == ChangingStatus.OutputChanges)
            ValidateResults();

        Changed?.Invoke();
    }

    public event Action? Changed;
    public event Action? ResultsChanged;

    private Test(AProject project, Guid id)
    {
        Id = id;
        Project = project;
        Settings = SettingsFile.Open(Path.Join(Project.Path, AProject.TestGeneratorDir, "Tests", $"{id}.xml"));
        ResultsSection = Settings.GetSection("results");
        
        foreach (var result in Settings.Get<TestResult[]>("results", []))
        {
            var provider = Tests.Service.ResultProviders.FirstOrDefault(p => p.Key == result.Provider);
            if (provider != null)
                Results.Add(provider.Load(this));
        }
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
        Results.Clear();
        Status = TestStatus.InProgress;
        var res = await build.Run(Args, Stdin);
        LastTestTime = DateTime.Now;
        IsChanged = ChangingStatus.NotChanged;

        Results.Add(TestsService.ExitCodeResultProvider.New(this, res.ExitCode));
        Results.Add(TestsService.StdoutResultProvider.New(this, res.Stdout));
        Results.Add(TestsService.StderrResultProvider.New(this, res.Stderr));
        Status = Results.All(r => r.Status != TestResult.TestResultStatus.Failed) ? TestStatus.Success : TestStatus.Failed;
        Settings.Set("results", Results.ToArray());
    }

    private void ValidateResults()
    {
        Status = Results.All(r => r.Refresh() != TestResult.TestResultStatus.Failed) ? TestStatus.Success : TestStatus.Failed;
        ResultsChanged?.Invoke();
        Settings.Set("results", Results.ToArray());
    }
}