using System.Collections.ObjectModel;
using System.Text.Json;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using TestGenerator.Shared.Types;
using TestGenerator.Shared.Utils;

namespace Tests.Core;

public class TestsService
{
    public ObservableCollection<TestsGroup> Groups { get; } = [];
    private SettingsSection _projectData;

    public ObservableCollection<ITextComparator> TextComparators { get; } =
        [new TextComparator(), new WordsComparator()];

    public TestsService()
    {
        _projectData = Tests.ProjectData;
        AAppService.Instance.Subscribe<string>("projectChanged", path => Load());
    }

    public void Save()
    {
        _projectData.Set("testGroups", Groups);
    }

    private void Load()
    {
        _projectData = Tests.ProjectData;
        Groups.Clear();
        foreach (var testsGroup in _projectData.Get<TestsGroup[]>("testGroups", []))
        {
            Groups.Add(testsGroup);
        }
    }

    public async Task Run()
    {
        var build = await AAppService.Instance.Request<ABuild?>("getBuild",
            Tests.ProjectSettings.Get<Guid>("selectedBuild"));
        if (build == null)
            return;

        foreach (var group in Groups)
        {
            group.Status = Test.TestStatus.InProgress;
        }

        async Task<int> TestingBackgroundFunc(IBackgroundTask task)
        {
            var totalCount = Groups.Sum(g => g.Count);
            var i = 0;
            foreach (var group in Groups)
            {
                await foreach (var test in group.Run(build))
                {
                    i++;
                    task.Status = test.Name;
                    task.Progress = i * 100.0 / totalCount;
                }
            }

            return 0;
        }

        await build.RunPreProcConsole();

        await AAppService.Instance.RunBackgroundTask("Тестирование", TestingBackgroundFunc).Wait();

        await build.RunPostProcConsole();
    }

    public static TestResultProvider<int> ExitCodeResultProvider { get; } = new()
    {
        Key = "ExitCode",
        Name = "Код возврата",
        DataKey = "exitCode",
        ShortResultFunc = (test, i) => i.ToString(),
        Validator = (test, code) =>
        {
            switch (test.ExitCodeOperator)
            {
                case "==":
                    return code == test.ExitCode ? TestResult.TestResultStatus.Success : TestResult.TestResultStatus.Failed;
                case "!=":
                    return code != test.ExitCode ? TestResult.TestResultStatus.Success : TestResult.TestResultStatus.Failed;
                case ">":
                    return code > test.ExitCode ? TestResult.TestResultStatus.Success : TestResult.TestResultStatus.Failed;
                case ">=":
                    return code >= test.ExitCode ? TestResult.TestResultStatus.Success : TestResult.TestResultStatus.Failed;
                case "<":
                    return code < test.ExitCode ? TestResult.TestResultStatus.Success : TestResult.TestResultStatus.Failed;
                case "<=":
                    return code <= test.ExitCode ? TestResult.TestResultStatus.Success : TestResult.TestResultStatus.Failed;
                default:
                    return TestResult.TestResultStatus.Success;
            }
        }
    };

    private class TextResultControl : TextBox, ITestResultControl
    {
        protected override Type StyleKeyOverride => typeof(TextBox);

        public TextResultControl()
        {
            BorderThickness = new Thickness(0);
            IsReadOnly = true;
            AcceptsReturn = true;
        }

        public void Open(TestResult testResult)
        {
            if (testResult is TestResult<string> result)
            {
                IsVisible = true;
                Text = result.Data;
            }
        }
    }

    public static TestResultProvider<string> StdoutResultProvider { get; } = new()
    {
        Key = "Stdout",
        Name = "Stdout",
        DataKey = "stdout",
        Control = new TextResultControl(),
        Validator = (test, stdout) => new WordsComparator().Compare(test.Stdout, stdout ?? "")
            ? TestResult.TestResultStatus.Success
            : TestResult.TestResultStatus.Failed
    };

    public static TestResultProvider<string> StderrResultProvider { get; } = new()
    {
        Key = "Stderr",
        Name = "Stderr",
        DataKey = "stderr",
        Control = new TextResultControl(),
        Validator = (test, stdout) => TestResult.TestResultStatus.None
    };

    public ObservableCollection<ITestResultProvider> ResultProviders { get; } =
        [ExitCodeResultProvider, StdoutResultProvider, StderrResultProvider];
}