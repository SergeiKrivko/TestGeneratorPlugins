using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Tests.Core;

namespace Tests.Ui;

public partial class TestEditor : UserControl
{
    private Test? _test;
    public Test? Test
    {
        get => _test;
        set
        {
            _test = value;
            Load();
        }
    }
    
    public TestEditor()
    {
        InitializeComponent();
        ExitCodeOperatorBox.ItemsSource = new ObservableCollection<string>(["?", "==", "!=", ">", ">=", "<", "<="]);
    }
    
    private void Load()
    {
        if (Test == null)
            return;
        NameBox.Text = Test.Name;
        ArgsBox.Text = Test.Args;
        ExitCodeOperatorBox.SelectedItem = Test.ExitCodeOperator;
        ExitCodeBox.Text = Test.ExitCode.ToString();
        ExitCodeBox.IsVisible = ExitCodeOperatorBox.SelectedItem as string != "?";
        StdinBox.Text = Test.Stdin;
        StdoutBox.Text = Test.Stdout;
    }

    private void NameBox_OnTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (Test == null)
            return;
        Test.Name = NameBox.Text ?? "";
    }

    private void ArgsBox_OnTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (Test == null)
            return;
        Test.Args = ArgsBox.Text ?? "";
    }

    private void ExitCodeBox_OnTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (Test == null)
            return;
        if (int.TryParse(ExitCodeBox.Text, out var res))
            Test.ExitCode = res;
        else
            Test.ExitCode = null;
    }

    private void ExitCodeOperatorBox_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (Test == null)
            return;
        Test.ExitCodeOperator = ExitCodeOperatorBox.SelectedItem as string ?? "";
        ExitCodeBox.IsVisible = ExitCodeOperatorBox.SelectedItem as string != "?";
    }

    private void StdinBox_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (Test == null)
            return;
        Test.Stdin = StdinBox.Text ?? "";
    }

    private void StdoutBox_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (Test == null)
            return;
        Test.Stdout = StdinBox.Text ?? "";
    }
}