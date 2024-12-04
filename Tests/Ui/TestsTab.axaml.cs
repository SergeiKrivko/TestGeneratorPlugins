using System.Collections.ObjectModel;
using System.Text.Json;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
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

    private void OnCutRequested()
    {
        OnCopyRequested();
        foreach (var item in Tree.SelectedItems)
        {
            if (item is Test test)
            {
                Tests.Service.Remove(test);
            }
            else if (item is TestsGroup group)
            {
                Tests.Service.Remove(group);
            }
        }
    }

    private async void OnCopyRequested()
    {
        var tests = new List<string>();
        var groups = new List<PackedTestGroup>();
        foreach (var item in Tree.SelectedItems)
        {
            if (item is Test test)
            {
                tests.Add(test.Pack());
            }
            else if (item is TestsGroup group)
            {
                groups.Add(group.Pack());
            }
        }

        var packed = new PackedTestGroup { Name = "", Groups = groups.ToArray(), Tests = tests.ToArray() };

        var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
        if (clipboard == null)
            return;
        var dataObject = new DataObject();
        dataObject.Set(DataFormats.Text, JsonSerializer.Serialize(packed));
        await clipboard.SetDataObjectAsync(dataObject);
    }

    private async void OnPasteRequested()
    {
        var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
        if (clipboard == null)
            return;
        var packedString = await clipboard.GetDataAsync(DataFormats.Text) as string;
        if (packedString == null)
            return;
        try
        {
            var packed = JsonSerializer.Deserialize<PackedTestGroup>(packedString);
            if (packed == null)
                return;
            if (Tree.SelectedItem is Test test)
            {
                Tests.Service.InsertAfter(test, packed.Tests.Select(Test.Unpack).ToArray());
            }
            else if (Tree.SelectedItem is TestsGroup group)
            {
                var i = 0;
                foreach (var t in packed.Tests.Select(Test.Unpack))
                {
                    group.Tests.Insert(i++, t);
                }

                i = 0;
                foreach (var t in packed.Groups.Select(TestsGroup.Unpack))
                {
                    group.Groups.Insert(i++, t);
                }
            }
        }
        catch (JsonException)
        {
        }
    }

    private void Tree_OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.C && (e.KeyModifiers & KeyModifiers.Control) != 0)
        {
            OnCopyRequested();
        }
        else if (e.Key == Key.X && (e.KeyModifiers & KeyModifiers.Control) != 0)
        {
            OnCutRequested();
        }
        else if (e.Key == Key.V && (e.KeyModifiers & KeyModifiers.Control) != 0)
        {
            OnPasteRequested();
        }
        else if (e.Key == Key.Delete)
        {
            var lst = new List<object>();
            foreach (var item in Tree.SelectedItems)
            {
                lst.Add(item);
            }
            foreach (var item in lst)
            {
                switch (item)
                {
                    case Test test:
                        Tests.Service.Remove(test);
                        break;
                    case TestsGroup group:
                        Tests.Service.Remove(group);
                        break;
                }
            }
        }
    }
}