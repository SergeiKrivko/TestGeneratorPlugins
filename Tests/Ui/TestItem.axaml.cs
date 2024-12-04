using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Reactive;
using Avalonia.Threading;
using Tests.Core;

namespace Tests.Ui;

public partial class TestItem : UserControl
{
    public static readonly StyledProperty<Test?> TestProperty =
        AvaloniaProperty.Register<TestItem, Test?>(nameof(Test));

    public event Action? CutRequested;
    public event Action? CopyRequested;
    public event Action? PasteRequested;

    public Test? Test
    {
        get => GetValue(TestProperty);
        set => SetValue(TestProperty, value);
    }

    public TestItem()
    {
        InitializeComponent();
        TestProperty.Changed.Subscribe(new AnonymousObserver<AvaloniaPropertyChangedEventArgs<Test?>>(value =>
        {
            if (value.NewValue.Value != null)
            {
                value.NewValue.Value.Changed += Update;
                Update();
            }
        }));
    }

    private void Update()
    {
        Dispatcher.UIThread.Post(() =>
        {
            NameBlock.Text = Test?.Name;
            IconSuccess.IsVisible = Test?.Status == Test.TestStatus.Success;
            IconFailed.IsVisible = Test?.Status == Test.TestStatus.Failed;
            IconInProgress.IsVisible = Test?.Status == Test.TestStatus.InProgress;
            IconUnknown.IsVisible = Test?.Status == Test.TestStatus.Unknown;
            IconTimeout.IsVisible = Test?.Status == Test.TestStatus.Timeout;
            IconCancelled.IsVisible = Test?.Status == Test.TestStatus.Canceled;
            IconWarning.IsVisible = Test?.IsChanged == Test.ChangingStatus.CriticalChanges;
        });
    }

    private void RemoveMenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        if (Test == null)
            return;
        Tests.Service.Remove(Test);
    }

    private async void RunMenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        if (Test == null)
            return;
        await Tests.Service.Run(Test);
    }

    private void CutMenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        CutRequested?.Invoke();
    }

    private void CopyMenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        CopyRequested?.Invoke();
    }

    private void PasteMenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        PasteRequested?.Invoke();
    }
}