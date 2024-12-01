using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using TestGenerator.Shared.Types;
using Tests.Core;

namespace Tests.Ui;

public partial class TestsTab : MainTab
{
    public override string TabKey => "Tests";
    public override string TabName => "Тесты";
    public override int TabPriority => 50;

    public TestsTab()
    {
        InitializeComponent();
        Tree.ItemsSource = Tests.Service.Groups;
        Init();
        AAppService.Instance.Subscribe<string>("projectChanged", path =>
        {
            var buildId = Tests.ProjectSettings.Get<Guid>("selectedBuild");
            BuildsBox.SelectedItem = _builds?.FirstOrDefault(b => b.Id == buildId);
        });
    }

    private ObservableCollection<ABuild>? _builds;

    private async void Init()
    {
        _builds = await AAppService.Instance.Request<ObservableCollection<ABuild>>("getAllBuilds");
        BuildsBox.ItemsSource = _builds;
    }

    private void AddButton_OnClick(object? sender, RoutedEventArgs e)
    {
        Tests.Service.Groups.Add(TestsGroup.New());
        Tests.Service.Save();
    }

    private void BuildsBox_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        Tests.ProjectSettings.Set("selectedBuild", (BuildsBox.SelectedItem as ABuild)?.Id);
    }

    private async void RunButton_OnClick(object? sender, RoutedEventArgs e)
    {
        await Tests.Service.Run();
    }

    private void Tree_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        GroupEditor.IsVisible = false;
        TestEditor.IsVisible = false;
        if (Tree.SelectedItem is TestsGroup testsGroup)
        {
            GroupEditor.IsVisible = true;
            GroupEditor.Group = testsGroup;
        }
        else if (Tree.SelectedItem is Test test)
        {
            TestEditor.IsVisible = true;
            TestEditor.Test = test;
        }
    }
}