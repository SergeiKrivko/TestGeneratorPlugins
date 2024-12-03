using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Tests.Core;

namespace Tests.Ui;

public partial class ResultsView : UserControl
{
    private Test? _test;
    private Dictionary<string, TestResultView> _controls = [];

    public Test? Test
    {
        get => _test;
        set
        {
            if (_test != null)
                _test.ResultsChanged -= TestOnChanged;
            _test = value;
            IsVisible = value != null;
            if (_test != null)
            {
                _test.ResultsChanged += TestOnChanged;
                ListBox.ItemsSource = _test.Results;
            }
        }
    }

    private void TestOnChanged()
    {
        if (_test?.IsChanged == Test.ChangingStatus.OutputChanges)
            Dispatcher.UIThread.Post(() =>
            {
                ListBox.ItemsSource = new ObservableCollection<TestResult>();
                ListBox.ItemsSource = _test.Results;
            });
    }

    public ResultsView()
    {
        InitializeComponent();
        foreach (var provider in Tests.Service.ResultProviders)
        {
            if (provider.Control is Control control)
            {
                var view = new TestResultView(control);
                _controls[provider.Key] = view;
                view.Closed += () => ListBox.SelectedItem = null;
                RootPanel.Children.Add(view);
            }
        }
    }

    private void ListBox_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        var item = ListBox.SelectedItem as TestResult;
        ListBox.IsVisible = item?.CanBeExpanded != true;
        foreach (var (key, value) in _controls)
        {
            value.IsVisible = key == item?.Provider;
        }
        if (item?.CanBeExpanded == true)
            _controls[item.Provider].Open(item);
    }
}