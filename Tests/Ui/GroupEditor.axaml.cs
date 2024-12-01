using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Tests.Core;

namespace Tests.Ui;

public partial class GroupEditor : UserControl
{
    private TestsGroup? _group;
    public TestsGroup? Group
    {
        get => _group;
        set
        {
            _group = value;
            Load();
        }
    }
    
    public GroupEditor()
    {
        InitializeComponent();
    }

    private void Load()
    {
        if (Group == null)
            return;
        NameBox.Text = Group.Name;
    }

    private void NameBox_OnTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (Group == null)
            return;
        Group.Name = NameBox.Text ?? "";
        Tests.Service.Save();
    }
}