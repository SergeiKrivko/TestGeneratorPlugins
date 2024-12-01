using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Reactive;
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
        NameBlock.Text = Group?.Name;
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
}