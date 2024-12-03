using Avalonia.Controls;
using TestGenerator.Shared.Utils;

namespace Tests.Core;

public interface ITestResultProvider
{
    public string Key { get; }
    public string Name { get; }
    public string DataKey { get; }

    public TestResult Load(Test test);

    public ITestResultControl? Control { get; }
}

public class TestResultProvider<T> : ITestResultProvider
{
    public required string Key { get; init; }
    public required string Name { get; init; }
    public required string DataKey { get; init; }

    public required Func<Test, T?, TestResult.TestResultStatus> Validator { get; init; }
    public Func<Test, T?, string>? ShortResultFunc { get; init; }

    public ITestResultControl? Control { get; init; }

    public TestResult<T> New(Test test, T data)
    {
        return new TestResult<T>(test)
        {
            Name = Name, Provider = Key, Key = DataKey, Data = data, Validator = Validator,
            Status = Validator(test, data), ShortResult = ShortResultFunc?.Invoke(test, data),
            CanBeExpanded = Control != null
        };
    }

    public TestResult Load(Test test)
    {
        var data = test.ResultsSection.Get<T>(DataKey);
        return new TestResult<T>(test)
        {
            Name = Name, Provider = Key, Key = DataKey, Data = data, Validator = Validator,
            Status = Validator(test, data), ShortResult = ShortResultFunc?.Invoke(test, data),
            CanBeExpanded = Control != null
        };
    }
}