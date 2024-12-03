using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Tests.Core;

namespace Tests.Ui;

public partial class TestResultView : UserControl
{
    private ITestResultControl? _testResultControl;
    
    public TestResultView(Control child)
    {
        InitializeComponent();
        Console.WriteLine(child);
        ChildPanel.Children.Add(child);
        _testResultControl = child as ITestResultControl;
    }

    public event Action? Closed;

    public void Open(TestResult result)
    {
        IconSuccess.IsVisible = result.Status == TestResult.TestResultStatus.Success;
        IconFailed.IsVisible = result.Status == TestResult.TestResultStatus.Failed;
        IconWarning.IsVisible = result.Status == TestResult.TestResultStatus.Warning;
        IconNone.IsVisible = result.Status == TestResult.TestResultStatus.None;
        NameBlock.Text = result.Name;
        _testResultControl?.Open(result);
    }

    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        Closed?.Invoke();
    }
}