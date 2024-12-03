using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Reactive;
using Avalonia.Threading;
using Tests.Core;

namespace Tests.Ui;

public partial class GroupItem : UserControl
{
    public static readonly StyledProperty<TestsGroup?> GroupProperty =
        AvaloniaProperty.Register<GroupItem, TestsGroup?>(nameof(Group));

    public TestsGroup? Group
    {
        get => GetValue(GroupProperty);
        set => SetValue(GroupProperty, value);
    }

    public GroupItem()
    {
        InitializeComponent();
        GroupProperty.Changed.Subscribe(new AnonymousObserver<AvaloniaPropertyChangedEventArgs<TestsGroup?>>(value =>
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
            NameBlock.Text = Group?.Name;
            UpdateCount();
        });
    }

    private void NewTest()
    {
        if (Group == null)
            return;
        Group.Tests.Add(Test.New());
        Tests.Service.Save();
    }

    private void AddButton_OnClick(object? sender, RoutedEventArgs e)
    {
        NewTest();
    }

    private void RemoveMenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        if (Group == null)
            return;
        Tests.Service.Remove(Group);
    }

    private void AddGroupMenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        Group?.Groups.Add(TestsGroup.New());
    }

    private void AddTestMenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        Group?.Tests.Add(Test.New());
    }

    private void UpdateCount()
    {
        var total = Group?.Count();
        var success = Group?.Count(Test.TestStatus.Success);
        var failed = Group?.Count(Test.TestStatus.Failed);
        TestsCountBlock.Text = TestsCountBlockSuccess.Text = TestsCountBlockFailed.Text = $"{success} / {total}";
        TestsCountBlock.IsVisible = failed == 0 && success < total;
        TestsCountBlockFailed.IsVisible = failed > 0;
        TestsCountBlockSuccess.IsVisible = total > 0 && failed == 0 && success == total;
    }

    private async void RunMenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        if (Group == null)
            return;
        await Tests.Service.Run(Group);
    }
}