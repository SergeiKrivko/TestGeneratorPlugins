using System.Text.Json.Serialization;
using TestGenerator.Shared.Utils;

namespace Tests.Core;

public class TestResult
{
    public enum TestResultStatus
    {
        None,
        Success,
        Warning,
        Failed
    }
    
    public required string Name { get; init; }
    public required string Provider { get; init; }
    public required TestResultStatus Status { get; set; }

    public virtual TestResultStatus Refresh() => TestResultStatus.None;
    public string? ShortResult { get; init; }

    [JsonIgnore] public bool CanBeExpanded { get; init; } = true;

    public bool IsSuccess => Status == TestResultStatus.Success;
    public bool IsFailed => Status == TestResultStatus.Failed;
    public bool IsWarning => Status == TestResultStatus.Warning;
    public bool IsNone => Status == TestResultStatus.None;
}

public class TestResult<T> : TestResult
{
    private Test _test;

    internal TestResult(Test test)
    {
        _test = test;
    }

    public required string Key { get; init; }

    public required T? Data
    {
        get => _test.ResultsSection.Get<T>(Key);
        init => _test.ResultsSection.Set(Key, value);
    }

    public required Func<Test, T?, TestResultStatus> Validator { get; init; }

    public override TestResultStatus Refresh() => Status = Validator(_test, Data);
}