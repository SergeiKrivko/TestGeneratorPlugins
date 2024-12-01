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
        });
    }
}